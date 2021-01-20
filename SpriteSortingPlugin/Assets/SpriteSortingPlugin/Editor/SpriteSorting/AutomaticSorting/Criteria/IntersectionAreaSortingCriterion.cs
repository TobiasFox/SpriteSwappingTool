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

using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class IntersectionAreaCriterion : SortingCriterion
    {
        private PolygonIntersectionAreaAnalyzer polygonIntersectionAreaAnalyzer;

        private DefaultSortingCriterionData IntersectionAreaCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public IntersectionAreaCriterion(DefaultSortingCriterionData sortingCriterionData) : base(sortingCriterionData)
        {
            sortingCriterionType = IntersectionAreaCriterionData.sortingCriterionType;
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            var spriteRenderer = sortingComponent.SpriteRenderer;
            var otherSpriteRenderer = otherSortingComponent.SpriteRenderer;

            var polygon = GetOutlinePoints(spriteDataItemValidator, spriteRenderer);
            var otherPolygon = GetOutlinePoints(otherSpriteDataItemValidator, otherSpriteRenderer);
            if (polygonIntersectionAreaAnalyzer == null)
            {
                polygonIntersectionAreaAnalyzer = new PolygonIntersectionAreaAnalyzer();
            }

            var area = CalculateArea(spriteRenderer, spriteDataItemValidator);
            var otherArea = CalculateArea(otherSpriteRenderer, otherSpriteDataItemValidator);
            var intersectionArea = polygonIntersectionAreaAnalyzer.CalculateIntersectionArea(polygon, otherPolygon);

            var autoSortingAreaWithoutIntersectionArea = area - intersectionArea;
            var otherAutoSortingAreaWithoutIntersectionArea = otherArea - intersectionArea;
            var isAutoSortingAreaWithoutIntersectionAreaLarger = autoSortingAreaWithoutIntersectionArea >=
                                                                 otherAutoSortingAreaWithoutIntersectionArea;

            if (IntersectionAreaCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingAreaWithoutIntersectionAreaLarger ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingAreaWithoutIntersectionAreaLarger ? 0 : 1]++;
            }
        }

        private Vector2[] GetOutlinePoints(SpriteDataItemValidator validator, SpriteRenderer spriteRenderer)
        {
            Vector2[] outlinePoints;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:

                    outlinePoints = new Vector2[4];
                    var bounds = spriteRenderer.bounds;
                    var min = bounds.min;
                    var max = bounds.max;

                    outlinePoints[0] = new Vector2(min.x, min.y);
                    outlinePoints[1] = new Vector2(max.x, min.y);
                    outlinePoints[2] = new Vector2(max.x, max.y);
                    outlinePoints[3] = new Vector2(min.x, max.y);

                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(spriteRenderer.transform);

                    outlinePoints = oobb.Points;

                    break;
                case OutlinePrecision.PixelPerfect:

                    var colliderPoints = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .outlinePoints;
                    outlinePoints = new Vector2[colliderPoints.Length];

                    for (var i = 0; i < outlinePoints.Length; i++)
                    {
                        var transformedPoint = spriteRenderer.transform.TransformPoint(colliderPoints[i]);
                        outlinePoints[i] = transformedPoint;
                    }

                    break;
                default:
                    outlinePoints = new Vector2[0];
                    break;
            }

            return outlinePoints;
        }

        private float CalculateArea(SpriteRenderer currentSpriteRenderer, SpriteDataItemValidator validator)
        {
            var returnValue = 0f;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    var bounds = currentSpriteRenderer.bounds;
                    returnValue = bounds.size.x * bounds.size.y;
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(currentSpriteRenderer.transform);
                    returnValue = oobb.GetArea();

                    break;
                case OutlinePrecision.PixelPerfect:
                    var spriteDataItem =
                        autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid];

                    returnValue = spriteDataItem.CalculatePolygonArea(currentSpriteRenderer.transform);
                    break;
            }

            return returnValue;
        }
    }
}