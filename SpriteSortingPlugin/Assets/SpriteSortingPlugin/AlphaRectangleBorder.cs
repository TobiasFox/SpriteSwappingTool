using System;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct AlphaRectangleBorder : ICloneable
    {
        public int topBorder;
        public int leftBorder;
        public int bottomBorder;
        public int rightBorder;

        public int spriteWidth;
        public int spriteHeight;
        public float pixelPerUnit;

        public object Clone()
        {
            return new AlphaRectangleBorder
            {
                topBorder = this.topBorder,
                leftBorder = this.leftBorder,
                bottomBorder = this.bottomBorder,
                rightBorder = this.rightBorder,
                spriteHeight = this.spriteHeight,
                spriteWidth = this.spriteWidth,
                pixelPerUnit = this.pixelPerUnit
            };
        }
    }
}