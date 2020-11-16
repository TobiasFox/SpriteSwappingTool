using SpriteSortingPlugin.AutomaticSorting.Data;
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

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            Color primaryColor;
            Color otherPrimaryColor;

            if (PrimaryColorSortingCriterionData.isUsingSpriteColor)
            {
                primaryColor = autoSortingCalculationData.spriteData
                    .spriteDataDictionary[spriteDataItemValidator.AssetGuid].spriteAnalysisData.primaryColor;

                otherPrimaryColor = autoSortingCalculationData.spriteData
                    .spriteDataDictionary[spriteDataItemValidator.AssetGuid].spriteAnalysisData.primaryColor;
            }
            else
            {
                primaryColor = autoSortingComponent.spriteRenderer.color;
                otherPrimaryColor = otherAutoSortingComponent.spriteRenderer.color;
            }

            for (var i = 0; i < 3; i++)
            {
                var isChannelInForeground = IsInForeground(primaryColor, otherPrimaryColor, i);

                if (isChannelInForeground)
                {
                    results[0]++;
                }
                else
                {
                    results[1]++;
                }
            }

            return results;
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