using System;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.UI
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
            sharpnessProperty.doubleValue = EditorGUILayout.DoubleField(new GUIContent(sharpnessProperty.displayName,
                UITooltipConstants.SpriteDataSharpnessTooltip), sharpnessProperty.doubleValue);
            if (EditorGUI.EndChangeCheck())
            {
                sharpnessProperty.doubleValue = Math.Max(0, sharpnessProperty.doubleValue);
            }

            var perceivedLightnessProperty = property.FindPropertyRelative("perceivedLightness");
            perceivedLightnessProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent(perceivedLightnessProperty.displayName, UITooltipConstants.SpriteDataPerceivedLightnessTooltip),
                perceivedLightnessProperty.floatValue, 0, 100);

            EditorGUI.BeginChangeCheck();
            var averageAlphaProperty = property.FindPropertyRelative("averageAlpha");
            averageAlphaProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent("Average alpha", UITooltipConstants.SpriteDataAverageAlphaTooltip),
                averageAlphaProperty.floatValue, 0, 1);


            var primaryColorProperty = property.FindPropertyRelative("primaryColor");
            EditorGUILayout.PropertyField(primaryColorProperty,
                new GUIContent(primaryColorProperty.displayName, UITooltipConstants.SpriteDataPrimaryColorTooltip));

            EditorGUI.indentLevel--;
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}