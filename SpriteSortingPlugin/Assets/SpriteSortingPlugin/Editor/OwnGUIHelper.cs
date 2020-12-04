using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public static class OwnGUIHelper
    {
        public static float DrawField(float value)
        {
            return EditorGUILayout.FloatField(nameof(value), value);
        }

        public static bool DrawField(bool value)
        {
            return EditorGUILayout.Toggle(nameof(value), value);
        }

        public static Color DrawField(Color color)
        {
            return EditorGUILayout.ColorField(nameof(color), color);
        }

        public static int DrawField(int value)
        {
            return EditorGUILayout.IntField(nameof(value), value);
        }
    }
}