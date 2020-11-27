using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteAnalyzer;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class LightnessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        private LightnessAnalyzer lightnessAnalyzer;

        public LightnessSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override void InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var perceivedLightness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.perceivedLightness;

            var otherPerceivedLightness = autoSortingCalculationData.spriteData
                .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                .spriteAnalysisData.perceivedLightness;

            if (lightnessAnalyzer == null)
            {
                lightnessAnalyzer = new LightnessAnalyzer();
            }

            perceivedLightness = lightnessAnalyzer.ApplySpriteRendererColor(perceivedLightness,
                autoSortingComponent.OriginSpriteRenderer.color);

            otherPerceivedLightness = lightnessAnalyzer.ApplySpriteRendererColor(otherPerceivedLightness,
                otherAutoSortingComponent.OriginSpriteRenderer.color);

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