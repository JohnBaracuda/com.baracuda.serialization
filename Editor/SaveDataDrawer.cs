using System;
using Baracuda.Utility.Reflection;
using Baracuda.Utility.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Baracuda.Serialization.Editor
{
    public class SaveDataDrawer
    {
        private readonly Action<bool> _drawerDelegate;
        private readonly Action _defaultValueDelegate;

        public readonly string Label;
        public readonly string HumanizedLabel;
        public readonly string TypeName;
        public SaveData SaveData { get; }

        public void DrawValue(bool humanizeLabel)
        {
            _drawerDelegate(humanizeLabel);
        }

        public void DrawDefaultValue()
        {
            _defaultValueDelegate();
        }

        public void DrawTags()
        {
            GUI.enabled = false;
            UnityEditor.EditorGUILayout.TextField("Tags", SaveData.FileOptions.Tags.CombineToString());
            GUI.enabled = true;
        }

        public SaveDataDrawer(SaveData saveData)
        {
            SaveData = saveData;
            var type = saveData.DataType;
            var drawer = CreateDrawer(type);

            var valueProperty = saveData.GetType().GetProperty("Value");
            var defaultProperty = saveData.GetType().GetProperty("DefaultValue");
            var defaultValue = defaultProperty!.GetValue(saveData);

            HumanizedLabel = saveData.Key.ToString().Humanize();
            TypeName = type.HumanizedName();
            Label = saveData.Key.ToString();

            _drawerDelegate = humanizeLabel =>
            {
                var value = valueProperty!.GetValue(saveData);
                UnityEditor.EditorGUI.BeginChangeCheck();
                var result = drawer(value, humanizeLabel ? HumanizedLabel : Label);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    valueProperty.SetValue(saveData, result);
                }
            };

            _defaultValueDelegate = () =>
            {
                GUI.enabled = false;
                drawer(defaultValue, "Default");
                GUI.enabled = true;
            };
        }

        private Func<object, string, object> CreateDrawer(Type type)
        {
            if (type == typeof(int))
            {
                return (value, label) => UnityEditor.EditorGUILayout.IntField(label, value != null ? (int)value : 0);
            }
            if (type == typeof(long))
            {
                return (value, label) => UnityEditor.EditorGUILayout.LongField(label, value != null ? (long)value : 0);
            }
            if (type == typeof(double))
            {
                return (value, label) => UnityEditor.EditorGUILayout.DoubleField(label, value != null ? (double)value : 0);
            }
            if (type == typeof(byte))
            {
                return (value, label) => UnityEditor.EditorGUILayout.IntField(label, value != null ? (byte)value : 0);
            }
            if (type == typeof(float))
            {
                return (value, label) => UnityEditor.EditorGUILayout.FloatField(label, value != null ? (float)value : 0f);
            }
            if (type == typeof(string))
            {
                return (value, label) => UnityEditor.EditorGUILayout.TextField(label, value != null ? (string)value : string.Empty);
            }
            if (type == typeof(bool))
            {
                return (value, label) => UnityEditor.EditorGUILayout.Toggle(label, value != null && (bool)value);
            }
            if (type == typeof(Vector2))
            {
                return (value, label) => UnityEditor.EditorGUILayout.Vector2Field(label, value != null ? (Vector2)value : Vector2.zero);
            }
            if (type == typeof(Vector3))
            {
                return (value, label) => UnityEditor.EditorGUILayout.Vector3Field(label, value != null ? (Vector3)value : Vector3.zero);
            }
            if (type == typeof(Vector4))
            {
                return (value, label) => UnityEditor.EditorGUILayout.Vector4Field(label, value != null ? (Vector4)value : Vector4.zero);
            }
            if (type == typeof(Quaternion))
            {
                return (value, label) =>
                {
                    var quaternion = value != null ? (Quaternion)value : Quaternion.identity;
                    var vector = UnityEditor.EditorGUILayout.Vector4Field(label, new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w));
                    return new Quaternion(vector.x, vector.y, vector.z, vector.w);
                };
            }
            if (type == typeof(Color))
            {
                return (value, label) => UnityEditor.EditorGUILayout.ColorField(label, value != null ? (Color)value : Color.white);
            }
            if (type == typeof(Rect))
            {
                return (value, label) => UnityEditor.EditorGUILayout.RectField(label, value != null ? (Rect)value : Rect.zero);
            }
            if (type == typeof(Bounds))
            {
                return (value, label) => UnityEditor.EditorGUILayout.BoundsField(label, value != null ? (Bounds)value : new Bounds(Vector3.zero, Vector3.one));
            }
            if (type.IsEnum)
            {
                return (value, label) => UnityEditor.EditorGUILayout.EnumPopup(label, value != null ? (Enum)value : (Enum)Activator.CreateInstance(type));
            }
            if (type == typeof(AnimationCurve))
            {
                return (value, label) => UnityEditor.EditorGUILayout.CurveField(label, value != null ? (AnimationCurve)value : new AnimationCurve());
            }
            if (type == typeof(LayerMask))
            {
                return (value, label) =>
                {
                    var mask = value != null ? ((LayerMask)value).value : 0;
                    return (LayerMask)UnityEditor.EditorGUILayout.LayerField(label, mask);
                };
            }
            if (type == typeof(Vector2Int))
            {
                return (value, label) => UnityEditor.EditorGUILayout.Vector2IntField(label, value != null ? (Vector2Int)value : Vector2Int.zero);
            }
            if (type == typeof(Vector3Int))
            {
                return (value, label) => UnityEditor.EditorGUILayout.Vector3IntField(label, value != null ? (Vector3Int)value : Vector3Int.zero);
            }
            if (type == typeof(BoundsInt))
            {
                return (value, label) =>
                    UnityEditor.EditorGUILayout.BoundsIntField(label, value != null ? (BoundsInt)value : new BoundsInt(Vector3Int.zero, Vector3Int.one));
            }
            if (type == typeof(RectInt))
            {
                return (value, label) => UnityEditor.EditorGUILayout.RectIntField(label, value != null ? (RectInt)value : new RectInt(0, 0, 0, 0));
            }
            if (type == typeof(Object))
            {
                return (value, label) => UnityEditor.EditorGUILayout.ObjectField(label, value as Object, typeof(Object), true);
            }

            return (value, label) =>
            {
                GUI.enabled = false;
                UnityEditor.EditorGUILayout.LabelField(label, value?.ToString());
                GUI.enabled = true;
                return null;
            };
        }
    }
}