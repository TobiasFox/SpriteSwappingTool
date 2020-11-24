using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public class SpriteBlurrinessAnalyzer : ISpriteDataAnalyzer
    {
        private BlurrinessAnalyzer blurrinessAnalyzer;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            if (blurrinessAnalyzer == null)
            {
                blurrinessAnalyzer = new BlurrinessAnalyzer();
            }

            spriteDataItem.spriteAnalysisData.blurriness = blurrinessAnalyzer.Analyze(sprite);
        }
    }
}