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
    public class SizeSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer otherSpriteRenderer;

        private DefaultSortingCriterionData SizeSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public SizeSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(sortingCriterionData)
        {
            sortingCriterionType = SizeSortingCriterionData.sortingCriterionType;
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            spriteRenderer = sortingComponent.SpriteRenderer;
            otherSpriteRenderer = otherSortingComponent.SpriteRenderer;

            AnalyzeArea();
        }

        private void AnalyzeArea()
        {
            var area = CalculateArea(spriteRenderer, spriteDataItemValidator);
            var otherArea = CalculateArea(otherSpriteRenderer, otherSpriteDataItemValidator);
            var isAutoSortingComponentIsLarger = area >= otherArea;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingComponentIsLarger ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentIsLarger ? 0 : 1]++;
            }
        }

        private void DrawBoundingBox(Bounds enclosingBoundingBox)
        {
            var points = new Vector2[4];
            points[0] = new Vector2(enclosingBoundingBox.min.x, enclosingBoundingBox.min.y);
            points[1] = new Vector2(enclosingBoundingBox.min.x, enclosingBoundingBox.max.y);
            points[2] = new Vector2(enclosingBoundingBox.max.x, enclosingBoundingBox.max.y);
            points[3] = new Vector2(enclosingBoundingBox.max.x, enclosingBoundingBox.min.y);

            for (int i = 0; i < points.Length; i++)
            {
                Debug.DrawLine(points[i], points[(i + 1) % points.Length], Color.green, 2);
            }
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