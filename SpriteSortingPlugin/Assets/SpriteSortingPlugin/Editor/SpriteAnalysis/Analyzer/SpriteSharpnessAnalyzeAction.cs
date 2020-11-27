using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public class SpriteSharpnessAnalyzer : ISpriteDataAnalyzer
    {
        private SharpnessAnalyzer sharpnessAnalyzer;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            if (sharpnessAnalyzer == null)
            {
                sharpnessAnalyzer = new SharpnessAnalyzer();
            }

            spriteDataItem.spriteAnalysisData.sharpness = sharpnessAnalyzer.Analyze(sprite);
        }
    }
}