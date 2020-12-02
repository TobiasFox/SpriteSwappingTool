using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis
{
    public struct SpriteAnalyzeInputData
    {
        public OutlineAnalysisType outlineAnalysisType;
        public string assetGuid;
        public bool isAllowingSpriteReimport;
        public SpriteData spriteData;
        public Sprite sprite;
    }
}