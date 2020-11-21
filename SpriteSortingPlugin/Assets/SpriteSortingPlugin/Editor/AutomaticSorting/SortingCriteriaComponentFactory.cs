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
                case SortingCriterionType.Blurriness:
                    CreateBlurrinessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Brightness:
                    CreateBrightnessDataAndCriterion(ref sortingCriteriaComponent);
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
            sortingCriterionData.foregroundSortingName = "is large sprite in foreground";
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
            sortingCriterionData.foregroundSortingName = "is sprite with higher pixel density in foreground";
            sortingCriteriaComponent.sortingCriterion = new ResolutionSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateBlurrinessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<DefaultSortingCriterionData>();
            sortingCriterionData.criterionName = "Sprite Blurriness";
            sortingCriterionData.foregroundSortingName = "is more blurry sprite in foreground";
            sortingCriteriaComponent.sortingCriterion = new BlurrinessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreatePrimaryColorDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<PrimaryColorSortingCriterionData>();
            sortingCriteriaComponent.sortingCriterion = new PrimaryColorSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateBrightnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<LightnessSortingCriterionData>();
            sortingCriteriaComponent.sortingCriterion = new LightnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }
        
        private static void CreateContainmentDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = ScriptableObject.CreateInstance<ContainmentSortingCriterionData>();
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }
    }
}