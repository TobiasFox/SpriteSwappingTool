using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI
{
    public static class UIUtil
    {
        public static bool DrawFoldoutBoolContent(bool isActive, GUIContent content)
        {
            var foldoutBoolContentRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);

            var labelRect = foldoutBoolContentRect;
            labelRect.xMin += 15f;
            labelRect.width = 136;

            var foldoutRect = foldoutBoolContentRect;
            foldoutRect.xMax = 13;

            var toggleRect = foldoutBoolContentRect;
            toggleRect.xMin = labelRect.xMax + 2;

            isActive = GUI.Toggle(foldoutRect, isActive, GUIContent.none, EditorStyles.foldout);

            EditorGUI.LabelField(labelRect, content);

            isActive = GUI.Toggle(toggleRect, isActive, GUIContent.none);

            return isActive;
        }
    }
}