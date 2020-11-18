using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteAnalyzer;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class BrightnessSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private BrightnessSortingCriterionData BrightnessSortingCriterionData =>
            (BrightnessSortingCriterionData) sortingCriterionData;

        private BrightnessAnalyzer brightnessAnalyzer;

        public BrightnessSortingCriterion(BrightnessSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            var blurriness = 0d;
            var otherBlurriness = 0d;

            if (BrightnessSortingCriterionData.isUsingSpriteRendererColor)
            {
                if (brightnessAnalyzer == null)
                {
                    brightnessAnalyzer = new BrightnessAnalyzer();
                }

                blurriness = brightnessAnalyzer.Analyze(autoSortingComponent.spriteRenderer);
                otherBlurriness = brightnessAnalyzer.Analyze(otherAutoSortingComponent.spriteRenderer);
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

            if (BrightnessSortingCriterionData.isLighterSpriteIsInForeground)
            {
                results[isAutoSortingComponentIsLighter ? 0 : 1]++;
            }
            else
            {
                results[!isAutoSortingComponentIsLighter ? 0 : 1]++;
            }

            return results;
        }

        public override bool IsUsingSpriteData()
        {
            return BrightnessSortingCriterionData.isUsingSpriteColor;
        }
    }
}