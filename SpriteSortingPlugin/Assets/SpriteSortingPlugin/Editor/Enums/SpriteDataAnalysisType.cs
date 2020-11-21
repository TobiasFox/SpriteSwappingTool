using System;

namespace SpriteSortingPlugin
{
    [Flags]
    public enum SpriteDataAnalysisType
    {
        Nothing = 0,
        Outline = 1 << 0,
        Blurriness = 1 << 1,
        Lightness = 1 << 2,
        PrimaryColor = 1 << 3,
        All = ~0
    }
}