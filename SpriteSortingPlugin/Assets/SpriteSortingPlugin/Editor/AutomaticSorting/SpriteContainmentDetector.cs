using System.Collections.Generic;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting
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
            var lastSurfaceArea = float.PositiveInfinity;

            foreach (var sortingComponent in sortingComponentsToCheck)
            {
                if (sortingComponent.Equals(baseItem))
                {
                    continue;
                }

                if (ContainsBaseItem(sortingComponent, out var currentSurfaceArea) &&
                    currentSurfaceArea < lastSurfaceArea)
                {
                    smallestContainedBySortingComponent = sortingComponent;
                    lastSurfaceArea = currentSurfaceArea;
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
                    AssetDatabase.GetAssetPath(baseItem.OriginSpriteRenderer.sprite.GetInstanceID()));

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
                    baseOOBB.UpdateBox(baseItem.OriginSpriteRenderer.transform);
                    break;
                case OutlinePrecision.PixelPerfect:
                    if (!baseItemSpriteDataItem.IsValidOutline())
                    {
                        return;
                    }

                    basePolygonCollider = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        baseItemAssetGuid, baseItemSpriteDataItem, baseItem.OriginSpriteRenderer.transform);
                    break;
            }
        }

        private bool ContainsBaseItem(SortingComponent sortingComponent, out float surfaceArea)
        {
            var baseItemBounds = baseItem.OriginSpriteRenderer.bounds;
            var sortingComponentsBounds = sortingComponent.OriginSpriteRenderer.bounds;

            surfaceArea = sortingComponentsBounds.size.x * sortingComponentsBounds.size.y;
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
                    AssetDatabase.GetAssetPath(sortingComponent.OriginSpriteRenderer.sprite.GetInstanceID()));

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

                    otherOOBB.UpdateBox(sortingComponent.OriginSpriteRenderer.transform);

                    var isContained = otherOOBB.Contains(baseOOBB);
                    if (isContained)
                    {
                        surfaceArea = otherOOBB.GetSurfaceArea();
                    }

                    return isContained;

                case OutlinePrecision.PixelPerfect:
                    if (!sortingComponentSpriteDataItem.IsValidOutline())
                    {
                        return Contains(baseItemBounds, sortingComponentsBounds);
                    }

                    var otherPolygonCollider = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        sortingComponentAssetGuid, sortingComponentSpriteDataItem,
                        sortingComponent.OriginSpriteRenderer.transform);

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
                        surfaceArea =
                            sortingComponentSpriteDataItem.CalculatePolygonArea(sortingComponent.OriginSpriteRenderer
                                .transform);
                    }

                    polygonColliderCacher.DisableCachedCollider(sortingComponentAssetGuid,
                        otherPolygonCollider.GetInstanceID());

                    return isContained;
            }

            return Contains(sortingComponentsBounds, baseItemBounds);
        }

        private static bool Contains(Bounds bounds, Bounds otherBounds)
        {
            return bounds.Contains(otherBounds.min) &&
                   bounds.Contains(otherBounds.max);
        }
    }
}