﻿using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class PrimaryColorSortingCriterion : SortingCriterion<SortingCriterionData>
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