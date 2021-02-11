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

using SpriteSortingPlugin.SortingGeneration.Data;
using SpriteSortingPlugin.SpriteSwappingDetector;

namespace SpriteSortingPlugin.SortingGeneration.Criteria
{
    public class ContainmentSortingCriterion : SortingCriterion
    {
        private ContainmentSortingCriterionData ContainmentSortingCriterionData =>
            (ContainmentSortingCriterionData) sortingCriterionData;

        public bool IsSortingEnclosedSpriteInForeground =>
            ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground;

        public ContainmentSortingCriterion(ContainmentSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
            sortingCriterionType = SortingCriterionType.Containment;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            if (ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground ||
                !ContainmentSortingCriterionData.isCheckingAlpha)
            {
                return;
            }

            var alpha = autoSortingCalculationData.spriteData.spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.averageAlpha;

            alpha *= sortingComponent.SpriteRenderer.color.a;

            if (alpha < ContainmentSortingCriterionData.alphaThreshold)
            {
                sortingResults[1]++;
            }
        }

        public override bool IsUsingSpriteData()
        {
            return !ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground &&
                   ContainmentSortingCriterionData.isCheckingAlpha;
        }
    }
}