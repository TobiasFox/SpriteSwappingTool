using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteAnalyzer;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class LightnessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private LightnessSortingCriterionData LightnessSortingCriterionData =>
            (LightnessSortingCriterionData) sortingCriterionData;

        private LightnessAnalyzer lightnessAnalyzer;

        public LightnessSortingCriterion(LightnessSortingCriterionData sortingCriterionData) : base(
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

            if (LightnessSortingCriterionData.isUsingSpriteRendererColor)
            {
                if (lightnessAnalyzer == null)
                {
                    lightnessAnalyzer = new LightnessAnalyzer();
                }

                //TODO calc lightness with spriterenderer colour
                perceivedLightness = lightnessAnalyzer.Analyze(autoSortingComponent.OriginSpriteRenderer);
                otherPerceivedLightness = lightnessAnalyzer.Analyze(otherAutoSortingComponent.OriginSpriteRenderer);
            }


            var isAutoSortingComponentIsLighter = perceivedLightness >= otherPerceivedLightness;

            if (LightnessSortingCriterionData.isLighterSpriteIsInForeground)
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