using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteSorting.UI.AutoSorting;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting
{
    public static class SortingCriteriaComponentFactory
    {
        public static SortingCriteriaComponent CreateSortingCriteriaComponent(SortingCriterionType type)
        {
            var sortingCriteriaComponent = new SortingCriteriaComponent();

            switch (type)
            {
                case SortingCriterionType.Size:
                    CreateSizeDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.CameraDistance:
                    CreateCameraDistanceDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Resolution:
                    CreateResolutionDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Sharpness:
                    CreateSharpnessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Lightness:
                    CreateLightnessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.PrimaryColor:
                    CreatePrimaryColorDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Containment:
                    CreateContainmentDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.IntersectionArea:
                    CreateIntersectionAreaDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.SortPoint:
                    CreateSortPointDataAndCriterion(ref sortingCriteriaComponent);
                    break;
            }

            var criterionDataBaseEditor =
                SortingCriterionDataUIRepresentationFactory.CreateUIRepresentation(sortingCriteriaComponent.sortingCriterionData);
            criterionDataBaseEditor.Initialize(sortingCriteriaComponent.sortingCriterionData);
            sortingCriteriaComponent.criterionDataBaseUIRepresentation = criterionDataBaseEditor;

            return sortingCriteriaComponent;
        }

        private static void CreateSizeDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Size
            };
            sortingCriteriaComponent.sortingCriterion = new SizeSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateIntersectionAreaDataAndCriterion(
            ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.IntersectionArea, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new IntersectionAreaCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSortPointDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.SortPoint, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new SpriteSortPointSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateCameraDistanceDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.CameraDistance, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new CameraDistanceSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateResolutionDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Resolution, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new ResolutionSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSharpnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Sharpness, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new SharpnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreatePrimaryColorDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new PrimaryColorSortingCriterionData();
            sortingCriteriaComponent.sortingCriterion = new PrimaryColorSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateLightnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Lightness, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new LightnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateContainmentDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new ContainmentSortingCriterionData();
            sortingCriteriaComponent.sortingCriterion = new ContainmentSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }
    }
}