using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class DisplaySortingOrderParent : MonoBehaviour
    {
        public bool isDisplayingSortingOrder = true;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DisplaySortingOrderParent))]
    public class DisplaySortingOrderParentEditor : Editor
    {
        private DisplaySortingOrder[] displaySortingOrderChildren;

        private void OnEnable()
        {
            displaySortingOrderChildren =
                ((DisplaySortingOrderParent) target).GetComponentsInChildren<DisplaySortingOrder>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var serializedProperty = serializedObject.FindProperty("isDisplayingSortingOrder");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty);
            if (EditorGUI.EndChangeCheck())
            {
                if (displaySortingOrderChildren != null)
                {
                    foreach (var displaySortingOrderChild in displaySortingOrderChildren)
                    {
                        displaySortingOrderChild.isDisplayingSortingOrder = serializedProperty.boolValue;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}