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

        protected override void OnInspectorGuiInternal()
        {
            ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground = EditorGUILayout.ToggleLeft(
                "Is enclosed Sprite in foreground",
                ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground);

            using (new EditorGUI.DisabledScope(ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground))
            {
                ContainmentSortingCriterionData.isCheckingAlpha = EditorGUILayout.ToggleLeft(
                    "Is checking transparency of larger sprite", ContainmentSortingCriterionData.isCheckingAlpha);

                using (new EditorGUI.DisabledScope(!ContainmentSortingCriterionData.isCheckingAlpha))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        GUILayout.Toggle(!ContainmentSortingCriterionData.isUsingSpriteRendererColor,
                            "Use Color of Sprite Only", Styling.ButtonStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            ContainmentSortingCriterionData.isUsingSpriteRendererColor = false;
                        }

                        EditorGUI.BeginChangeCheck();
                        GUILayout.Toggle(ContainmentSortingCriterionData.isUsingSpriteRendererColor,
                            "Combine Colors of Sprite and SpriteRenderer", Styling.ButtonStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            ContainmentSortingCriterionData.isUsingSpriteRendererColor = true;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    ContainmentSortingCriterionData.alphaThreshold =
                        EditorGUILayout.FloatField("Alpha threshold", ContainmentSortingCriterionData.alphaThreshold);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ContainmentSortingCriterionData.alphaThreshold =
                            Mathf.Clamp01(ContainmentSortingCriterionData.alphaThreshold);
                    }
                }
            }
        }

        public override string GetTitleName()
        {
            return "Containment";
        }
    }
}