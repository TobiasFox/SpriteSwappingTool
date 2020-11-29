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
            sharpnessProperty.doubleValue = EditorGUILayout.DoubleField(new GUIContent("Sharpness",
                "This value indicates how sharp a sprite is. The higher the value, the sharper the sprite.\n" +
                "Minimum: 0.0 "), sharpnessProperty.doubleValue);
            if (EditorGUI.EndChangeCheck())
            {
                sharpnessProperty.doubleValue = Math.Max(0, sharpnessProperty.doubleValue);
            }

            var perceivedLightnessProperty = property.FindPropertyRelative("perceivedLightness");
            perceivedLightnessProperty.floatValue = EditorGUILayout.Slider(new GUIContent("Perceived Lightness",
                "This value represents the average perceived lightness of a sprite, where 0 means dark and 100.0 means bright.\n" +
                "Range: 0.0 - 100.0"), perceivedLightnessProperty.floatValue, 0, 100);

            EditorGUI.BeginChangeCheck();
            var averageAlphaProperty = property.FindPropertyRelative("averageAlpha");
            averageAlphaProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent("Average alpha",
                    "This value represents the average alpha of a sprite ignoring completely transparent pixels."),
                averageAlphaProperty.floatValue, 0, 1);


            var primaryColorProperty = property.FindPropertyRelative("primaryColor");
            EditorGUILayout.PropertyField(primaryColorProperty, new GUIContent(primaryColorProperty.displayName,
                "This is the primary or average Color of a sprite ignoring completely transparent pixels."));

            EditorGUI.indentLevel--;
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}