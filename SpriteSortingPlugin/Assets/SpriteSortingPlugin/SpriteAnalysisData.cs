using System;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct SpriteAnalysisData
    {
        public double sharpness;
        public float perceivedLightness;
        public Color primaryColor;
        public float averageAlpha;
    }
}