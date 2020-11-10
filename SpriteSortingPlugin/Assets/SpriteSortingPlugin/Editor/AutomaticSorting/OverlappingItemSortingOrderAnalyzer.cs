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

        public List<AutoSortingComponent> GenerateAutomaticSortingOrder(SortingComponent baseItem,
            List<SortingComponent> overlappingSortingComponents, SpriteDetectionData spriteDetectionData)
        {
            resultList = new List<AutoSortingComponent>();

            if (overlappingSortingComponents == null || overlappingSortingComponents.Count + 1 <= 1)
            {
                return resultList;
            }

            this.overlappingSortingComponents = overlappingSortingComponents;
            this.baseItem = baseItem;
            this.spriteDetectionData = spriteDetectionData;
            overlappingSortingComponents.Insert(0, baseItem);

            var autoSortingComponents = InitSortingDataList();
            AnalyzeContainment(ref autoSortingComponents);

            SplitAutoSortingComponentsByContainment(autoSortingComponents, out var notContainedComponents,
                out var containedComponents);

            SortNotContainedComponents(notContainedComponents);
            SortContainedComponents(containedComponents);

            //TODO consider baseItem
            
            return resultList;
        }

        private void SortContainedComponents(List<AutoSortingComponent> containedComponents)
        {
            AddFirstComponentIfPossible(containedComponents);

            foreach (var containedComponent in containedComponents)
            {
                var correspondingIndex = GetCorrespondingAutoSortingComponentIndex(resultList,
                    containedComponent.containedByAutoSortingComponent);
                if (correspondingIndex < 0)
                {
                    Debug.LogWarning("should not happen, break while");
                    break;
                }

                var beginCheckIndex = correspondingIndex + 1;
                containedComponent.sortingOrder =
                    containedComponent.containedByAutoSortingComponent.CurrentSortingOrder + 1;

                if (beginCheckIndex >= resultList.Count)
                {
                    resultList.Add(containedComponent);
                    continue;
                }

                InsertInResultList(containedComponent, beginCheckIndex);
            }
        }

        private void AddFirstComponentIfPossible(List<AutoSortingComponent> components)
        {
            if (resultList.Count != 0 || components.Count <= 0)
            {
                return;
            }

            var firstComponent = components[0];
            components.RemoveAt(0);

            resultList.Add(firstComponent);
            firstComponent.sortingOrder = firstComponent.CurrentSortingOrder;
        }

        private void SortNotContainedComponents(List<AutoSortingComponent> notContainedComponents)
        {
            AddFirstComponentIfPossible(notContainedComponents);

            foreach (var notContainedComponent in notContainedComponents)
            {
                InsertInResultList(notContainedComponent);
            }
        }

        private void InsertInResultList(AutoSortingComponent currentItem, int beginCheckIndex = 0)
        {
            var isInsertedInResultList = false;
            for (int i = beginCheckIndex; i < resultList.Count; i++)
            {
                //check against each of the items which are already in the resultList

                var autoSortingComponent = resultList[i];

                var sortingResult = CompareWithSortingCriterias(currentItem, autoSortingComponent);
                if (!sortingResult.isOverlapping)
                {
                    continue;
                }

                if (sortingResult.order < 0)
                {
                    currentItem.sortingOrder = autoSortingComponent.sortingOrder;
                    resultList.Insert(i, currentItem);

                    for (int j = i + 1; j < resultList.Count; j++)
                    {
                        var sortingComponent = resultList[j];
                        if (currentItem.IsOverlapping(sortingComponent))
                        {
                            sortingComponent.sortingOrder++;
                        }
                    }

                    isInsertedInResultList = true;
                    break;
                }

                currentItem.sortingOrder = autoSortingComponent.sortingOrder + 1;
            }

            if (!isInsertedInResultList)
            {
                resultList.Add(currentItem);
            }
        }

        private void SplitAutoSortingComponentsByContainment(List<AutoSortingComponent> autoSortingComponents,
            out List<AutoSortingComponent> notContainedComponents, out List<AutoSortingComponent> containedComponents)
        {
            notContainedComponents = new List<AutoSortingComponent>();
            containedComponents = new List<AutoSortingComponent>();

            foreach (var autoSortingComponent in autoSortingComponents)
            {
                if (autoSortingComponent.containedByAutoSortingComponent == null)
                {
                    notContainedComponents.Add(autoSortingComponent);
                }
                else
                {
                    containedComponents.Add(autoSortingComponent);
                }
            }
        }

        private AutoSortingResult CompareWithSortingCriterias(AutoSortingComponent unsortedItem,
            AutoSortingComponent sortedItem)
        {
            //check each sorting criteria
            var result = new AutoSortingResult {isOverlapping = unsortedItem.IsOverlapping(sortedItem)};

            if (!result.isOverlapping)
            {
                return result;
            }

            return result;
        }

        private void AnalyzeContainment(ref List<AutoSortingComponent> autoSortingComponents)
        {
            var spriteContainmentDetector = new SpriteContainmentDetector();

            foreach (var autoSortingComponent in autoSortingComponents)
            {
                var containedSortingComponent = spriteContainmentDetector.DetectContainedBySortingComponent(
                    autoSortingComponent, overlappingSortingComponents, spriteDetectionData);

                if (containedSortingComponent == null)
                {
                    continue;
                }

                var correspondingAutoSortingComponentIndex =
                    GetCorrespondingAutoSortingComponentIndex(autoSortingComponents, containedSortingComponent);

                if (correspondingAutoSortingComponentIndex < 0)
                {
                    continue;
                }

                autoSortingComponent.containedByAutoSortingComponent =
                    autoSortingComponents[correspondingAutoSortingComponentIndex];

                Debug.LogFormat("containment found: {0} in {1} ", containedSortingComponent.spriteRenderer.name,
                    autoSortingComponent.spriteRenderer.name);
            }
        }

        private int GetCorrespondingAutoSortingComponentIndex(List<AutoSortingComponent> autoSortingComponents,
            SortingComponent sortingComponent)
        {
            for (var i = 0; i < autoSortingComponents.Count; i++)
            {
                if (!autoSortingComponents[i].Equals(sortingComponent))
                {
                    continue;
                }

                return i;
            }

            return -1;
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