using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(LightnessSortingCriterionData))]
    public class LightnessCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private GUIStyle buttonStyle;

        protected override void InternalInitialize()
        {
            buttonStyle = new GUIStyle("Button");
        }

        private LightnessSortingCriterionData LightnessSortingCriterionData =>
            (LightnessSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                LightnessSortingCriterionData.isUsingSpriteColor =
                    GUILayout.Toggle(LightnessSortingCriterionData.isUsingSpriteColor, "use Color of Sprite",
                        buttonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    LightnessSortingCriterionData.isUsingSpriteRendererColor =
                        !LightnessSortingCriterionData.isUsingSpriteColor;
                }

                EditorGUI.BeginChangeCheck();
                LightnessSortingCriterionData.isUsingSpriteRendererColor =
                    GUILayout.Toggle(LightnessSortingCriterionData.isUsingSpriteRendererColor,
                        "use Color of SpriteRenderer", buttonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    LightnessSortingCriterionData.isUsingSpriteColor =
                        !LightnessSortingCriterionData.isUsingSpriteRendererColor;
                }
            }

            LightnessSortingCriterionData.isLighterSpriteIsInForeground = EditorGUILayout.ToggleLeft(
                "is lighter sprite in foreground", LightnessSortingCriterionData.isLighterSpriteIsInForeground);
        }

        public override string GetTitleName()
        {
            return "Perceived Sprite Lightness";
        }
    }
}