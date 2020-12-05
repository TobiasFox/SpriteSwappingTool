using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
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
            CameraDistanceSortingCriterionData.isCloserSpriteInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent("Is closer Sprite in foreground",
                    UITooltipConstants.CameraDistanceForegroundSpriteTooltip),
                CameraDistanceSortingCriterionData.isCloserSpriteInForeground);
        }
    }
}