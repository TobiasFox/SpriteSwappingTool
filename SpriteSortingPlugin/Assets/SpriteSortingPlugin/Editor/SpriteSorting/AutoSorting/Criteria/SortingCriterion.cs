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

using SpriteSortingPlugin.SpriteSorting.AutoSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.AutoSorting.Criteria
{
    public abstract class SortingCriterion
    {
        protected readonly SortingCriterionData sortingCriterionData;
        protected SpriteDataItemValidator spriteDataItemValidator;
        protected SpriteDataItemValidator otherSpriteDataItemValidator;
        protected AutoSortingCalculationData autoSortingCalculationData;
        protected int[] sortingResults;
        protected SortingCriterionType sortingCriterionType;

        public SortingCriterionType SortingCriterionType => sortingCriterionType;

        protected SortingCriterion(SortingCriterionData sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
        }

        public float[] Sort(SortingComponent sortingComponent, SortingComponent otherSortingComponent,
            AutoSortingCalculationData autoSortingCalculationData)
        {
            this.autoSortingCalculationData = autoSortingCalculationData;
            sortingResults = new int[2];
            var weightedResult = new float[2];

            if (IsUsingSpriteData())
            {
                var isValidForSorting = ValidateSortingComponentsForSorting(sortingComponent, otherSortingComponent);
                if (!isValidForSorting)
                {
                    return weightedResult;
                }
            }

            InternalSort(sortingComponent, otherSortingComponent);

            for (var i = 0; i < sortingResults.Length; i++)
            {
                weightedResult[i] = sortingResults[i] * sortingCriterionData.priority;
            }

            // Debug.Log(GetType().Name + " sorted: [" + sortingResults[0] + "," + sortingResults[1] + "]");

            return weightedResult;
        }

        private bool ValidateSortingComponentsForSorting(SortingComponent sortingComponent,
            SortingComponent otherSortingComponent)
        {
            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(sortingComponent.SpriteRenderer);
            otherSpriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(otherSortingComponent.SpriteRenderer);

            if (spriteDataItemValidator == null || otherSpriteDataItemValidator == null)
            {
                return false;
            }

            var isContainingSpriteData = autoSortingCalculationData.spriteData.spriteDataDictionary.ContainsKey(
                spriteDataItemValidator
                    .AssetGuid);
            var isContainingOtherSpriteData =
                autoSortingCalculationData.spriteData.spriteDataDictionary.ContainsKey(spriteDataItemValidator
                    .AssetGuid);

            return isContainingSpriteData && isContainingOtherSpriteData;
        }

        protected abstract void InternalSort(SortingComponent sortingComponent,
            SortingComponent otherSortingComponent);

        public abstract bool IsUsingSpriteData();
    }
}