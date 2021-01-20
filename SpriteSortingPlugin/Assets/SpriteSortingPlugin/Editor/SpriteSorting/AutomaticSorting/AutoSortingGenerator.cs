#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System.Collections.Generic;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting
{
    public class AutoSortingGenerator
    {
        private readonly List<SortingCriterion> sortingCriteria = new List<SortingCriterion>();

        private AutoSortingCalculationData autoSortingCalculationData;
        private List<AutoSortingComponent> resultList;
        private List<SortingComponent> overlappingSortingComponents;
        private SortingComponent baseItem;
        private ContainmentSortingCriterion containmentSortingCriterion;

        public void AddSortingCriterion(SortingCriterion sortingCriterion)
        {
            sortingCriteria.Add(sortingCriterion);
        }

        public void SetContainmentCriterion(ContainmentSortingCriterion containmentSortingCriterion)
        {
            this.containmentSortingCriterion = containmentSortingCriterion;
        }

        public List<AutoSortingComponent> GenerateAutomaticSortingOrder(SortingComponent baseItem,
            List<SortingComponent> overlappingSortingComponents, AutoSortingCalculationData autoSortingCalculationData,
            out List<SortingCriterionType> skippedSortingCriteria)
        {
            resultList = new List<AutoSortingComponent>();
            skippedSortingCriteria = new List<SortingCriterionType>();

            if (overlappingSortingComponents == null || overlappingSortingComponents.Count + 1 <= 1)
            {
                return resultList;
            }

            this.overlappingSortingComponents = new List<SortingComponent>(overlappingSortingComponents);
            this.baseItem = baseItem;
            this.autoSortingCalculationData = autoSortingCalculationData;
            this.overlappingSortingComponents.Insert(0, baseItem);
            var autoSortingComponents = InitAutoSortingComponents();

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(this.autoSortingCalculationData.spriteData);

            var hasValidSortingCriteria = ValidateSortingCriteria(out skippedSortingCriteria);
            if (!hasValidSortingCriteria)
            {
                spriteDataItemValidatorCache.Clear();
                return autoSortingComponents;
            }

            if (containmentSortingCriterion != null)
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

            containmentSortingCriterion = null;
            return resultList;
        }

        private bool ValidateSortingCriteria(out List<SortingCriterionType> skippedSortingCriteria)
        {
            skippedSortingCriteria = new List<SortingCriterionType>();

            var isUsingSpriteData = false;
            foreach (var sortingCriterion in sortingCriteria)
            {
                isUsingSpriteData |= sortingCriterion.IsUsingSpriteData();
            }

            if (containmentSortingCriterion != null)
            {
                isUsingSpriteData |= containmentSortingCriterion.IsUsingSpriteData();
            }

            if (!isUsingSpriteData)
            {
                return true;
            }

            if (autoSortingCalculationData.spriteData == null)
            {
                return false;
            }

            var collectedGuidList = new List<string>();

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            foreach (var sortingComponent in overlappingSortingComponents)
            {
                var spriteDataItemValidator =
                    spriteDataItemValidatorCache.GetOrCreateValidator(sortingComponent.SpriteRenderer);
                if (spriteDataItemValidator == null)
                {
                    continue;
                }

                collectedGuidList.Add(spriteDataItemValidator.AssetGuid);
            }

            var isAllGuidsContained =
                autoSortingCalculationData.spriteData.SpriteDataDictionaryContainsAllGuids(collectedGuidList);

            foreach (var sortingCriterion in sortingCriteria)
            {
                if (sortingCriterion.IsUsingSpriteData() && !isAllGuidsContained)
                {
                    skippedSortingCriteria.Add(sortingCriterion.SortingCriterionType);
                }
            }

            if (containmentSortingCriterion != null && containmentSortingCriterion.IsUsingSpriteData())
            {
                skippedSortingCriteria.Add(containmentSortingCriterion.SortingCriterionType);
            }

            var isSkippingAllSortingCriteria = skippedSortingCriteria.Count == sortingCriteria.Count;
            return !isSkippingAllSortingCriteria;
        }

        private void SortContainedComponents(List<AutoSortingComponent> containedComponents)
        {
            if (containedComponents == null || containedComponents.Count == 0)
            {
                return;
            }

            AddFirstComponentIfPossible(containedComponents);

            foreach (var containedComponent in containedComponents)
            {
                var containedByAutoSortingComponent = containedComponent.containedByAutoSortingComponent;
                var correspondingIndex = GetCorrespondingAutoSortingComponentIndex(resultList,
                    containedByAutoSortingComponent);
                if (correspondingIndex < 0)
                {
                    Debug.LogWarning("should not happen -> continue. Is probably a cycle " +
                                     containedComponent.sortingComponent + " " + containedByAutoSortingComponent);
                    continue;
                }

                var sortInForeground = containmentSortingCriterion.IsSortingEnclosedSpriteInForeground;

                if (!sortInForeground)
                {
                    var results = containmentSortingCriterion.Sort(containedByAutoSortingComponent,
                        containedComponent.sortingComponent, autoSortingCalculationData);

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
            firstComponent.sortingOrder = firstComponent.sortingComponent.OriginSortingOrder;
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
                var sortedComponent = resultList[i];

                var sortingResult = CompareWithSortingCriterias(currentItem, sortedComponent);
                if (!sortingResult.isOverlapping)
                {
                    continue;
                }

                if (sortingResult.order < 0)
                {
                    currentItem.sortingOrder = sortedComponent.sortingOrder;
                    InsertInResultListAndIncreaseSortingOrderAfterIndex(currentItem, i);

                    isInsertedInResultList = true;
                    break;
                }

                currentItem.sortingOrder = sortedComponent.sortingOrder + 1;
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
                var autoSortingComponent = resultList[i];
                if (currentItem.sortingComponent.IsOverlapping(autoSortingComponent.sortingComponent))
                {
                    autoSortingComponent.sortingOrder++;
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
            var result = new AutoSortingResult
                {isOverlapping = unsortedItem.sortingComponent.IsOverlapping(sortedItem.sortingComponent)};

            if (!result.isOverlapping)
            {
                return result;
            }

            var resultCounter = new float[2];
            foreach (var sortingCriterion in sortingCriteria)
            {
                var tempResults = sortingCriterion.Sort(unsortedItem.sortingComponent, sortedItem.sortingComponent,
                    autoSortingCalculationData);

                for (var i = 0; i < resultCounter.Length; i++)
                {
                    resultCounter[i] += tempResults[i];
                }
            }

            // Debug.Log("result sum: [" + resultCounter[0] + "," + resultCounter[1] + "]");

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
                    autoSortingComponent.sortingComponent, overlappingSortingComponents, autoSortingCalculationData);

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
                    autoSortingComponents[correspondingAutoSortingComponentIndex].sortingComponent;

                // Debug.LogFormat("containment found: {0} in {1} ", containedSortingComponent.SpriteRenderer.name,
                // autoSortingComponent.sortingComponent.SpriteRenderer.name);
            }
        }

        private int GetCorrespondingAutoSortingComponentIndex(List<AutoSortingComponent> autoSortingComponents,
            SortingComponent sortingComponent)
        {
            for (var i = 0; i < autoSortingComponents.Count; i++)
            {
                if (!autoSortingComponents[i].sortingComponent.Equals(sortingComponent))
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        private List<AutoSortingComponent> InitAutoSortingComponents()
        {
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