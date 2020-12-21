using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class ContainmentSortingCriterion : SortingCriterion<SortingCriterionData>
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