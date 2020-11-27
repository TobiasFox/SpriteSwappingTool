using SpriteSortingPlugin.AutomaticSorting.Criterias;
using SpriteSortingPlugin.AutomaticSorting.CustomEditors;
using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting
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
                case SortingCriterionType.Position:
                    CreatePositionDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Resolution:
                    CreateResolutionDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Sharpness:
                    CreateSharpnessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Brightness:
                    CreateLightnessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.PrimaryColor:
                    CreatePrimaryColorDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Containment:
                    CreateContainmentDataAndCriterion(ref sortingCriteriaComponent);
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
            sortingCriterionData.criterionName = "Size";
            sortingCriterionData.foregroundSortingName = "Is large sprite in foreground";
            sortingCriteriaComponent.sortingCriterion = new SizeSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreatePositionDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<PositionSortingCriterionData>();
            sortingCriteriaComponent.sortingCriterion = new PositionSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateResolutionDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.criterionName = "Sprite Resolution";
            sortingCriterionData.foregroundSortingName = "Is sprite with higher pixel density in foreground";
            sortingCriteriaComponent.sortingCriterion = new ResolutionSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSharpnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.criterionName = "Sprite Sharpness";
            sortingCriterionData.foregroundSortingName = "Is sharper sprite in foreground";
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
            sortingCriterionData.criterionName = "Perceived Lightness";
            sortingCriterionData.foregroundSortingName = "Is lighter sprite in foreground";
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