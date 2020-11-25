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
            var blurriness = 0d;
            var otherBlurriness = 0d;

            if (LightnessSortingCriterionData.isUsingSpriteRendererColor)
            {
                if (lightnessAnalyzer == null)
                {
                    lightnessAnalyzer = new LightnessAnalyzer();
                }

                blurriness = lightnessAnalyzer.Analyze(autoSortingComponent.OriginSpriteRenderer);
                otherBlurriness = lightnessAnalyzer.Analyze(otherAutoSortingComponent.OriginSpriteRenderer);
            }
            else
            {
                blurriness = autoSortingCalculationData.spriteData
                    .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                    .spriteAnalysisData.blurriness;

                otherBlurriness = autoSortingCalculationData.spriteData
                    .spriteDataDictionary[spriteDataItemValidator.AssetGuid]
                    .spriteAnalysisData.blurriness;
            }


            var isAutoSortingComponentIsLighter = blurriness >= otherBlurriness;

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
            return LightnessSortingCriterionData.isUsingSpriteColor;
        }
    }
}