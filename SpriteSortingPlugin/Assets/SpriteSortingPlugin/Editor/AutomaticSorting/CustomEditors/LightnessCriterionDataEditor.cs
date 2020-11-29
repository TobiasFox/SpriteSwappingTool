using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    // [CustomEditor(typeof(LightnessSortingCriterionData))]
    public class LightnessCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private LightnessSortingCriterionData LightnessSortingCriterionData =>
            (LightnessSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = "Perceived Sprite Lightness";
            tooltip = "";
        }

        protected override void OnInspectorGuiInternal()
        {
            LightnessSortingCriterionData.isLighterSpriteIsInForeground = EditorGUILayout.ToggleLeft(
                "Is lighter sprite in foreground", LightnessSortingCriterionData.isLighterSpriteIsInForeground);
        }
    }
}