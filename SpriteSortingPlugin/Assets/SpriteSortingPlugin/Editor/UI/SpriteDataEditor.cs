using UnityEditor;

namespace SpriteSortingPlugin.UI
{
    [CustomEditor(typeof(SpriteData))]
    public class SpriteDataEditor : Editor
    {
        private SerializedProperty spriteDataListProperty;

        private void Awake()
        {
            spriteDataListProperty = serializedObject.FindProperty("spriteDataList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(spriteDataListProperty, false);
            if (spriteDataListProperty.isExpanded)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(spriteDataListProperty.FindPropertyRelative("Array.size"));
                }

                for (var i = 0; i < spriteDataListProperty.arraySize; i++)
                {
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(spriteDataListProperty.GetArrayElementAtIndex(i), true);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}