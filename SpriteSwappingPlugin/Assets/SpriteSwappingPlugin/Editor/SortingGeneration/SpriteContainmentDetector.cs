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
using SpriteSwappingPlugin.OOBB;
using SpriteSwappingPlugin.OverlappingSpriteDetection;
using SpriteSwappingPlugin.SpriteSwappingDetector;
using UnityEditor;
using UnityEngine;

namespace SpriteSwappingPlugin.SortingGeneration
{
    public class SpriteContainmentDetector
    {
        private SpriteDetectionData spriteDetectionData;
        private SortingComponent baseItem;
        private string baseItemAssetGuid;
        private bool hasBaseItemSpriteDataItem;
        private SpriteDataItem baseItemSpriteDataItem;
        private PolygonCollider2D basePolygonCollider;
        private ObjectOrientedBoundingBox baseOOBB;
        private PolygonColliderCacher polygonColliderCacher;

        public SortingComponent DetectContainedBySortingComponent(SortingComponent baseItem,
            List<SortingComponent> sortingComponentsToCheck, SpriteDetectionData spriteDetectionData)
        {
            if (baseItem == null || sortingComponentsToCheck == null)
            {
                return null;
            }

            polygonColliderCacher = PolygonColliderCacher.GetInstance();

            this.baseItem = baseItem;
            this.spriteDetectionData = spriteDetectionData;
            Initialize();

            SortingComponent smallestContainedBySortingComponent = null;
            var lastArea = float.PositiveInfinity;

            foreach (var sortingComponent in sortingComponentsToCheck)
            {
                if (sortingComponent.Equals(baseItem))
                {
                    continue;
                }

                if (ContainsBaseItem(sortingComponent, out var currentArea) && currentArea < lastArea)
                {
                    smallestContainedBySortingComponent = sortingComponent;
                    lastArea = currentArea;
                }
            }

            if (basePolygonCollider != null)
            {
                polygonColliderCacher.DisableCachedCollider(baseItemAssetGuid,
                    basePolygonCollider.GetInstanceID());
            }

            return smallestContainedBySortingComponent;
        }

        private void Initialize()
        {
            baseItemAssetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(baseItem.SpriteRenderer.sprite.GetInstanceID()));

            hasBaseItemSpriteDataItem = spriteDetectionData.spriteData != null &&
                                        spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(
                                            baseItemAssetGuid, out baseItemSpriteDataItem);

            if (!hasBaseItemSpriteDataItem)
            {
                return;
            }

            switch (spriteDetectionData.outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (!baseItemSpriteDataItem.IsValidOOBB())
                    {
                        return;
                    }

                    baseOOBB = baseItemSpriteDataItem.objectOrientedBoundingBox;
                    baseOOBB.UpdateBox(baseItem.SpriteRenderer.transform);
                    break;
                case OutlinePrecision.PixelPerfect:
                    if (!baseItemSpriteDataItem.IsValidOutline())
                    {
                        return;
                    }

                    basePolygonCollider = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        baseItemAssetGuid, baseItemSpriteDataItem, baseItem.SpriteRenderer.transform);
                    break;
            }
        }

        private bool ContainsBaseItem(SortingComponent sortingComponent, out float area)
        {
            var baseItemBounds = CreatePlaneBounds(baseItem.SpriteRenderer.bounds);
            var sortingComponentsBounds = CreatePlaneBounds(sortingComponent.SpriteRenderer.bounds);

            area = sortingComponentsBounds.size.x * sortingComponentsBounds.size.y;
            if (!sortingComponentsBounds.Intersects(baseItemBounds))
            {
                return false;
            }

            if (spriteDetectionData.spriteData == null || !hasBaseItemSpriteDataItem)
            {
                return Contains(sortingComponentsBounds, baseItemBounds);
            }

            var sortingComponentAssetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(sortingComponent.SpriteRenderer.sprite.GetInstanceID()));

            var hasSortingComponentSpriteDataItem =
                spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(sortingComponentAssetGuid,
                    out var sortingComponentSpriteDataItem);

            if (!hasSortingComponentSpriteDataItem)
            {
                return Contains(sortingComponentsBounds, baseItemBounds);
            }


            switch (spriteDetectionData.outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (!sortingComponentSpriteDataItem.IsValidOOBB())
                    {
                        return Contains(sortingComponentsBounds, baseItemBounds);
                    }

                    var otherOOBB = sortingComponentSpriteDataItem.objectOrientedBoundingBox;
                    if (baseOOBB == otherOOBB)
                    {
                        otherOOBB = (ObjectOrientedBoundingBox) otherOOBB.Clone();
                    }

                    otherOOBB.UpdateBox(sortingComponent.SpriteRenderer.transform);

                    var isContained = otherOOBB.Contains(baseOOBB);
                    if (isContained)
                    {
                        area = otherOOBB.GetArea();
                    }

                    return isContained;

                case OutlinePrecision.PixelPerfect:
                    if (!sortingComponentSpriteDataItem.IsValidOutline())
                    {
                        return Contains(baseItemBounds, sortingComponentsBounds);
                    }

                    var otherPolygonCollider = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        sortingComponentAssetGuid, sortingComponentSpriteDataItem,
                        sortingComponent.SpriteRenderer.transform);

                    var distance = otherPolygonCollider.Distance(basePolygonCollider);

                    isContained = false;
                    if (distance.isOverlapped)
                    {
                        isContained = true;
                        foreach (var point in basePolygonCollider.points)
                        {
                            var transformedPoint = basePolygonCollider.transform.TransformPoint(point);
                            if (otherPolygonCollider.OverlapPoint(transformedPoint))
                            {
                                continue;
                            }

                            isContained = false;
                            break;
                        }
                    }

                    if (isContained)
                    {
                        area =
                            sortingComponentSpriteDataItem.CalculatePolygonArea(sortingComponent.SpriteRenderer
                                .transform);
                    }

                    polygonColliderCacher.DisableCachedCollider(sortingComponentAssetGuid,
                        otherPolygonCollider.GetInstanceID());

                    return isContained;
            }

            return Contains(sortingComponentsBounds, baseItemBounds);
        }

        private Bounds CreatePlaneBounds(Bounds originBounds)
        {
            return new Bounds(originBounds.center, new Vector3(originBounds.size.x, originBounds.size.y, 0));
        }

        private static bool Contains(Bounds bounds, Bounds otherBounds)
        {
            return bounds.Contains(otherBounds.min) &&
                   bounds.Contains(otherBounds.max);
        }
    }
}