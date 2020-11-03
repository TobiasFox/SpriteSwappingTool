using System;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class OOBBGenerator
    {
        private int[] borders;
        
        public ObjectOrientedBoundingBox Generate(Sprite sprite)
        {
            // var startTime = EditorApplication.timeSinceStartup;
            var spriteTexture = sprite.texture;
            var pixelsPerUnit = sprite.pixelsPerUnit;
            var pixelArray = spriteTexture.GetPixels();
            
            borders = new int[] {spriteTexture.height, spriteTexture.width, 0, 0};
            var counter = 0;

            for (int y = 0; y < spriteTexture.height; y++)
            {
                for (int x = 0; x < spriteTexture.width; x++)
                {
                    var color = pixelArray[counter];

                    AnalyzeOutmostAlpha(x, y, color.a);
                    counter++;
                }
            }

            //more performant around 0,124s
            // for (int i = 0; i < pixelArray.Length; i++)
            // {
            //     var color = pixelArray[i];
            //
            //     if (color.a == 0)
            //     {
            //         continue;
            //     }
            //
            //     var tempHeight = i % spriteTexture.width;
            //     var row = i / spriteTexture.width;
            //
            //     AnalyzeOutmostAlpha(tempHeight, row, color.a);
            //     counter++;
            // }

            //offset of about 1 pixel in all directions
            borders[0] = Math.Max(borders[0] - 1, 0);
            borders[1] = Math.Max(borders[1] - 1, 0);
            borders[2] = Math.Min(borders[2] + 1, spriteTexture.height);
            borders[3] = Math.Min(borders[3] + 1, spriteTexture.width);

            var alphaRectangleBorder = new AlphaRectangleBorder
            {
                topBorder = borders[0],
                leftBorder = borders[1],
                bottomBorder = spriteTexture.height - borders[2],
                rightBorder = spriteTexture.width - borders[3],
                spriteHeight = spriteTexture.height,
                spriteWidth = spriteTexture.width,
                pixelPerUnit = pixelsPerUnit
            };

            var oobb = new ObjectOrientedBoundingBox(alphaRectangleBorder, Vector2.zero, 0);
            return oobb;
        }
        
        private void AnalyzeOutmostAlpha(int x, int y, float alpha)
        {
            if (alpha == 0)
            {
                return;
            }

            //top
            if (y < borders[0])
            {
                borders[0] = y;
            }

            //left
            if (x < borders[1])
            {
                borders[1] = x;
            }

            //bottom
            if (y > borders[2])
            {
                borders[2] = y;
            }

            //right
            if (x > borders[3])
            {
                borders[3] = x;
            }
        }
    }
}
