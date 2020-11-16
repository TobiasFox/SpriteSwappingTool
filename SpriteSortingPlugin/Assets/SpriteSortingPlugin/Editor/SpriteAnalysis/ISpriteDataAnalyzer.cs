using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis
{
    public interface ISpriteDataAnalyzer
    {
        void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite, SpriteAnalyzeInputData spriteAnalyzeInputData);
    }
}