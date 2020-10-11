using System;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct AlphaRectangleBorder
    {
        public int topBorder;
        public int leftBorder;
        public int bottomBorder;
        public int rightBorder;

        public int spriteWidth;
        public int spriteHeight;
        public float pixelPerUnit;
    }
}