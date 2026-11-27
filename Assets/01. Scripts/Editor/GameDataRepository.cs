using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefense.Data;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.EditorTools
{
    /// <summary>데이터 종류 하나. 같은 종류에 속하는 만들 수 있는 클래스 목록을 가진다.</summary>
    public sealed class GameDataCategory
    {
        public GameDataCategory(Type type, GameDataCategoryAttribute attribute, IReadOnlyList<Type> creatableTypes)
        {
            Type = type;
            Label = attribute.Label;
            Folder = attribute.Folder;
            Order = attribute.Order;
            CreatableTypes = creatableTypes;
        }

        public Type Type { get; }
        public string Label { get; }
        public string Folder { get; }
        public int Order { get; }
        public IReadOnlyList<Type> CreatableTypes { get; }
    }

    /// <summary>
    /// 게임 데이터 에셋을 찾고 만들고 지우는 곳. 화면과 분리해서, 나중에 CSV 연동도 이 기능을 쓸 수 있게 한다.
    /// </summary>
    public static class GameDataRepository
    {
        public const string RootFolder = "Assets/02. Datas";

        public static List<GameDataCategory> GetCategories()
        {
            var categories = new List<GameDataCategory>();
            foreach (Type type in TypeCache.GetTypesWithAttribute<GameDataCategoryAttribute>())
            {
                if (!typeof(GameData).IsAssignableFrom(type)) continue;

                var attribute = (GameDataCategoryAttribute)Attribute.GetCustomAttribute(type, typeof(GameDataCategoryAttribute), false);
                List<Type> creatable = TypeCache.GetTypesDerivedFrom(type)
                    .Prepend(type)
                    .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                    .OrderBy(t => t.Name)
                    .ToList();
                categories.Add(new GameDataCategory(type, attribute, creatable));
            }
            return categories.OrderBy(c => c.Order).ThenBy(c => c.Label).ToList();
        }

        /// <summary>이 종류의 모든 에셋. 폴더 위치와 관계없이 찾는다.</summary>
        public static List<GameData> FindAll(GameDataCategory category)
        {
            return AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Select(AssetDatabase.LoadAssetAtPath<GameData>)
                .Where(data => data != null && category.Type.IsInstanceOfType(data))
                .OrderBy(data => data.name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static GameData Create(GameDataCategory category, Type type, string assetName)
        {
            if (!category.CreatableTypes.Contains(type))
            {
                throw new ArgumentException($"{category.Label}에서 만들 수 없는 클래스입니다: {type.Name}");
            }

            string folder = EnsureFolder($"{RootFolder}/{category.Folder}");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{assetName}.asset");
            var data = (GameData)ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            return data;
        }

        /// <summary>같은 폴더에 복사본을 만든다.</summary>
        public static GameData Duplicate(GameData source)
        {
            // 저장하지 않은 수정 내용도 복사본에 담기게 한다
            AssetDatabase.SaveAssetIfDirty(source);

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string path = AssetDatabase.GenerateUniqueAssetPath(sourcePath);
            return AssetDatabase.CopyAsset(sourcePath, path) ? AssetDatabase.LoadAssetAtPath<GameData>(path) : null;
        }

        /// <summary>에셋 이름을 바꾼다. 실패하면 이유를 돌려주고, 성공하면 빈 문자열이다.</summary>
        public static string Rename(GameData data, string newName)
        {
            newName = newName == null ? string.Empty : newName.Trim();
            if (newName.Length == 0) return "이름이 비어 있습니다.";
            if (newName == data.name) return string.Empty;

            string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(data), newName);
            if (string.IsNullOrEmpty(error)) AssetDatabase.SaveAssets();
            return error;
        }

        /// <summary>
        /// 이 데이터를 직접 사용하는 씬·프리팹·에셋의 경로. 저장된 상태를 기준으로 한다.
        /// </summary>
        public static List<string> FindReferences(GameData data)
        {
            string target = AssetDatabase.GetAssetPath(data);
            string[] extensions = { ".unity", ".prefab", ".asset" };

            return AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) && path != target)
                .Where(path => extensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
                .Where(path => AssetDatabase.GetDependencies(path, false).Contains(target))
                .OrderBy(path => path)
                .ToList();
        }

        /// <summary>휴지통으로 보낸다. 실수로 지워도 되살릴 수 있다.</summary>
        public static bool Delete(GameData data)
        {
            return AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(data));
        }

        private static string EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return path;

            int cut = path.LastIndexOf('/');
            string parent = EnsureFolder(path.Substring(0, cut));
            AssetDatabase.CreateFolder(parent, path.Substring(cut + 1));
            return path;
        }
    }
}
