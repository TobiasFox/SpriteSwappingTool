using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SpriteOutlineAnalyzer
    {
        private int startPixelIndex;
        private PixelDirection startPixelEntryDirection;
        private Color[] pixels;
        private int spriteHeight;
        private int spriteWidth;
        private float spritePixelsPerUnit;
        private Vector2 pixelOffset;

        public List<Vector2> Analyze(Sprite sprite)
        {
            var pointList = AnalyzeSpriteOutline(sprite);
            //TODO flatten colliderPoints


            pixels = null;
            return pointList;
        }

        private List<Vector2> AnalyzeSpriteOutline(Sprite sprite)
        {
            var pointList = new List<Vector2>();
            spritePixelsPerUnit = sprite.pixelsPerUnit;
            var spriteTexture = sprite.texture;

            pixels = spriteTexture.GetPixels();
            spriteHeight = spriteTexture.height;
            spriteWidth = spriteTexture.width;
            PixelDirectionUtility.spriteWidth = spriteWidth;

            startPixelIndex = GetFirstSpritePixelIndex(out startPixelEntryDirection);
            if (startPixelIndex < 0)
            {
                Debug.Log("sprite is completely transparent");
                return pointList;
            }

            var rectCenter = sprite.rect.center;
            var halfPixelOffset = 1f / spritePixelsPerUnit / 2f;
            pixelOffset = new Vector2(rectCenter.x / spritePixelsPerUnit - halfPixelOffset,
                rectCenter.y / spritePixelsPerUnit - halfPixelOffset);


            var point = ConvertToColliderPoint(startPixelIndex);
            pointList.Add(point);

            var boundaryPoint = startPixelIndex;
            var pixelDirectionToCheck = PixelDirectionUtility.GetOppositePixelDirection(startPixelEntryDirection);
            var firstEntryDirection = (PixelDirection) ((int) pixelDirectionToCheck);
            var neighbourOfBoundaryPointIndex =
                PixelDirectionUtility.GetIndexOfPixelDirection(startPixelIndex, pixelDirectionToCheck);
            // var counter = 0;
            var neighbourCounter = 0;

            while (neighbourOfBoundaryPointIndex != startPixelIndex)
            {
                if (neighbourOfBoundaryPointIndex == startPixelIndex &&
                    firstEntryDirection == startPixelEntryDirection)
                {
                    break;
                }

                if (neighbourCounter > PixelDirectionUtility.PixelDirections)
                {
                    break;
                }

                if (pixels[neighbourOfBoundaryPointIndex].a > 0)
                {
                    //found pixel
                    var nextPoint = ConvertToColliderPoint(neighbourOfBoundaryPointIndex);
                    pointList.Add(nextPoint);

                    boundaryPoint = neighbourOfBoundaryPointIndex;

                    var backtracedDirection =
                        PixelDirectionUtility.GetBacktracedPixelDirectionClockWise(pixelDirectionToCheck,
                            firstEntryDirection);

                    pixelDirectionToCheck = PixelDirectionUtility.GetOppositePixelDirection(backtracedDirection);
                    firstEntryDirection = (PixelDirection) ((int) pixelDirectionToCheck);

                    neighbourOfBoundaryPointIndex =
                        PixelDirectionUtility.GetIndexOfPixelDirection(boundaryPoint, pixelDirectionToCheck);
                    neighbourCounter = 0;
                }
                else
                {
                    //TODO check against picture boundaries 
                    //move to next clockwise pixel 
                    pixelDirectionToCheck = PixelDirectionUtility.GetNextPixelDirectionClockWise(pixelDirectionToCheck);
                    neighbourOfBoundaryPointIndex =
                        PixelDirectionUtility.GetIndexOfPixelDirection(boundaryPoint, pixelDirectionToCheck);
                    neighbourCounter++;
                }

                // counter++;
            }

            return pointList;
        }

        private Vector2 ConvertToColliderPoint(int pixelIndex)
        {
            var pixelWidth = pixelIndex % spriteWidth;
            var pixelHeight = pixelIndex / spriteWidth;

            var point = new Vector2((pixelWidth / spritePixelsPerUnit),
                (pixelHeight / spritePixelsPerUnit)) - pixelOffset;
            // point *= 1 + (1f / spritePixelsPerUnit);

            return point;
        }

        private int GetFirstSpritePixelIndex(out PixelDirection entryDirection)
        {
            var counter = 0;
            entryDirection = PixelDirection.East;

            for (var y = spriteHeight - 1; y >= 0; y--)
            {
                for (var x = 0; x < spriteWidth; x++)
                {
                    var alphaValue = pixels[counter].a;

                    if (alphaValue > 0)
                    {
                        return counter;
                    }

                    counter++;
                }
            }

            return -1;
        }
    }
}