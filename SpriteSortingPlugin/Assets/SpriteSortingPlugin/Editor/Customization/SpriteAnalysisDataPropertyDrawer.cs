using System;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Customization
{
    [CustomPropertyDrawer(typeof(SpriteAnalysisData))]
    public class SpriteAnalysisDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            var sharpnessProperty = property.FindPropertyRelative("sharpness");
            EditorGUILayout.PropertyField(sharpnessProperty);
            if (EditorGUI.EndChangeCheck())
            {
                sharpnessProperty.floatValue = Math.Max(0, sharpnessProperty.floatValue);
            }

            EditorGUI.BeginChangeCheck();
            var perceivedLightnessProperty = property.FindPropertyRelative("perceivedLightness");
            EditorGUILayout.PropertyField(perceivedLightnessProperty);
            if (EditorGUI.EndChangeCheck())
            {
                perceivedLightnessProperty.floatValue = Mathf.Clamp(perceivedLightnessProperty.floatValue, 0f, 100f);
            }

            EditorGUI.BeginChangeCheck();
            var primaryColorProperty = property.FindPropertyRelative("primaryColor");
            EditorGUILayout.PropertyField(primaryColorProperty);
            if (EditorGUI.EndChangeCheck())
            {
            }

            EditorGUI.BeginChangeCheck();
            var averageAlphaProperty = property.FindPropertyRelative("averageAlpha");
            EditorGUILayout.PropertyField(averageAlphaProperty);
            if (EditorGUI.EndChangeCheck())
            {
                averageAlphaProperty.floatValue = Mathf.Clamp01(averageAlphaProperty.floatValue);
            }

            EditorGUI.indentLevel--;
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}