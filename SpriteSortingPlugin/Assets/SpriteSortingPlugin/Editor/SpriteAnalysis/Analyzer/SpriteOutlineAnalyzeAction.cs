using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public class SpriteOutlineAnalyzeAction : ISpriteDataAnalyzer
    {
        private SpriteAnalyzer.SpriteOutlineAnalyzer outlineAnalyzer;
        private OOBBGenerator oOBBGenerator;

        public void Analyse(ref SpriteDataItem spriteDataItem, Sprite sprite,
            SpriteAnalyzeInputData spriteAnalyzeInputData)
        {
            var outlineType = spriteAnalyzeInputData.outlineAnalysisType;

            if (outlineType == OutlineAnalysisType.Nothing)
            {
                return;
            }

            if (outlineType.HasFlag(OutlineAnalysisType.ObjectOrientedBoundingBox))
            {
                var oobb = GenerateOOBB(sprite);
                spriteDataItem.objectOrientedBoundingBox = oobb;
                // currentProgress++;
            }

            if (outlineType.HasFlag(OutlineAnalysisType.PixelPerfect))
            {
                var colliderPoints = GenerateAlphaOutline(sprite);
                spriteDataItem.outlinePoints = colliderPoints;
                // currentProgress++;
            }
        }

        private Vector2[] GenerateAlphaOutline(Sprite sprite)
        {
            if (outlineAnalyzer == null)
            {
                outlineAnalyzer = new SpriteAnalyzer.SpriteOutlineAnalyzer();
            }

            var points = outlineAnalyzer.Analyze(sprite);
            return points;
        }

        private ObjectOrientedBoundingBox GenerateOOBB(Sprite sprite)
        {
            if (oOBBGenerator == null)
            {
                oOBBGenerator = new OOBBGenerator();
            }

            var oobb = oOBBGenerator.Generate(sprite);
            return oobb;
        }
    }
}