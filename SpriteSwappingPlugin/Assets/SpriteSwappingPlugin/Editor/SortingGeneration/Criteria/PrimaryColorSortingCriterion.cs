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

using SpriteSwappingPlugin.SortingGeneration.Data;
using SpriteSwappingPlugin.SpriteSwappingDetector;
using UnityEngine;

namespace SpriteSwappingPlugin.SortingGeneration.Criteria
{
    public class PrimaryColorSortingCriterion : SortingCriterion
    {
        private PrimaryColorSortingCriterionData PrimaryColorSortingCriterionData =>
            (PrimaryColorSortingCriterionData) sortingCriterionData;

        public PrimaryColorSortingCriterion(PrimaryColorSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
            sortingCriterionType = SortingCriterionType.PrimaryColor;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            var primaryColor = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid].spriteAnalysisData.primaryColor;

            var otherPrimaryColor = autoSortingCalculationData.spriteData
                .spriteDataDictionary[otherSpriteDataItemValidator.AssetGuid].spriteAnalysisData.primaryColor;

            primaryColor *= sortingComponent.SpriteRenderer.color;
            otherPrimaryColor *= otherSortingComponent.SpriteRenderer.color;

            for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
            {
                if (!PrimaryColorSortingCriterionData.activeChannels[i])
                {
                    continue;
                }

                var isChannelInForeground = IsInForeground(primaryColor, otherPrimaryColor, i);

                if (isChannelInForeground)
                {
                    sortingResults[0]++;
                }
                else
                {
                    sortingResults[1]++;
                }
            }
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }

        private bool IsInForeground(Color primaryColor, Color otherPrimaryColor, int channel)
        {
            var from = PrimaryColorSortingCriterionData.backgroundColor[channel];
            var to = PrimaryColorSortingCriterionData.foregroundColor[channel];
            var primaryChannel = primaryColor[channel];
            var otherPrimaryChannel = otherPrimaryColor[channel];

            var tPrimary = Mathf.InverseLerp(from, to, primaryChannel);
            var tOtherPrimary = Mathf.InverseLerp(from, to, otherPrimaryChannel);

            return tPrimary >= tOtherPrimary;
        }
    }
}