using SpriteSortingPlugin.SpriteAnalyzer;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class LightnessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        private LightnessAnalyzer lightnessAnalyzer;

        public LightnessSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
            sortingCriterionType = DefaultSortingCriterionData.sortingCriterionType;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            var perceivedLightness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.perceivedLightness;

            var otherPerceivedLightness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[otherSpriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.perceivedLightness;

            if (lightnessAnalyzer == null)
            {
                lightnessAnalyzer = new LightnessAnalyzer();
            }

            perceivedLightness = lightnessAnalyzer.ApplySpriteRendererColor(perceivedLightness,
                sortingComponent.SpriteRenderer.color);

            otherPerceivedLightness = lightnessAnalyzer.ApplySpriteRendererColor(otherPerceivedLightness,
                otherSortingComponent.SpriteRenderer.color);

            var isAutoSortingComponentIsLighter = perceivedLightness >= otherPerceivedLightness;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingComponentIsLighter ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentIsLighter ? 0 : 1]++;
            }
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }
    }
}