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

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            var alpha = 0f;

            if (ContainmentSortingCriterionData.isUsingSpriteRendererColor)
            {
                alpha = autoSortingComponent.OriginSpriteRenderer.color.a;
            }
            else
            {
                alpha = autoSortingCalculationData.spriteData.spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                    .spriteAnalysisData.averageAlpha;
            }

            if (alpha < ContainmentSortingCriterionData.alphaThreshold)
            {
                results[1]++;
            }

            return results;
        }

        public override bool IsUsingSpriteData()
        {
            return !ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground &&
                   ContainmentSortingCriterionData.isCheckingAlpha &&
                   ContainmentSortingCriterionData.isUsingSpriteColor;
        }
    }
}