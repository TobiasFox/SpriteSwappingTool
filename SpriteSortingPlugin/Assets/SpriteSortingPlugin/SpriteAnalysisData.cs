using System;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct SpriteAnalysisData
    {
        public double blurriness;
        public float perceivedLightness;
        public Color primaryColor;
    }
}