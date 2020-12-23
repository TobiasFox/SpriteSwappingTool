using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.UI
{
    [CustomPropertyDrawer(typeof(SpriteDataItem))]
    public class SpriteDataItemPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUI.IndentLevelScope())
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded,
                    property.FindPropertyRelative("assetName").stringValue, true);

                if (!property.isExpanded)
                {
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
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
                }
            }

            EditorGUI.EndProperty();
        }
    }
}