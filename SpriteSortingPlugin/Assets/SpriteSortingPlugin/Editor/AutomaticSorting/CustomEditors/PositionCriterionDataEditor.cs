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
                "Is further away Sprite in foreground", PositionSortingCriterionData.isFurtherAwaySpriteInForeground);
        }

        public override string GetTitleName()
        {
            return "Position difference";
        }
    }
}