using SpriteSortingPlugin.AutomaticSorting.Data;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class BlurrinessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private BlurrinessSortingCriterionData BlurrinessSortingCriterionData =>
            (BlurrinessSortingCriterionData) sortingCriterionData;

        public BlurrinessSortingCriterion(BlurrinessSortingCriterionData sortingCriterionData) : base(
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
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.blurriness;


            var isAutoSortingComponentIsMoreBlurry = blurriness >= otherBlurriness;

            if (BlurrinessSortingCriterionData.isMoreBlurrySpriteInForeground)
            {
                results[isAutoSortingComponentIsMoreBlurry ? 0 : 1]++;
            }
            else
            {
                results[!isAutoSortingComponentIsMoreBlurry ? 0 : 1]++;
            }

            return results;
        }
    }
}