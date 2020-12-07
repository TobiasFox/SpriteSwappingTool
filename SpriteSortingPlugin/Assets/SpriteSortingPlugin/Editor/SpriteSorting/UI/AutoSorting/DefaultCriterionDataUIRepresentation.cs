using SpriteSortingPlugin.SpriteSorting.AutomaticSorting;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public class DefaultCriterionDataUIRepresentation : CriterionDataBaseUIRepresentation<SortingCriterionData>
    {
        private string foregroundSortingName;
        private string foregroundSortingTooltip;

        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = GetSortingCriteriaTitle(DefaultSortingCriterionData.sortingCriterionType);
            tooltip = GetSortingCriterionToolTip(DefaultSortingCriterionData.sortingCriterionType);
            foregroundSortingName =
                GetSortingCriteriaForegroundSortingName(DefaultSortingCriterionData.sortingCriterionType);
            foregroundSortingTooltip =
                GetSortingCriteriaForegroundSortingTooltip(DefaultSortingCriterionData.sortingCriterionType);
        }

        protected override void OnInspectorGuiInternal()
        {
            DefaultSortingCriterionData.isSortingInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent(foregroundSortingName, foregroundSortingTooltip),
                DefaultSortingCriterionData.isSortingInForeground);
        }

        private string GetSortingCriteriaTitle(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = "Size";
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = "Intersection Area";
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = "Sprite Sort Point";
                    break;
                case SortingCriterionType.Resolution:
                    returnString = "Sprite Resolution";
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = "Sprite Sharpness";
                    break;
                case SortingCriterionType.Lightness:
                    returnString = "Perceived Lightness";
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = "Camera distance difference";
                    break;
            }

            return returnString;
        }

        private string GetSortingCriterionToolTip(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = UITooltipConstants.SizeTooltip;
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = UITooltipConstants.IntersectionAreaTooltip;
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = UITooltipConstants.SpriteSortPointTooltip;
                    break;
                case SortingCriterionType.Resolution:
                    returnString = UITooltipConstants.ResolutionTooltip;
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = UITooltipConstants.SharpnessTooltip;
                    break;
                case SortingCriterionType.Lightness:
                    returnString = UITooltipConstants.PerceivedLightnessTooltip;
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = UITooltipConstants.CameraDistanceTooltip;
                    break;
            }

            return returnString;
        }

        private string GetSortingCriteriaForegroundSortingName(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = "Is large Sprite in foreground";
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = "Is Sprite with smaller \"area-intersection area\" ratio in foreground";
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = "Is Sprite with overlapping Sprite Sort Point in foreground";
                    break;
                case SortingCriterionType.Resolution:
                    returnString = "Is Sprite with higher resolution in foreground";
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = "Is sharper Sprite in foreground";
                    break;
                case SortingCriterionType.Lightness:
                    returnString = "Is lighter Sprite in foreground";
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = "Is closer Sprite in foreground";
                    break;
            }

            return returnString;
        }

        private string GetSortingCriteriaForegroundSortingTooltip(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = UITooltipConstants.SizeForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = UITooltipConstants.IntersectionAreaForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = UITooltipConstants.SpriteSortPointForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.Resolution:
                    returnString = UITooltipConstants.ResolutionForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = UITooltipConstants.SharpnessForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.Lightness:
                    returnString = UITooltipConstants.PerceivedLightnessForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = UITooltipConstants.CameraDistanceForegroundSpriteTooltip;
                    break;
            }

            return returnString;
        }
    }
}