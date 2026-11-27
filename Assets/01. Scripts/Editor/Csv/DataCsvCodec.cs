using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.EditorTools
{
    /// <summary>CSV 칸의 값이 잘못된 경우.</summary>
    public class CsvValueException : Exception
    {
        public CsvValueException(string message) : base(message) { }
    }

    /// <summary>
    /// 직렬화된 필드 값과 CSV 칸 문자열 사이의 변환. 데이터 클래스마다 코드를 쓰지 않아도 되도록 필드를 자동으로 읽는다.
    /// 숫자·글자·열거형은 그대로, 다른 에셋 참조는 에셋 경로로, 목록·묶음·벡터는 JSON으로 적는다.
    /// </summary>
    public static class DataCsvCodec
    {
        /// <summary>CSV의 열이 되는 최상위 필드 이름. 인스펙터에 보이는 순서와 같다.</summary>
        public static List<string> GetFieldNames(SerializedObject serialized)
        {
            var names = new List<string>();
            SerializedProperty property = serialized.GetIterator();
            for (bool next = property.NextVisible(true); next; next = property.NextVisible(false))
            {
                if (property.name != "m_Script") names.Add(property.name);
            }
            return names;
        }

        public static string Encode(SerializedProperty property)
        {
            JToken token = ToToken(property);
            if (token is JValue value)
            {
                if (value.Type == JTokenType.Boolean) return (bool)value ? "true" : "false";
                return Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            return token.ToString(Formatting.None);
        }

        /// <summary>빈 칸을 "빈 값"으로 받아들이는 필드인지 여부. 그 밖의 필드는 빈 칸이면 바꾸지 않는다.</summary>
        public static bool AcceptsEmpty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.String
                   || property.propertyType == SerializedPropertyType.ObjectReference;
        }

        /// <summary>칸의 문자열을 필드에 넣는다. 값이 잘못되면 CsvValueException.</summary>
        public static void Decode(SerializedProperty property, Type ownerType, string text)
        {
            FieldInfo field = FindField(ownerType, property.name);
            if (field == null) throw new CsvValueException("필드를 찾을 수 없습니다.");

            JToken token;
            if (IsComposite(property))
            {
                try
                {
                    token = JToken.Parse(text);
                }
                catch (JsonException e)
                {
                    throw new CsvValueException("JSON 형식이 잘못되었습니다. " + e.Message);
                }
            }
            else
            {
                token = new JValue(text);
            }
            Apply(property, field.FieldType, field, token);
        }

        private static bool IsComposite(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String) return false;
            if (property.isArray || property.propertyType == SerializedPropertyType.Generic) return true;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.Color:
                    return true;
                default:
                    return false;
            }
        }

        private static JToken ToToken(SerializedProperty property)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                var array = new JArray();
                for (int i = 0; i < property.arraySize; i++) array.Add(ToToken(property.GetArrayElementAtIndex(i)));
                return array;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    return new JValue(property.longValue);
                case SerializedPropertyType.Boolean:
                    return new JValue(property.boolValue);
                case SerializedPropertyType.Float:
                    return property.numericType == SerializedPropertyNumericType.Double
                        ? new JValue(property.doubleValue)
                        : new JValue(property.floatValue);
                case SerializedPropertyType.String:
                    return new JValue(property.stringValue);
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0
                        ? new JValue(property.enumNames[property.enumValueIndex])
                        : new JValue(property.intValue);
                case SerializedPropertyType.ObjectReference:
                    return new JValue(ReferenceToText(property.objectReferenceValue));
                case SerializedPropertyType.Vector2:
                    return new JArray(property.vector2Value.x, property.vector2Value.y);
                case SerializedPropertyType.Vector3:
                    return new JArray(property.vector3Value.x, property.vector3Value.y, property.vector3Value.z);
                case SerializedPropertyType.Vector2Int:
                    return new JArray(property.vector2IntValue.x, property.vector2IntValue.y);
                case SerializedPropertyType.Vector3Int:
                    return new JArray(property.vector3IntValue.x, property.vector3IntValue.y, property.vector3IntValue.z);
                case SerializedPropertyType.Color:
                    Color color = property.colorValue;
                    return new JArray(color.r, color.g, color.b, color.a);
                case SerializedPropertyType.Generic:
                    var result = new JObject();
                    foreach (SerializedProperty child in Children(property)) result[child.name] = ToToken(child);
                    return result;
                default:
                    throw new NotSupportedException($"CSV로 다룰 수 없는 필드입니다: {property.propertyPath} ({property.propertyType})");
            }
        }

        private static void Apply(SerializedProperty property, Type type, FieldInfo field, JToken token)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                if (!(token is JArray array)) throw new CsvValueException("목록은 [ ] 형태여야 합니다.");

                Type elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                property.arraySize = array.Count;
                for (int i = 0; i < array.Count; i++)
                {
                    Apply(property.GetArrayElementAtIndex(i), elementType, field, array[i]);
                }
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    if (!long.TryParse(Scalar(token), NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
                    {
                        throw new CsvValueException($"정수가 아닙니다: {Scalar(token)}");
                    }
                    if ((type == typeof(int) && (integer < int.MinValue || integer > int.MaxValue)))
                    {
                        throw new CsvValueException($"값이 너무 큽니다: {integer}");
                    }
                    CheckLimits(field, integer);
                    property.longValue = integer;
                    break;
                case SerializedPropertyType.Float:
                    if (!double.TryParse(Scalar(token), NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                        || double.IsNaN(number) || double.IsInfinity(number))
                    {
                        throw new CsvValueException($"숫자가 아닙니다: {Scalar(token)}");
                    }
                    CheckLimits(field, number);
                    if (property.numericType == SerializedPropertyNumericType.Double) property.doubleValue = number;
                    else property.floatValue = (float)number;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = ParseBool(Scalar(token));
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = Scalar(token);
                    break;
                case SerializedPropertyType.Enum:
                    int index = Array.FindIndex(property.enumNames,
                        n => string.Equals(n, Scalar(token), StringComparison.OrdinalIgnoreCase));
                    if (index < 0)
                    {
                        throw new CsvValueException($"{string.Join(", ", property.enumNames)} 중 하나여야 합니다: {Scalar(token)}");
                    }
                    property.enumValueIndex = index;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = TextToReference(Scalar(token), type);
                    break;
                case SerializedPropertyType.Vector2:
                    float[] v2 = Numbers(token, 2);
                    property.vector2Value = new Vector2(v2[0], v2[1]);
                    break;
                case SerializedPropertyType.Vector3:
                    float[] v3 = Numbers(token, 3);
                    property.vector3Value = new Vector3(v3[0], v3[1], v3[2]);
                    break;
                case SerializedPropertyType.Vector2Int:
                    float[] i2 = Numbers(token, 2);
                    property.vector2IntValue = new Vector2Int(Whole(i2[0]), Whole(i2[1]));
                    break;
                case SerializedPropertyType.Vector3Int:
                    float[] i3 = Numbers(token, 3);
                    property.vector3IntValue = new Vector3Int(Whole(i3[0]), Whole(i3[1]), Whole(i3[2]));
                    break;
                case SerializedPropertyType.Color:
                    float[] c = Numbers(token, 4);
                    property.colorValue = new Color(c[0], c[1], c[2], c[3]);
                    break;
                case SerializedPropertyType.Generic:
                    ApplyObject(property, type, token);
                    break;
                default:
                    throw new CsvValueException("CSV로 다룰 수 없는 필드입니다.");
            }
        }

        private static void ApplyObject(SerializedProperty property, Type type, JToken token)
        {
            if (!(token is JObject values)) throw new CsvValueException("묶음은 { } 형태여야 합니다.");

            Dictionary<string, SerializedProperty> children = Children(property).ToDictionary(c => c.name);
            foreach (JProperty value in values.Properties())
            {
                if (!children.TryGetValue(value.Name, out SerializedProperty child))
                {
                    throw new CsvValueException($"알 수 없는 항목입니다: {value.Name}");
                }
                FieldInfo childField = FindField(type, value.Name);
                Apply(child, childField.FieldType, childField, value.Value);
            }
        }

        private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            SerializedProperty end = property.GetEndProperty();
            SerializedProperty child = property.Copy();
            for (bool next = child.NextVisible(true); next && !SerializedProperty.EqualContents(child, end); next = child.NextVisible(false))
            {
                yield return child.Copy();
            }
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        private static string Scalar(JToken token)
        {
            if (!(token is JValue value)) throw new CsvValueException("값 하나가 필요합니다.");
            return Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static bool ParseBool(string text)
        {
            if (text == "1" || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)) return true;
            if (text == "0" || string.Equals(text, "false", StringComparison.OrdinalIgnoreCase)) return false;
            throw new CsvValueException($"true 또는 false여야 합니다: {text}");
        }

        private static float[] Numbers(JToken token, int count)
        {
            if (!(token is JArray array) || array.Count != count)
            {
                throw new CsvValueException($"숫자 {count}개를 [ ]로 적어야 합니다.");
            }
            return array.Select(item =>
            {
                if (!float.TryParse(Scalar(item), NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                {
                    throw new CsvValueException($"숫자가 아닙니다: {item}");
                }
                return number;
            }).ToArray();
        }

        private static int Whole(float number)
        {
            if (!Mathf.Approximately(number, Mathf.Round(number))) throw new CsvValueException($"정수가 아닙니다: {number}");
            return Mathf.RoundToInt(number);
        }

        // 인스펙터에서만 적용되는 Min, Range 제한을 CSV에도 적용한다
        private static void CheckLimits(FieldInfo field, double number)
        {
            var min = field.GetCustomAttribute<MinAttribute>();
            if (min != null && (float)number < min.min) throw new CsvValueException($"{min.min} 이상이어야 합니다: {number}");

            var range = field.GetCustomAttribute<RangeAttribute>();
            if (range != null && ((float)number < range.min || (float)number > range.max))
            {
                throw new CsvValueException($"{range.min}~{range.max} 사이여야 합니다: {number}");
            }
        }

        private static string ReferenceToText(UnityEngine.Object target)
        {
            if (target == null) return string.Empty;

            string path = AssetDatabase.GetAssetPath(target);
            // 에셋 안의 하위 에셋은 "경로#이름"으로 적는다
            return AssetDatabase.IsSubAsset(target) ? $"{path}#{target.name}" : path;
        }

        private static UnityEngine.Object TextToReference(string text, Type type)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            string[] parts = text.Trim().Split('#');
            UnityEngine.Object asset = parts.Length > 1
                ? AssetDatabase.LoadAllAssetsAtPath(parts[0]).FirstOrDefault(a => a.name == parts[1] && type.IsInstanceOfType(a))
                : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(parts[0]);

            // 프리팹의 컴포넌트를 참조하는 필드
            if (asset is GameObject prefab && typeof(Component).IsAssignableFrom(type))
            {
                asset = prefab.GetComponent(type);
            }

            if (asset == null || !type.IsInstanceOfType(asset))
            {
                throw new CsvValueException($"{type.Name} 에셋을 찾을 수 없습니다: {text}");
            }
            return asset;
        }
    }
}
