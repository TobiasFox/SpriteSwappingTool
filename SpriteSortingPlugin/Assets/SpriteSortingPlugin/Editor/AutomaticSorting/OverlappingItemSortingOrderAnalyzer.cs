using System.Collections.Generic;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using SpriteSortingPlugin.OverlappingSprites;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public class OverlappingItemSortingOrderAnalyzer
    {
        private List<SortingCriteria> sortingCriterias;
        private SpriteDetectionData spriteDetectionData;
        private OverlappingSpriteDetector overlappingSpriteDetector;
        private List<AutoSortingComponent> resultList;
        private List<SortingComponent> overlappingSortingComponents;
        private SortingComponent baseItem;

        public void AddSortingCriteria(SortingCriteria sortingCriteria)
        {
            if (sortingCriterias == null)
            {
                sortingCriterias = new List<SortingCriteria>();
            }

            sortingCriterias.Add(sortingCriteria);
        }

        public List<SortingComponent> GenerateAutomaticSortingOrder(SortingComponent baseItem,
            List<SortingComponent> overlappingSortingComponents, SpriteDetectionData spriteDetectionData)
        {
            if (overlappingSortingComponents == null || overlappingSortingComponents.Count <= 1)
            {
                return overlappingSortingComponents;
            }

            resultList = new List<AutoSortingComponent>();
            this.overlappingSortingComponents = overlappingSortingComponents;
            this.baseItem = baseItem;
            this.spriteDetectionData = spriteDetectionData;
            overlappingSortingComponents.Add(baseItem);
            var autoSortingComponents = InitSortingDataList();

            AnalyzeContainment(ref autoSortingComponents);

            var result = new List<SortingComponent>();
            foreach (var autoSortingComponent in autoSortingComponents)
            {
                result.Add(autoSortingComponent);
            }

            return result;
        }

        private void AnalyzeContainment(ref List<AutoSortingComponent> autoSortingComponents)
        {
            var spriteContainmentDetector = new SpriteContainmentDetector();

            foreach (var autoSortingComponent in autoSortingComponents)
            {
                var containedSortingComponent = spriteContainmentDetector.DetectContainedBySortingComponent(
                    autoSortingComponent,
                    overlappingSortingComponents, spriteDetectionData);

                if (containedSortingComponent != null)
                {
                    var correspondingAutoSortingComponent =
                        GetCorrespondingAutoSortingComponent(autoSortingComponents, containedSortingComponent);

                    autoSortingComponent.containedByAutoSortingComponent = correspondingAutoSortingComponent;

                    Debug.LogFormat("containment found: {0} in {1} ", containedSortingComponent.spriteRenderer.name,
                        autoSortingComponent.spriteRenderer.name);
                }
            }
        }

        private AutoSortingComponent GetCorrespondingAutoSortingComponent(
            List<AutoSortingComponent> autoSortingComponents, SortingComponent sortingComponent)
        {
            foreach (var autoSortingComponent in autoSortingComponents)
            {
                if (!autoSortingComponent.Equals(sortingComponent))
                {
                    continue;
                }

                return autoSortingComponent;
            }

            return null;
        }

        private List<AutoSortingComponent> InitSortingDataList()
        {
            AutoSortingComponent.ResetID();

            overlappingSpriteDetector = new OverlappingSpriteDetector();
            var autoSortingComponents = new List<AutoSortingComponent>();

            foreach (var overlappingItem in overlappingSortingComponents)
            {
                var autoSortingComponent = new AutoSortingComponent(overlappingItem);

                var overlappingItems = overlappingSpriteDetector.DetectOverlappingSortingComponents(overlappingItem,
                    overlappingSortingComponents, spriteDetectionData);

                foreach (var overlappingSortingItem in overlappingItems)
                {
                    autoSortingComponent.AddOverlappingSortingComponent(overlappingSortingItem);
                }

                autoSortingComponents.Add(autoSortingComponent);
            }

            return autoSortingComponents;
        }

        private int AnalyzeSortingCriterias(OverlappingItem currentOverlappingItem)
        {
            //TODO: sort after sorting layer, highest first

            // this.spriteDetectionData = spriteDetectionData;
            // var resultList = new List<OverlappingItem> {list[0]};
            //
            // var currentOverlappingItem = list[0];
            //
            // while (list.Count > 0)
            // {
            //     var nextOverlappingItem = list[0];
            //
            //     var currentSortingResultOverlappingItem = AnalyzeSortingCriterias(currentOverlappingItem);
            //     var nextSortingResultOverlappingItem = AnalyzeSortingCriterias(currentOverlappingItem);
            //
            //     var overlappingItemToCheck = nextOverlappingItem;
            //     if (nextSortingResultOverlappingItem > currentSortingResultOverlappingItem)
            //     {
            //     }
            // }


            // foreach (var sortingStrategy in sortingStrategies)
            // {
            //     
            //     
            //     resultList = sortingStrategy.Sort(resultList, spriteData);
            // }

            foreach (var sortingCriteria in sortingCriterias)
            {
                //analyze sorting criteria
            }

            return 0;
        }
    }
}