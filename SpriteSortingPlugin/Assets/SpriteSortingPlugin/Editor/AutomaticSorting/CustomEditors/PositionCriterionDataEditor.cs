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
            title = "Position difference";
            tooltip =
                "Compares the distance to the camera. Will be ignored when using orthographic Transparency Sort mode or Default Transparency Sort mode and orthographic camera project.";
        }

        protected override void OnInspectorGuiInternal()
        {
            PositionSortingCriterionData.isFurtherAwaySpriteInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent("Is further away Sprite in foreground",
                    "When enabled, SpriteRenderer with a higher distance to the camera will be sorted in the foreground."),
                PositionSortingCriterionData.isFurtherAwaySpriteInForeground);
        }
    }
}