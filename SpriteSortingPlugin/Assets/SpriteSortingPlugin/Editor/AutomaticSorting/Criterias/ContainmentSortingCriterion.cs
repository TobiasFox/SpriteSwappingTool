using SpriteSortingPlugin.AutomaticSorting.Data;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class ContainmentSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private ContainmentSortingCriterionData ContainmentSortingCriterionData =>
            (ContainmentSortingCriterionData) sortingCriterionData;

        public ContainmentSortingCriterion(ContainmentSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
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

            alpha *= sortingComponent.spriteRenderer.color.a;

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