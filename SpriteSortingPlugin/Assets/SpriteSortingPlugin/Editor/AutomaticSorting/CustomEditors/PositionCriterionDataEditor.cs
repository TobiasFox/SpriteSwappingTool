using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    //TODO rename to camera distance
    [CustomEditor(typeof(PositionSortingCriterionData))]
    public class PositionCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private PositionSortingCriterionData PositionSortingCriterionData =>
            (PositionSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = "Camera distance difference";
            tooltip = UITooltipConstants.CameraDistanceTooltip;
        }

        protected override void OnInspectorGuiInternal()
        {
            PositionSortingCriterionData.isFurtherAwaySpriteInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent("Is further away Sprite in foreground",
                    UITooltipConstants.CameraDistanceForegroundSpriteTooltip),
                PositionSortingCriterionData.isFurtherAwaySpriteInForeground);
        }
    }
}