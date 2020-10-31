using System;

namespace SpriteSortingPlugin
{
    [Flags]
    public enum OutlineType
    {
        Nothing = 0,
        OOBB = 1 << 0,
        Outline = 1 << 1,
        All = ~0
    }
}