using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    // [CustomEditor(typeof(LightnessSortingCriterionData))]
    public class LightnessCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private LightnessSortingCriterionData LightnessSortingCriterionData =>
            (LightnessSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            LightnessSortingCriterionData.isLighterSpriteIsInForeground = EditorGUILayout.ToggleLeft(
                "Is lighter sprite in foreground", LightnessSortingCriterionData.isLighterSpriteIsInForeground);
        }

        public override string GetTitleName()
        {
            return "Perceived Sprite Lightness";
        }
    }
}