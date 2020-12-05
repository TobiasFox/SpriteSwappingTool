using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteSorting.UI.AutoSorting;
using UnityEditor;
using UnityEngine;

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

            var specificEditor = Editor.CreateEditor(sortingCriteriaComponent.sortingCriterionData);
            var criterionDataBaseEditor = (CriterionDataBaseEditor<SortingCriterionData>) specificEditor;
            criterionDataBaseEditor.Initialize(sortingCriteriaComponent.sortingCriterionData);
            sortingCriteriaComponent.criterionDataBaseEditor = criterionDataBaseEditor;

            return sortingCriteriaComponent;
        }

        private static void CreateSizeDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.Size;
            sortingCriteriaComponent.sortingCriterion = new SizeSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateIntersectionAreaDataAndCriterion(
            ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.IntersectionArea;
            sortingCriterionData.isSortingInForeground = true;
            sortingCriteriaComponent.sortingCriterion = new IntersectionAreaCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSortPointDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.SortPoint;
            sortingCriterionData.isSortingInForeground = true;
            sortingCriteriaComponent.sortingCriterion = new SpriteSortPointSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateCameraDistanceDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.CameraDistance;
            sortingCriterionData.isSortingInForeground = true;
            sortingCriteriaComponent.sortingCriterion = new CameraDistanceSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateResolutionDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.Resolution;
            sortingCriterionData.isSortingInForeground = true;
            sortingCriteriaComponent.sortingCriterion = new ResolutionSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSharpnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.Sharpness;
            sortingCriterionData.isSortingInForeground = true;
            sortingCriteriaComponent.sortingCriterion = new SharpnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreatePrimaryColorDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<PrimaryColorSortingCriterionData>();
            sortingCriteriaComponent.sortingCriterion = new PrimaryColorSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateLightnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.sortingCriterionType = SortingCriterionType.Lightness;
            sortingCriterionData.isSortingInForeground = true;
            sortingCriteriaComponent.sortingCriterion = new LightnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateContainmentDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<ContainmentSortingCriterionData>();
            sortingCriteriaComponent.sortingCriterion = new ContainmentSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }
    }
}