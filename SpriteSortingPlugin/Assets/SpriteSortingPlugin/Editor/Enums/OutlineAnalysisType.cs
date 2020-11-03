using System;

namespace SpriteSortingPlugin
{
    [Flags]
    public enum OutlineAnalysisType
    {
        Nothing = 0,
        ObjectOrientedBoundingBox = 1 << 0,
        PixelPerfect = 1 << 1,
        All = ~0
    }
}