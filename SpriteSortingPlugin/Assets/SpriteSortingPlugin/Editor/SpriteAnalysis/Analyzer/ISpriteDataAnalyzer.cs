using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public interface ISpriteDataAnalyzer
    {
        void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite, SpriteAnalyzeInputData spriteAnalyzeInputData);
    }
}