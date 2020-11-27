﻿using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class PrimaryColorSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private PrimaryColorSortingCriterionData PrimaryColorSortingCriterionData =>
            (PrimaryColorSortingCriterionData) sortingCriterionData;

        public PrimaryColorSortingCriterion(PrimaryColorSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override void InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var primaryColor = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid].spriteAnalysisData.primaryColor;

            var otherPrimaryColor = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid].spriteAnalysisData.primaryColor;

            if (PrimaryColorSortingCriterionData.isUsingSpriteRendererColor)
            {
                primaryColor *= autoSortingComponent.OriginSpriteRenderer.color;
                otherPrimaryColor *= otherAutoSortingComponent.OriginSpriteRenderer.color;
            }

            for (var i = 0; i < 3; i++)
            {
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