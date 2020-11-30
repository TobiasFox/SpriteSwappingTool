using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Customization
{
    [CustomPropertyDrawer(typeof(SpriteDataItem))]
    public class SpriteDataItemPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.LabelField(position, "Asset Name", property.FindPropertyRelative("assetName").stringValue);
            }

            EditorGUI.indentLevel++;
            property.serializedObject.Update();
            var oobbProperty = property.FindPropertyRelative("objectOrientedBoundingBox");
            EditorGUILayout.PropertyField(oobbProperty, true);
            property.serializedObject.ApplyModifiedProperties();

            property.serializedObject.Update();
            var outlinePointsProperty = property.FindPropertyRelative("outlinePoints");
            EditorGUILayout.PropertyField(outlinePointsProperty, true);
            property.serializedObject.ApplyModifiedProperties();

            property.serializedObject.Update();
            var spriteAnalysisDataProperty = property.FindPropertyRelative("spriteAnalysisData");
            EditorGUILayout.PropertyField(spriteAnalysisDataProperty, true);
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}