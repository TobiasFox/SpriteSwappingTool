using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.AnalyzeActions
{
    public class SpritePrimaryColorAnalyzer : ISpriteDataAnalyzer
    {
        private PrimaryColorAnalyzer primaryColorAnalyzer;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            if (primaryColorAnalyzer == null)
            {
                primaryColorAnalyzer = new PrimaryColorAnalyzer();
            }

            spriteDataItem.spriteAnalysisData.primaryColor = primaryColorAnalyzer.Analyze(sprite);
        }
    }
}