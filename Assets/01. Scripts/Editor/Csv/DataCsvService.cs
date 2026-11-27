using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TowerDefense.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TowerDefense.EditorTools
{
    /// <summary>가져오기에서 바뀌는 칸 하나.</summary>
    public sealed class CsvChange
    {
        public CsvChange(string field, string before, string after)
        {
            Field = field;
            Before = before;
            After = after;
        }

        public string Field { get; }
        public string Before { get; }
        public string After { get; }
    }

    /// <summary>가져오기에서 에셋 하나에 할 일.</summary>
    public sealed class CsvEntry
    {
        /// <summary>새로 만드는 경우에는 null.</summary>
        public GameData Target;
        public Type Type;
        public string AssetName;
        public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
        public readonly List<CsvChange> Changes = new List<CsvChange>();

        public bool IsNew => Target == null;
    }

    /// <summary>적용하기 전에 보여주는 가져오기 계획.</summary>
    public sealed class CsvImportPlan
    {
        public GameDataCategory Category;
        public readonly List<CsvEntry> Entries = new List<CsvEntry>();
        public readonly List<string> Errors = new List<string>();
        /// <summary>CSV에 없는 에셋. 삭제하지 않고 알려주기만 한다.</summary>
        public readonly List<string> NotInCsv = new List<string>();

        public bool CanApply => Errors.Count == 0 && Entries.Count > 0;
    }

    /// <summary>
    /// 데이터와 CSV 사이의 내보내기·가져오기. 원본은 ScriptableObject이며, CSV로는 편집과 추가만 하고 삭제하지 않는다.
    /// id 열(에셋 GUID)로 행을 찾으므로 에셋 이름을 바꿔도 같은 데이터로 인식한다. id가 비어 있는 행은 새로 만든다.
    /// </summary>
    public static class DataCsvService
    {
        private const string IdColumn = "id";
        private const string NameColumn = "assetName";
        private const string TypeColumn = "type";

        public static string Export(GameDataCategory category)
        {
            List<GameData> all = GameDataRepository.FindAll(category);
            List<string> fields = GetColumns(category);

            var rows = new List<IReadOnlyList<string>>
            {
                new[] { IdColumn, NameColumn, TypeColumn }.Concat(fields).ToList()
            };
            foreach (GameData data in all)
            {
                var serialized = new SerializedObject(data);
                var row = new List<string> { GetId(data), data.name, data.GetType().Name };
                foreach (string field in fields)
                {
                    SerializedProperty property = serialized.FindProperty(field);
                    row.Add(property != null ? DataCsvCodec.Encode(property) : string.Empty);
                }
                rows.Add(row);
            }
            return CsvText.Write(rows);
        }

        public static void ExportToFile(GameDataCategory category, string path)
        {
            // 엑셀에서 한글이 깨지지 않도록 BOM을 붙인다
            File.WriteAllText(path, Export(category), new UTF8Encoding(true));
        }

        public static CsvImportPlan PlanFromFile(GameDataCategory category, string path)
        {
            try
            {
                // 엑셀에서 열어 둔 파일도 읽을 수 있게 공유 모드로 연다
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream, new UTF8Encoding(false, true)))
                {
                    return Plan(category, reader.ReadToEnd());
                }
            }
            catch (DecoderFallbackException)
            {
                return Failed(category, "UTF-8 파일이 아닙니다. 엑셀에서는 'CSV UTF-8' 형식으로 저장하세요.");
            }
            catch (IOException e)
            {
                return Failed(category, "파일을 읽을 수 없습니다. " + e.Message);
            }
        }

        /// <summary>CSV를 검사해서 할 일을 만든다. 에셋은 바꾸지 않는다.</summary>
        public static CsvImportPlan Plan(GameDataCategory category, string csv)
        {
            List<List<string>> rows;
            try
            {
                rows = CsvText.Parse(csv);
            }
            catch (FormatException e)
            {
                return Failed(category, e.Message);
            }
            if (rows.Count == 0) return Failed(category, "내용이 없습니다.");

            var plan = new CsvImportPlan { Category = category };
            List<string> header = rows[0].Select(h => h.Trim()).ToList();
            List<string> fields = GetColumns(category);
            CheckHeader(header, fields, plan);
            if (plan.Errors.Count > 0) return plan;

            Dictionary<string, GameData> existing = GameDataRepository.FindAll(category).ToDictionary(GetId);
            var seenIds = new HashSet<string>();
            var newPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                if (row.All(string.IsNullOrWhiteSpace)) continue;

                string line = $"{r + 1}행";
                if (row.Count > header.Count && row.Skip(header.Count).Any(c => c.Length > 0))
                {
                    plan.Errors.Add($"{line}: 열 수가 제목 행보다 많습니다.");
                    continue;
                }

                string Cell(string column)
                {
                    int index = header.IndexOf(column);
                    return index >= 0 && index < row.Count ? row[index] : string.Empty;
                }

                var entry = new CsvEntry { AssetName = Cell(NameColumn).Trim() };
                string id = Cell(IdColumn).Trim();
                string typeName = Cell(TypeColumn).Trim();

                if (id.Length > 0)
                {
                    if (!existing.TryGetValue(id, out entry.Target))
                    {
                        plan.Errors.Add($"{line}: id에 해당하는 데이터가 없습니다. 새 데이터라면 id를 비우세요. ({id})");
                        continue;
                    }
                    if (!seenIds.Add(id))
                    {
                        plan.Errors.Add($"{line}: 같은 id가 두 번 있습니다. ({id})");
                        continue;
                    }
                    entry.Type = entry.Target.GetType();
                    if (typeName.Length > 0 && typeName != entry.Type.Name)
                    {
                        plan.Errors.Add($"{line}: 기존 데이터의 type은 바꿀 수 없습니다. ({entry.Type.Name} → {typeName})");
                        continue;
                    }
                    if (entry.AssetName.Length == 0) entry.AssetName = entry.Target.name;
                }
                else
                {
                    entry.Type = typeName.Length == 0 && category.CreatableTypes.Count == 1
                        ? category.CreatableTypes[0]
                        : category.CreatableTypes.FirstOrDefault(t => t.Name == typeName);
                    if (entry.Type == null)
                    {
                        string names = string.Join(", ", category.CreatableTypes.Select(t => t.Name));
                        plan.Errors.Add($"{line}: type에 {names} 중 하나를 적으세요.");
                        continue;
                    }
                }

                if (!CheckName(entry, category, newPaths, line, plan)) continue;
                // 제목 행에 없는 필드는 바꾸지 않는다
                if (!ReadValues(entry, fields.Where(header.Contains), Cell, line, plan)) continue;

                if (entry.IsNew || entry.Changes.Count > 0) plan.Entries.Add(entry);
            }

            plan.NotInCsv.AddRange(existing.Where(pair => !seenIds.Contains(pair.Key)).Select(pair => pair.Value.name));
            return plan;
        }

        /// <summary>계획을 적용한다. 오류가 있는 계획은 아무것도 바꾸지 않는다. 기존 데이터의 값 변경은 실행 취소할 수 있다.</summary>
        public static void Apply(CsvImportPlan plan)
        {
            if (plan.Errors.Count > 0) throw new InvalidOperationException("오류가 있어 적용할 수 없습니다.");

            foreach (CsvEntry entry in plan.Entries)
            {
                GameData data = entry.IsNew
                    ? GameDataRepository.Create(plan.Category, entry.Type, entry.AssetName)
                    : entry.Target;
                if (data == null) continue;

                var serialized = new SerializedObject(data);
                foreach (KeyValuePair<string, string> value in entry.Values)
                {
                    DataCsvCodec.Decode(serialized.FindProperty(value.Key), data.GetType(), value.Value);
                }
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(data);

                if (!entry.IsNew && entry.AssetName != data.name)
                {
                    string error = GameDataRepository.Rename(data, entry.AssetName);
                    if (!string.IsNullOrEmpty(error)) Debug.LogError($"{data.name} 이름 변경 실패: {error}", data);
                }
            }
            AssetDatabase.SaveAssets();
        }

        private static CsvImportPlan Failed(GameDataCategory category, string error)
        {
            var plan = new CsvImportPlan { Category = category };
            plan.Errors.Add(error);
            return plan;
        }

        private static string GetId(GameData data)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));
        }

        /// <summary>이 종류의 모든 클래스가 가진 필드.</summary>
        private static List<string> GetColumns(GameDataCategory category)
        {
            var fields = new List<string>();
            foreach (Type type in category.CreatableTypes)
            {
                ScriptableObject sample = ScriptableObject.CreateInstance(type);
                fields.AddRange(DataCsvCodec.GetFieldNames(new SerializedObject(sample)).Except(fields).ToList());
                Object.DestroyImmediate(sample);
            }
            return fields;
        }

        private static void CheckHeader(List<string> header, List<string> fields, CsvImportPlan plan)
        {
            if (!header.Contains(IdColumn) || !header.Contains(NameColumn))
            {
                plan.Errors.Add($"제목 행에 {IdColumn}, {NameColumn} 열이 있어야 합니다.");
            }

            foreach (IGrouping<string, string> group in header.Where(h => h.Length > 0).GroupBy(h => h))
            {
                if (group.Count() > 1) plan.Errors.Add($"같은 열이 두 번 있습니다: {group.Key}");

                // #으로 시작하는 열은 메모로 보고 무시한다
                bool known = group.Key == IdColumn || group.Key == NameColumn || group.Key == TypeColumn
                             || group.Key.StartsWith("#", StringComparison.Ordinal) || fields.Contains(group.Key);
                if (!known) plan.Errors.Add($"알 수 없는 열입니다: {group.Key}");
            }
        }

        private static bool CheckName(CsvEntry entry, GameDataCategory category, HashSet<string> newPaths,
            string line, CsvImportPlan plan)
        {
            string name = entry.AssetName;
            if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                plan.Errors.Add($"{line}: assetName이 비었거나 파일 이름에 쓸 수 없는 글자가 있습니다. ({name})");
                return false;
            }
            if (!entry.IsNew && name == entry.Target.name) return true;

            string folder = entry.IsNew
                ? $"{GameDataRepository.RootFolder}/{category.Folder}"
                : Path.GetDirectoryName(AssetDatabase.GetAssetPath(entry.Target)).Replace('\\', '/');
            string path = $"{folder}/{name}.asset";
            // 대소문자만 바꾸는 이름 변경은 같은 파일이므로 허용한다
            bool sameFile = !entry.IsNew && string.Equals(name, entry.Target.name, StringComparison.OrdinalIgnoreCase);
            if (!newPaths.Add(path) || (!sameFile && File.Exists(path)))
            {
                plan.Errors.Add($"{line}: 같은 이름의 에셋이 이미 있습니다. ({path})");
                return false;
            }
            if (!entry.IsNew) entry.Changes.Add(new CsvChange(NameColumn, entry.Target.name, name));
            return true;
        }

        // 임시 복사본에 값을 넣어 검사하고, 실제로 달라지는 칸만 변경 목록에 담는다
        private static bool ReadValues(CsvEntry entry, IEnumerable<string> fields, Func<string, string> cell,
            string line, CsvImportPlan plan)
        {
            ScriptableObject before = entry.IsNew ? ScriptableObject.CreateInstance(entry.Type) : Object.Instantiate(entry.Target);
            ScriptableObject after = Object.Instantiate(before);
            bool valid = true;
            try
            {
                var original = new SerializedObject(before);
                var edited = new SerializedObject(after);
                foreach (string field in fields)
                {
                    string text = cell(field);
                    SerializedProperty property = edited.FindProperty(field);
                    if (property == null)
                    {
                        if (text.Length == 0) continue;
                        plan.Errors.Add($"{line} {field}: {entry.Type.Name}에는 없는 항목입니다.");
                        valid = false;
                        continue;
                    }
                    // 숫자 같은 필드의 빈 칸은 "바꾸지 않음"이다
                    if (text.Length == 0 && !DataCsvCodec.AcceptsEmpty(property)) continue;

                    try
                    {
                        DataCsvCodec.Decode(property, entry.Type, text);
                    }
                    catch (CsvValueException e)
                    {
                        plan.Errors.Add($"{line} {field}: {e.Message}");
                        valid = false;
                        continue;
                    }

                    string oldValue = DataCsvCodec.Encode(original.FindProperty(field));
                    string newValue = DataCsvCodec.Encode(property);
                    if (oldValue == newValue) continue;

                    entry.Values[field] = text;
                    entry.Changes.Add(new CsvChange(field, oldValue, newValue));
                }
            }
            finally
            {
                Object.DestroyImmediate(before);
                Object.DestroyImmediate(after);
            }
            return valid;
        }
    }
}
