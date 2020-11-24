using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(LightnessSortingCriterionData))]
    public class LightnessCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private LightnessSortingCriterionData LightnessSortingCriterionData =>
            (LightnessSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                LightnessSortingCriterionData.isUsingSpriteColor =
                    GUILayout.Toggle(LightnessSortingCriterionData.isUsingSpriteColor, "Use Color of Sprite",
                        Styling.ButtonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    LightnessSortingCriterionData.isUsingSpriteRendererColor =
                        !LightnessSortingCriterionData.isUsingSpriteColor;
                }

                EditorGUI.BeginChangeCheck();
                LightnessSortingCriterionData.isUsingSpriteRendererColor =
                    GUILayout.Toggle(LightnessSortingCriterionData.isUsingSpriteRendererColor,
                        "Use Color of SpriteRenderer", Styling.ButtonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    LightnessSortingCriterionData.isUsingSpriteColor =
                        !LightnessSortingCriterionData.isUsingSpriteRendererColor;
                }
            }

            LightnessSortingCriterionData.isLighterSpriteIsInForeground = EditorGUILayout.ToggleLeft(
                "Is lighter sprite in foreground", LightnessSortingCriterionData.isLighterSpriteIsInForeground);
        }

        public override string GetTitleName()
        {
            return "Perceived Sprite Lightness";
        }
    }
}