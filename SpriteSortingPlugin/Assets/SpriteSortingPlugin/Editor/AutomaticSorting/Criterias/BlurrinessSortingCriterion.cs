using SpriteSortingPlugin.AutomaticSorting.Data;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class BlurrinessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public BlurrinessSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            var blurriness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.blurriness;

            var otherBlurriness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[otherSpriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.blurriness;


            var isAutoSortingComponentIsMoreBlurry = blurriness >= otherBlurriness;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                results[isAutoSortingComponentIsMoreBlurry ? 0 : 1]++;
            }
            else
            {
                results[!isAutoSortingComponentIsMoreBlurry ? 0 : 1]++;
            }

            return results;
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }
    }
}