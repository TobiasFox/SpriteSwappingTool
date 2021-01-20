#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalyzer
{
    public class SpriteOutlineAnalyzer
    {
        private const float Tolerance = 0.0001f;
        private const int MinPointsOfOutline = 16;

        private Color[] pixels;
        private int spriteWidth;
        private float spritePixelsPerUnit;
        private Vector2 pixelOffset;

        private Vector2 lineOrigin;
        private Vector2 lineDirection;

        private HashSet<int> analyzedPoints;

        public bool Simplify(Vector2[] points, float outlineTolerance, out Vector2[] simplifiedPoints)
        {
            var simplifiedPointList = new List<Vector2>();
            var pointList = new List<Vector2>(points);

            LineUtility.Simplify(pointList, outlineTolerance, simplifiedPointList);

            //validate min points
            if (simplifiedPointList.Count < 4)
            {
                simplifiedPoints = null;
                return false;
            }

            simplifiedPoints = simplifiedPointList.ToArray();
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

        public Vector2[] Analyze(Sprite sprite, out List<List<Vector2>> additionalOutlinesList)
        {
            additionalOutlinesList = AnalyzeSpriteOutlines(sprite, true);

            for (var i = 0; i < additionalOutlinesList.Count; i++)
            {
                var outline = additionalOutlinesList[i];
                FlattenOutlineList(ref outline);
                ValidateClosedOutlineList(ref outline);
            }

            var mainOutline = additionalOutlinesList[0];
            additionalOutlinesList.RemoveAt(0);

            return mainOutline.ToArray();
        }

        private List<List<Vector2>> AnalyzeSpriteOutlines(Sprite sprite, bool isCollectingAdditionalOutlines = false)
        {
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
            spriteWidth = spriteTexture.width;
            PixelDirectionUtility.spriteWidth = spriteWidth;

            var spriteRectCenter = sprite.rect.center;
            var halfPixelOffset = 1f / spritePixelsPerUnit / 2f;
            pixelOffset = new Vector2(spriteRectCenter.x / spritePixelsPerUnit - halfPixelOffset,
                spriteRectCenter.y / spritePixelsPerUnit - halfPixelOffset);

            var largestOutlineList = AnalyseAllOutlines(out var additionalOutlineList, isCollectingAdditionalOutlines);

            if (isCollectingAdditionalOutlines)
            {
                additionalOutlineList.Remove(largestOutlineList);
                additionalOutlineList.Insert(0, largestOutlineList);
            }
            else
            {
                additionalOutlineList.Add(largestOutlineList);
            }

            pixels = null;
            analyzedPoints.Clear();

            return additionalOutlineList;
        }

        private List<Vector2> AnalyseAllOutlines(out List<List<Vector2>> additionalOutlineList,
            bool isCollectingSmallerAreaLists = false)
        {
            additionalOutlineList = new List<List<Vector2>>();
            var largestOutlineList = new List<Vector2>();
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
                        additionalOutlineList.Add(analyzedOutline);
                    }
                }
            }

            return largestOutlineList;
        }

        private List<Vector2> AnalyseSpriteOutline(int startPixelIndex)
        {
            var pointList = new List<Vector2>();

            var startPoint = ConvertToColliderPoint(startPixelIndex);
            pointList.Add(startPoint);
            analyzedPoints.Add(startPixelIndex);

            var startPixelEntryDirection = PixelDirection.East;
            var pixelDirectionToCheck = PixelDirectionUtility.GetOppositePixelDirection(startPixelEntryDirection);
            var firstEntryDirection = pixelDirectionToCheck;

            var boundaryPixelIndex = startPixelIndex;
            var neighbourCounter = 0;

            var neighbourOfBoundaryPointIndex = GetNextValidPixelNeighbourIndexWithinSprite(startPixelIndex,
                ref neighbourCounter, ref pixelDirectionToCheck);

            while (neighbourOfBoundaryPointIndex != startPixelIndex)
            {
                if (neighbourOfBoundaryPointIndex == startPixelIndex &&
                    firstEntryDirection == startPixelEntryDirection)
                {
                    break;
                }

                if (neighbourCounter > PixelDirectionUtility.PixelDirections || neighbourOfBoundaryPointIndex < 0)
                {
                    break;
                }

                if (pixels[neighbourOfBoundaryPointIndex].a > 0)
                {
                    //found boundary pixel
                    var nextPoint = ConvertToColliderPoint(neighbourOfBoundaryPointIndex);
                    pointList.Add(nextPoint);
                    analyzedPoints.Add(neighbourOfBoundaryPointIndex);

                    boundaryPixelIndex = neighbourOfBoundaryPointIndex;

                    var backtracedDirection = PixelDirectionUtility.GetBacktracedPixelDirectionClockWise(
                        pixelDirectionToCheck, firstEntryDirection);

                    pixelDirectionToCheck = PixelDirectionUtility.GetOppositePixelDirection(backtracedDirection);
                    firstEntryDirection = pixelDirectionToCheck;
                    neighbourCounter = 0;
                }
                else
                {
                    pixelDirectionToCheck = PixelDirectionUtility.GetNextPixelDirectionClockWise(pixelDirectionToCheck);
                }

                neighbourOfBoundaryPointIndex = GetNextValidPixelNeighbourIndexWithinSprite(boundaryPixelIndex,
                    ref neighbourCounter, ref pixelDirectionToCheck);
            }

            return pointList;
        }

        private int GetNextValidPixelNeighbourIndexWithinSprite(int currentBoundaryIndex, ref int neighbourCounter,
            ref PixelDirection pixelDirectionToCheck)
        {
            if (neighbourCounter < 0)
            {
                return -1;
            }

            while (neighbourCounter < PixelDirectionUtility.PixelDirections)
            {
                neighbourCounter++;

                var isOutsideOfLeftImageBorder = currentBoundaryIndex % spriteWidth == 0 &&
                                                 (pixelDirectionToCheck == PixelDirection.Northwest ||
                                                  pixelDirectionToCheck == PixelDirection.West ||
                                                  pixelDirectionToCheck == PixelDirection.Southwest);
                if (isOutsideOfLeftImageBorder)
                {
                    pixelDirectionToCheck = PixelDirectionUtility.GetNextPixelDirectionClockWise(pixelDirectionToCheck);
                    continue;
                }

                var isOutsideOfRightImageBorder = currentBoundaryIndex % spriteWidth == spriteWidth - 1 &&
                                                  (pixelDirectionToCheck == PixelDirection.Northeast ||
                                                   pixelDirectionToCheck == PixelDirection.East ||
                                                   pixelDirectionToCheck == PixelDirection.Southeast);
                if (isOutsideOfRightImageBorder)
                {
                    pixelDirectionToCheck = PixelDirectionUtility.GetNextPixelDirectionClockWise(pixelDirectionToCheck);
                    continue;
                }

                var currentNeighbourIndex =
                    PixelDirectionUtility.GetIndexOfPixelDirection(currentBoundaryIndex, pixelDirectionToCheck);

                var isInsideOfBottomImageBorder = currentNeighbourIndex >= 0;
                var isInsideOfTopImageBorder = currentNeighbourIndex < pixels.Length;

                if (isInsideOfBottomImageBorder && isInsideOfTopImageBorder)
                {
                    return currentNeighbourIndex;
                }

                pixelDirectionToCheck = PixelDirectionUtility.GetNextPixelDirectionClockWise(pixelDirectionToCheck);
            }

            return -1;
        }

        private Vector2 ConvertToColliderPoint(int pixelIndex)
        {
            var pixelWidth = pixelIndex % spriteWidth;
            var pixelHeight = pixelIndex / spriteWidth;

            var point = new Vector2((pixelWidth / spritePixelsPerUnit),
                (pixelHeight / spritePixelsPerUnit)) - pixelOffset;

            return point;
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