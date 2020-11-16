using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis
{
    public class SpriteBrightnessAnalyzer : ISpriteDataAnalyzer
    {
        private BrightnessAnalyzer brightnessAnalyzer;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            if (brightnessAnalyzer == null)
            {
                brightnessAnalyzer = new BrightnessAnalyzer();
            }

            spriteDataItem.spriteAnalysisData.brightness = brightnessAnalyzer.Analyze(sprite);
        }
    }
}