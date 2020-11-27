using System.Collections.Generic;
using SpriteSortingPlugin.AutomaticSorting.Criterias;
using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using SpriteSortingPlugin.OverlappingSprites;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public class AutoSortingGenerator
    {
        private readonly List<SortingCriterion<SortingCriterionData>> sortingCriterias =
            new List<SortingCriterion<SortingCriterionData>>();

        private AutoSortingCalculationData autoSortingCalculationData;
        private OverlappingSpriteDetector overlappingSpriteDetector;
        private List<AutoSortingComponent> resultList;
        private List<SortingComponent> overlappingSortingComponents;
        private SortingComponent baseItem;
        private ContainmentSortingCriterionData containmentSortingCriterionData;
        private SortingCriterion<SortingCriterionData> containmentSortingCriterion;

        public void AddSortingCriteria(SortingCriterion<SortingCriterionData> sortingCriterion)
        {
            sortingCriterias.Add(sortingCriterion);
        }

        public void SetContainmentCriteria(ContainmentSortingCriterionData containmentSortingCriterionData,
            SortingCriterion<SortingCriterionData> containmentSortingCriterion)
        {
            this.containmentSortingCriterionData = containmentSortingCriterionData;
            this.containmentSortingCriterion = containmentSortingCriterion;
        }

        public List<AutoSortingComponent> GenerateAutomaticSortingOrder(SortingComponent baseItem,
            List<SortingComponent> overlappingSortingComponents, AutoSortingCalculationData autoSortingCalculationData)
        {
            resultList = new List<AutoSortingComponent>();

            if (overlappingSortingComponents == null || overlappingSortingComponents.Count + 1 <= 1)
            {
                return resultList;
            }

            this.overlappingSortingComponents = overlappingSortingComponents;
            this.baseItem = baseItem;
            this.autoSortingCalculationData = autoSortingCalculationData;
            overlappingSortingComponents.Insert(0, baseItem);

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(this.autoSortingCalculationData.spriteData);

            var autoSortingComponents = InitSortingDataList();

            if (containmentSortingCriterionData != null && containmentSortingCriterionData.isActive)
            {
                AnalyzeContainment(ref autoSortingComponents);

                SplitAutoSortingComponentsByContainment(autoSortingComponents, out var notContainedComponents,
                    out var containedComponents);

                SortNotContainedComponents(notContainedComponents);
                SortContainedComponents(containedComponents);
            }
            else
            {
                SortNotContainedComponents(autoSortingComponents);
            }

            spriteDataItemValidatorCache.Clear();

            resultList.Reverse();

            //TODO consider baseItem
            containmentSortingCriterionData = null;
            containmentSortingCriterion = null;
            return resultList;
        }

        private void SortContainedComponents(List<AutoSortingComponent> containedComponents)
        {
            AddFirstComponentIfPossible(containedComponents);

            foreach (var containedComponent in containedComponents)
            {
                var containedByAutoSortingComponent = containedComponent.containedByAutoSortingComponent;
                var correspondingIndex = GetCorrespondingAutoSortingComponentIndex(resultList,
                    containedByAutoSortingComponent);
                if (correspondingIndex < 0)
                {
                    Debug.LogWarning("should not happen, break for");
                    break;
                }

                var sortInForeground = containmentSortingCriterionData.isSortingEnclosedSpriteInForeground;

                if (!sortInForeground)
                {
                    var results = containmentSortingCriterion.Sort(containedByAutoSortingComponent, containedComponent,
                        autoSortingCalculationData);

                    if (results[1] > results[0])
                    {
                        sortInForeground = true;
                    }
                }

                if (sortInForeground)
                {
                    var beginCheckIndex = correspondingIndex + 1;
                    containedComponent.sortingOrder =
                        containedByAutoSortingComponent.OriginSortingOrder + 1;

                    if (beginCheckIndex >= resultList.Count)
                    {
                        containedComponent.sortingOrder = resultList[resultList.Count - 1].sortingOrder + 1;
                        resultList.Add(containedComponent);
                        continue;
                    }

                    InsertInResultList(containedComponent, beginCheckIndex);
                }
                else
                {
                    if (correspondingIndex == 0)
                    {
                        containedComponent.sortingOrder =
                            containedByAutoSortingComponent.OriginSortingOrder;
                        InsertInResultListAndIncreaseSortingOrderAfterIndex(containedComponent, 0);

                        continue;
                    }

                    InsertInResultList(containedComponent, 0, correspondingIndex);
                }
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
            firstComponent.sortingOrder = firstComponent.OriginSortingOrder;
        }

        private void SortNotContainedComponents(List<AutoSortingComponent> notContainedComponents)
        {
            AddFirstComponentIfPossible(notContainedComponents);

            foreach (var notContainedComponent in notContainedComponents)
            {
                InsertInResultList(notContainedComponent);
            }
        }

        private void InsertInResultList(AutoSortingComponent currentItem, int beginCheckIndex = 0,
            int endCheckIndex = -1)
        {
            if (beginCheckIndex < 0 || beginCheckIndex > resultList.Count || endCheckIndex > resultList.Count)
            {
                return;
            }

            var isInsertedInResultList = false;
            var endIndex = endCheckIndex < 0 ? resultList.Count : endCheckIndex;
            var lastIndex = beginCheckIndex;

            for (var i = beginCheckIndex; i < endIndex; i++)
            {
                //check against each of the items which are already in the resultList
                lastIndex++;
                var autoSortingComponent = resultList[i];

                var sortingResult = CompareWithSortingCriterias(currentItem, autoSortingComponent);
                if (!sortingResult.isOverlapping)
                {
                    continue;
                }

                if (sortingResult.order < 0)
                {
                    currentItem.sortingOrder = autoSortingComponent.sortingOrder;
                    InsertInResultListAndIncreaseSortingOrderAfterIndex(currentItem, i);

                    isInsertedInResultList = true;
                    break;
                }

                currentItem.sortingOrder = autoSortingComponent.sortingOrder + 1;
            }

            if (!isInsertedInResultList)
            {
                if (endCheckIndex < 0)
                {
                    resultList.Add(currentItem);
                }
                else
                {
                    InsertInResultListAndIncreaseSortingOrderAfterIndex(currentItem, lastIndex);
                }
            }
        }

        private void InsertInResultListAndIncreaseSortingOrderAfterIndex(AutoSortingComponent currentItem, int index)
        {
            resultList.Insert(index, currentItem);

            for (var i = index + 1; i < resultList.Count; i++)
            {
                var sortingComponent = resultList[i];
                if (currentItem.IsOverlapping(sortingComponent))
                {
                    sortingComponent.sortingOrder++;
                }
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

            var resultCounter = new int[2];
            foreach (var sortingCriteria in sortingCriterias)
            {
                var tempResults = sortingCriteria.Sort(unsortedItem, sortedItem, autoSortingCalculationData);

                for (int i = 0; i < resultCounter.Length; i++)
                {
                    resultCounter[i] += tempResults[i];
                }
            }

            Debug.Log("result sum: [" + resultCounter[0] + "," + resultCounter[1] + "]");

            if (resultCounter[0] >= resultCounter[1])
            {
                result.order = 1;
            }
            else
            {
                result.order = -1;
            }

            return result;
        }

        private void AnalyzeContainment(ref List<AutoSortingComponent> autoSortingComponents)
        {
            var spriteContainmentDetector = new SpriteContainmentDetector();

            foreach (var autoSortingComponent in autoSortingComponents)
            {
                var containedSortingComponent = spriteContainmentDetector.DetectContainedBySortingComponent(
                    autoSortingComponent, overlappingSortingComponents, autoSortingCalculationData);

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

                Debug.LogFormat("containment found: {0} in {1} ", containedSortingComponent.OriginSpriteRenderer.name,
                    autoSortingComponent.OriginSpriteRenderer.name);
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
            overlappingSpriteDetector = new OverlappingSpriteDetector();
            var autoSortingComponents = new List<AutoSortingComponent>();

            foreach (var overlappingItem in overlappingSortingComponents)
            {
                var autoSortingComponent = new AutoSortingComponent(overlappingItem);

                autoSortingComponents.Add(autoSortingComponent);
            }

            return autoSortingComponents;
        }
    }
}