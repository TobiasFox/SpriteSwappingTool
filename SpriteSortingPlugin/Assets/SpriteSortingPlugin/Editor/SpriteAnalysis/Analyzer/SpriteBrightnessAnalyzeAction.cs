using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public class SpriteBrightnessAnalyzeAction : ISpriteDataAnalyzer
    {
        private LightnessAnalyzer lightnessAnalyzer;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            if (lightnessAnalyzer == null)
            {
                lightnessAnalyzer = new LightnessAnalyzer();
            }

            spriteDataItem.spriteAnalysisData.perceivedLightness = lightnessAnalyzer.Analyze(sprite);
        }
    }
}