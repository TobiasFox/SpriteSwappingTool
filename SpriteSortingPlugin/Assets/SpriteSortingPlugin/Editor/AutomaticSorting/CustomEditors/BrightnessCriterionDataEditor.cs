using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(BrightnessSortingCriterionData))]
    public class BrightnessCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private GUIStyle buttonStyle;

        protected override void InternalInitialize()
        {
            buttonStyle = new GUIStyle("Button");
        }

        private BrightnessSortingCriterionData BrightnessSortingCriterionData =>
            (BrightnessSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                BrightnessSortingCriterionData.isUsingSpriteColor =
                    GUILayout.Toggle(BrightnessSortingCriterionData.isUsingSpriteColor, "use Color of Sprite",
                        buttonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    BrightnessSortingCriterionData.isUsingSpriteRendererColor =
                        !BrightnessSortingCriterionData.isUsingSpriteColor;
                }

                EditorGUI.BeginChangeCheck();
                BrightnessSortingCriterionData.isUsingSpriteRendererColor =
                    GUILayout.Toggle(BrightnessSortingCriterionData.isUsingSpriteRendererColor,
                        "use Color of SpriteRenderer", buttonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    BrightnessSortingCriterionData.isUsingSpriteColor =
                        !BrightnessSortingCriterionData.isUsingSpriteRendererColor;
                }
            }

            BrightnessSortingCriterionData.isLighterSpriteIsInForeground = EditorGUILayout.ToggleLeft(
                "is lighter sprite in foreground", BrightnessSortingCriterionData.isLighterSpriteIsInForeground);
        }

        public override string GetTitleName()
        {
            return "Sprite Brightness";
        }
    }
}