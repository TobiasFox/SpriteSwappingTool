using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(PositionSortingCriterionData))]
    public class PositionCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private PositionSortingCriterionData PositionSortingCriterionData =>
            (PositionSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            PositionSortingCriterionData.isFurtherAwaySpriteInForeground = EditorGUILayout.ToggleLeft(
                "is further away Sprite in foreground", PositionSortingCriterionData.isFurtherAwaySpriteInForeground);
        }

        protected override string GetTitleName()
        {
            return "Position difference";
        }
    }
}