using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.AnalyzeActions
{
    public interface ISpriteDataAnalyzer
    {
        void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite, SpriteAnalyzeInputData spriteAnalyzeInputData);
    }
}