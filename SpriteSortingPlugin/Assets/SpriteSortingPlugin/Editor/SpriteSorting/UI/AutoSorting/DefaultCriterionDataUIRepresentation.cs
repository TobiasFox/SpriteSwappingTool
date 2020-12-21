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
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(CalculateIndentSpace);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent(foregroundSortingName, foregroundSortingTooltip));
                        GUILayout.FlexibleSpace();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DefaultSortingCriterionData.isSortingInForeground = GUILayout.Toggle(
                            DefaultSortingCriterionData.isSortingInForeground, "Foreground",
                            Styling.ButtonStyle);

                        DefaultSortingCriterionData.isSortingInForeground = !GUILayout.Toggle(
                            !DefaultSortingCriterionData.isSortingInForeground, "Background",
                            Styling.ButtonStyle);
                    }
                }
            }
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
                    returnString = "Sort large Sprite in";
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = "Sort Sprite with smaller \"area-intersection area\" ratio in";
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = "Sort Sprite with overlapping Sprite Sort Point in";
                    break;
                case SortingCriterionType.Resolution:
                    returnString = "Sort Sprite with higher resolution in";
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = "Sort sharper Sprite in";
                    break;
                case SortingCriterionType.Lightness:
                    returnString = "Sort lighter Sprite in";
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = "Sort closer Sprite in";
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