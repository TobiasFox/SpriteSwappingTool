using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalyzer
{
    public class SpriteOutlineAnalyzer
    {
        private const float Tolerance = 0.0001f;

        private int startPixelIndex;
        private PixelDirection startPixelEntryDirection;
        private Color[] pixels;
        private int spriteHeight;
        private int spriteWidth;
        private float spritePixelsPerUnit;
        private Vector2 pixelOffset;

        private Vector2 origin;
        private Vector2 direction;

        public Vector2[] Analyze(Sprite sprite)
        {
            var pointList = AnalyzeSpriteOutline(sprite);

            FlattenPointList(ref pointList);

            var first = pointList[0];
            if (!first.Equals(pointList[pointList.Count - 1]))
            {
                pointList.Add(first);
            }

            pixels = null;
            return pointList.ToArray();
        }

        public bool Simplify(Vector2[] points, float outlineTolerance, out Vector2[] simplifiedPoints)
        {
            var convertedV3Points = new Vector3[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                convertedV3Points[i] = (Vector3) points[i];
            }

            var lineRendererGameObject = new GameObject("LineRendererHelper")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var lineRenderer = lineRendererGameObject.AddComponent<LineRenderer>();

            lineRenderer.positionCount = convertedV3Points.Length;
            lineRenderer.SetPositions(convertedV3Points);

            lineRenderer.Simplify(outlineTolerance);

            //validate min points
            if (lineRenderer.positionCount < 4)
            {
                simplifiedPoints = null;
                return false;
            }

            var positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);

            Object.DestroyImmediate(lineRendererGameObject);

            simplifiedPoints = new Vector2[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                simplifiedPoints[i] = (Vector2) positions[i];
            }

            return true;
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
                    neighbourCounter = 1;
                }
                else
                {
                    //TODO check against picture boundaries 
                    //move to next clockwise pixel 
                    pixelDirectionToCheck = PixelDirectionUtility.GetNextPixelDirectionClockWise(pixelDirectionToCheck);
                    neighbourOfBoundaryPointIndex =
                        PixelDirectionUtility.GetIndexOfPixelDirection(boundaryPoint, pixelDirectionToCheck);
                    neighbourCounter++;

                    // var boundaryWidth=boundaryPoint & spriteWidth;
                    //
                    // for (; neighbourCounter < PixelDirectionUtility.PixelDirections; neighbourCounter++)
                    // {
                    //    
                    //
                    //     if (neighbourOfBoundaryPointIndex < 0 || neighbourOfBoundaryPointIndex >= pixels.Length)
                    //     {
                    //         continue;
                    //     }
                    //     
                    //     var currentWidth = neighbourOfBoundaryPointIndex & spriteWidth;
                    //     var currentHeight = neighbourOfBoundaryPointIndex / spriteWidth;
                    //
                    //     if (boundaryWidth)
                    //     {
                    //         continue;
                    //     }
                    //
                    //     break;
                    // }
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

        private void FlattenPointList(ref List<Vector2> pointList)
        {
            if (pointList.Count < 3)
            {
                return;
            }

            var currentIndex = 0;
            origin = pointList[currentIndex++];
            direction = pointList[currentIndex++] - origin;

            while (currentIndex < pointList.Count - 1)
            {
                var nextPoint = pointList[currentIndex];
                if (IsOnLine(nextPoint.x, nextPoint.y))
                {
                    pointList.RemoveAt(currentIndex - 1);
                    continue;
                }

                origin = pointList[currentIndex - 1];
                direction = nextPoint - origin;

                currentIndex++;
            }
        }

        private bool IsOnLine(float x, float y)
        {
            if (direction.x == 0)
            {
                return Mathf.Abs(x - origin.x) < Tolerance;
            }

            if (direction.y == 0)
            {
                return Mathf.Abs(y - origin.y) < Tolerance;
            }

            var t1 = (x - origin.x) / direction.x;
            var t2 = (y - origin.y) / direction.y;

            return Mathf.Abs(t1 - t2) < Tolerance;
        }
    }
}