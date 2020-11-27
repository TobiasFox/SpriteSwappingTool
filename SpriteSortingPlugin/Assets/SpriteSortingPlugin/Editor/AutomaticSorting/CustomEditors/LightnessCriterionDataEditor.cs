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
                GUILayout.Toggle(!LightnessSortingCriterionData.isUsingSpriteRendererColor, "Use Color of Sprite Only",
                    Styling.ButtonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    LightnessSortingCriterionData.isUsingSpriteRendererColor = false;
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Toggle(LightnessSortingCriterionData.isUsingSpriteRendererColor,
                    "Combine Colors of Sprite and SpriteRenderer", Styling.ButtonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    LightnessSortingCriterionData.isUsingSpriteRendererColor = true;
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