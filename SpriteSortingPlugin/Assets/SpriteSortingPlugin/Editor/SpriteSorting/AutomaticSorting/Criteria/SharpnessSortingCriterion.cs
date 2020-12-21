using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class SharpnessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public SharpnessSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
            sortingCriterionType = DefaultSortingCriterionData.sortingCriterionType;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            var sharpness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.sharpness;

            var otherSharpness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[otherSpriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.sharpness;


            var isAutoSortingComponentIsSharper = sharpness >= otherSharpness;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingComponentIsSharper ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentIsSharper ? 0 : 1]++;
            }
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }
    }
}