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

using SpriteSortingPlugin.SpriteAnalyzer;
using SpriteSortingPlugin.SpriteSorting.AutoSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.AutoSorting.Criteria
{
    public class LightnessSortingCriterion : SortingCriterion
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        private LightnessAnalyzer lightnessAnalyzer;

        public LightnessSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
            sortingCriterionType = DefaultSortingCriterionData.sortingCriterionType;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            var perceivedLightness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.perceivedLightness;

            var otherPerceivedLightness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[otherSpriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.perceivedLightness;

            if (lightnessAnalyzer == null)
            {
                lightnessAnalyzer = new LightnessAnalyzer();
            }

            perceivedLightness = lightnessAnalyzer.ApplySpriteRendererColor(perceivedLightness,
                sortingComponent.SpriteRenderer.color);

            otherPerceivedLightness = lightnessAnalyzer.ApplySpriteRendererColor(otherPerceivedLightness,
                otherSortingComponent.SpriteRenderer.color);

            var isAutoSortingComponentIsLighter = perceivedLightness >= otherPerceivedLightness;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingComponentIsLighter ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentIsLighter ? 0 : 1]++;
            }
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }
    }
}