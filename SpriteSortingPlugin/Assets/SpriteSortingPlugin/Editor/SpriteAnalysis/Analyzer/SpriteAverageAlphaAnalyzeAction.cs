using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public class SpriteAverageAlphaAnalyzer : ISpriteDataAnalyzer
    {
        private AverageAlphaAnalyzer averageAlphaAnalyzer;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            if (averageAlphaAnalyzer == null)
            {
                averageAlphaAnalyzer = new AverageAlphaAnalyzer();
            }

            spriteDataItem.spriteAnalysisData.averageAlpha = averageAlphaAnalyzer.Analyze(sprite);
        }
    }
}