﻿using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalyzer
{
    public class SpriteOutlineAnalyzer
    {
        private const float Tolerance = 0.0001f;
        private const int MinPointsOfOutline = 16;

        private Color[] pixels;
        private int spriteHeight;
        private int spriteWidth;
        private float spritePixelsPerUnit;
        private Vector2 pixelOffset;

        private Vector2 lineOrigin;
        private Vector2 lineDirection;

        private HashSet<int> analyzedPoints;

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

        public Vector2[] Analyze(Sprite sprite)
        {
            var outlineList = AnalyzeSpriteOutlines(sprite);

            var mainOutline = outlineList[0];
            FlattenOutlineList(ref mainOutline);
            ValidateClosedOutlineList(ref mainOutline);

            return mainOutline.ToArray();
        }

        public Vector2[] Analyze(Sprite sprite, out List<List<Vector2>> smallerAreaLists)
        {
            smallerAreaLists = AnalyzeSpriteOutlines(sprite, true);

            for (var i = 0; i < smallerAreaLists.Count; i++)
            {
                var outline = smallerAreaLists[i];
                FlattenOutlineList(ref outline);
                ValidateClosedOutlineList(ref outline);
            }

            var mainOutline = smallerAreaLists[0];
            smallerAreaLists.RemoveAt(0);

            return mainOutline.ToArray();
        }

        private List<List<Vector2>> AnalyzeSpriteOutlines(Sprite sprite, bool isCollectingSmallerAreaLists = false)
        {
            var returnOutlineList = new List<List<Vector2>>();
            var largestOutlineList = new List<Vector2>();
            if (analyzedPoints == null)
            {
                analyzedPoints = new HashSet<int>();
            }
            else
            {
                analyzedPoints.Clear();
            }

            spritePixelsPerUnit = sprite.pixelsPerUnit;
            var spriteTexture = sprite.texture;

            pixels = spriteTexture.GetPixels();
            spriteHeight = spriteTexture.height;
            spriteWidth = spriteTexture.width;
            PixelDirectionUtility.spriteWidth = spriteWidth;

            var spriteRectCenter = sprite.rect.center;
            var halfPixelOffset = 1f / spritePixelsPerUnit / 2f;
            pixelOffset = new Vector2(spriteRectCenter.x / spritePixelsPerUnit - halfPixelOffset,
                spriteRectCenter.y / spritePixelsPerUnit - halfPixelOffset);

            var isInsideAlreadyAnalyzedOutline = false;

            for (var i = 0; i < pixels.Length; i++)
            {
                if (analyzedPoints.Contains(i))
                {
                    isInsideAlreadyAnalyzedOutline = true;
                    continue;
                }

                var alphaValue = pixels[i].a;

                if (isInsideAlreadyAnalyzedOutline && alphaValue <= 0)
                {
                    isInsideAlreadyAnalyzedOutline = false;
                }
                else if (!isInsideAlreadyAnalyzedOutline && alphaValue > 0)
                {
                    isInsideAlreadyAnalyzedOutline = true;

                    var analyzedOutline = AnalyseSpriteOutline(i);

                    if (analyzedOutline.Count <= MinPointsOfOutline)
                    {
                        continue;
                    }

                    if (analyzedOutline.Count > largestOutlineList.Count)
                    {
                        largestOutlineList = analyzedOutline;
                    }

                    if (isCollectingSmallerAreaLists)
                    {
                        returnOutlineList.Add(analyzedOutline);
                    }
                }
            }

            if (isCollectingSmallerAreaLists)
            {
                returnOutlineList.Remove(largestOutlineList);
                returnOutlineList.Insert(0, largestOutlineList);
            }
            else
            {
                returnOutlineList.Add(largestOutlineList);
            }

            pixels = null;
            analyzedPoints.Clear();

            return returnOutlineList;
        }

        private List<Vector2> AnalyseSpriteOutline(int startPixelIndex)
        {
            var pointList = new List<Vector2>();

            var startPoint = ConvertToColliderPoint(startPixelIndex);
            pointList.Add(startPoint);
            analyzedPoints.Add(startPixelIndex);

            var startPixelEntryDirection = PixelDirection.East;
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
                    analyzedPoints.Add(neighbourOfBoundaryPointIndex);

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

        private void FlattenOutlineList(ref List<Vector2> pointList)
        {
            if (pointList.Count < 3)
            {
                return;
            }

            var currentIndex = 0;
            lineOrigin = pointList[currentIndex++];
            lineDirection = pointList[currentIndex++] - lineOrigin;

            while (currentIndex < pointList.Count - 1)
            {
                var nextPoint = pointList[currentIndex];
                if (IsOnLine(nextPoint.x, nextPoint.y))
                {
                    pointList.RemoveAt(currentIndex - 1);
                    continue;
                }

                lineOrigin = pointList[currentIndex - 1];
                lineDirection = nextPoint - lineOrigin;

                currentIndex++;
            }
        }

        private bool IsOnLine(float x, float y)
        {
            if (lineDirection.x == 0)
            {
                return Mathf.Abs(x - lineOrigin.x) < Tolerance;
            }

            if (lineDirection.y == 0)
            {
                return Mathf.Abs(y - lineOrigin.y) < Tolerance;
            }

            var t1 = (x - lineOrigin.x) / lineDirection.x;
            var t2 = (y - lineOrigin.y) / lineDirection.y;

            return Mathf.Abs(t1 - t2) < Tolerance;
        }

        private void ValidateClosedOutlineList(ref List<Vector2> pointList)
        {
            if (pointList.Count <= 1)
            {
                return;
            }

            var first = pointList[0];
            if (!first.Equals(pointList[pointList.Count - 1]))
            {
                pointList.Add(first);
            }
        }
    }
}