using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(CameraDistanceSortingCriterionData))]
    public class CameraDistanceCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private CameraDistanceSortingCriterionData CameraDistanceSortingCriterionData =>
            (CameraDistanceSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = "Camera distance difference";
            tooltip = UITooltipConstants.CameraDistanceTooltip;
        }

        protected override void OnInspectorGuiInternal()
        {
            CameraDistanceSortingCriterionData.isFurtherAwaySpriteInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent("Is further away Sprite in foreground",
                    UITooltipConstants.CameraDistanceForegroundSpriteTooltip),
                CameraDistanceSortingCriterionData.isFurtherAwaySpriteInForeground);
        }
    }
}