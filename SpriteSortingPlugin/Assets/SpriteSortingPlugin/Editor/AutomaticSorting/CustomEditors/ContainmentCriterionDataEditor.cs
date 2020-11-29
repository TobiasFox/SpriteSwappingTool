using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(ContainmentSortingCriterionData))]
    public class ContainmentCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private ContainmentSortingCriterionData ContainmentSortingCriterionData =>
            (ContainmentSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = "Containment";
            tooltip = "Compares if SpriteRenderers are completely enclosed by other SpriteRenderers.\n" +
                      "When analyzing the alpha value, a " + nameof(SpriteData) + " asset is required.";
        }

        protected override void OnInspectorGuiInternal()
        {
            ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent("Is contained Sprite in foreground",
                    "When enabled, completely enclosed SpriteRenderers will be sorted in the foreground. Otherwise, enclosed SpriteRenderer will be sorted in the background and may be completely hidden."),
                ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground);

            using (new EditorGUI.DisabledScope(ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground))
            {
                ContainmentSortingCriterionData.isCheckingAlpha = EditorGUILayout.ToggleLeft(
                    new GUIContent("Is checking transparency of larger sprite",
                        "Completely enclosed SpriteRenderer will be sorted in the background. Consider comparing their transparency by enable this option.\n" +
                        "When enabled, the average transparency of the larger sprite will be compared to the alpha threshold."),
                    ContainmentSortingCriterionData.isCheckingAlpha);

                using (new EditorGUI.DisabledScope(!ContainmentSortingCriterionData.isCheckingAlpha))
                {
                    EditorGUI.BeginChangeCheck();
                    ContainmentSortingCriterionData.alphaThreshold =
                        EditorGUILayout.FloatField(
                            new GUIContent("Alpha threshold",
                                "Specifies the alpha threshold up to which a SpriteRenderer should be rendered in the foreground even though it is completely enclosed by another SpriteRenderer\n" +
                                "Range: 0.0 - 1.0"),
                            ContainmentSortingCriterionData.alphaThreshold);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ContainmentSortingCriterionData.alphaThreshold =
                            Mathf.Clamp01(ContainmentSortingCriterionData.alphaThreshold);
                    }
                }
            }
        }
    }
}