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

        public SortingComponent DetectContainingSortingComponent(SortingComponent baseItem,
            List<SortingComponent> sortingComponentsToCheck, SpriteDetectionData spriteDetectionData)
        {
            if (baseItem == null || sortingComponentsToCheck == null)
            {
                return null;
            }

            this.baseItem = baseItem;
            this.spriteDetectionData = spriteDetectionData;

            foreach (var sortingComponent in sortingComponentsToCheck)
            {
                if (IsContained(sortingComponent))
                {
                    return sortingComponent;
                }
            }

            return null;
        }

        private bool IsContained(SortingComponent sortingComponent)
        {
            var baseItemBounds = baseItem.spriteRenderer.bounds;
            var sortingComponentsBounds = sortingComponent.spriteRenderer.bounds;
            if (!baseItemBounds.Intersects(sortingComponentsBounds))
            {
                return false;
            }

            if (!spriteDetectionData.spriteData)
            {
                return Contains(baseItemBounds, sortingComponentsBounds);
            }

            var basteItemAssetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(baseItem.spriteRenderer.sprite.GetInstanceID()));

            var hasBaseItemSpriteDataItem =
                spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(basteItemAssetGuid,
                    out var baseItemSpriteDataItem);

            if (!hasBaseItemSpriteDataItem)
            {
                return Contains(baseItemBounds, sortingComponentsBounds);
            }

            var sortingComponentAssetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(baseItem.spriteRenderer.sprite.GetInstanceID()));

            var hasSortingComponentSpriteDataItem =
                spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(sortingComponentAssetGuid,
                    out var sortingComponentSpriteDataItem);

            if (!hasSortingComponentSpriteDataItem)
            {
                return Contains(baseItemBounds, sortingComponentsBounds);
            }


            switch (spriteDetectionData.outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (!baseItemSpriteDataItem.IsValidOOBB() || !sortingComponentSpriteDataItem.IsValidOOBB())
                    {
                        return Contains(baseItemBounds, sortingComponentsBounds);
                    }

                    var baseOOBB = baseItemSpriteDataItem.objectOrientedBoundingBox;
                    var otherOOBB = sortingComponentSpriteDataItem.objectOrientedBoundingBox;

                    if (baseOOBB == otherOOBB)
                    {
                        otherOOBB = (ObjectOrientedBoundingBox) otherOOBB.Clone();
                    }

                    baseOOBB.UpdateBox(baseItem.spriteRenderer.transform);
                    otherOOBB.UpdateBox(sortingComponent.spriteRenderer.transform);

                    var isContained = baseOOBB.Contains(otherOOBB);
                    return isContained;
                case OutlinePrecision.PixelPerfect:
                    if (!baseItemSpriteDataItem.IsValidOutline() || !sortingComponentSpriteDataItem.IsValidOutline())
                    {
                        return Contains(baseItemBounds, sortingComponentsBounds);
                    }

                    var polygonColliderToCheck = PolygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        basteItemAssetGuid, baseItemSpriteDataItem, baseItem.spriteRenderer.transform);

                    var otherPolygonColliderToCheck = PolygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        sortingComponentAssetGuid, sortingComponentSpriteDataItem,
                        sortingComponent.spriteRenderer.transform);

                    var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);

                    isContained = false;
                    if (distance.isOverlapped)
                    {
                        isContained = true;
                        foreach (var point in otherPolygonColliderToCheck.points)
                        {
                            if (polygonColliderToCheck.OverlapPoint(point))
                            {
                                continue;
                            }

                            isContained = false;
                            break;
                        }
                    }

                    PolygonColliderCacher.DisableCachedCollider(basteItemAssetGuid,
                        polygonColliderToCheck.GetInstanceID());
                    PolygonColliderCacher.DisableCachedCollider(sortingComponentAssetGuid,
                        otherPolygonColliderToCheck.GetInstanceID());

                    return isContained;
            }

            return Contains(baseItemBounds, sortingComponentsBounds);
        }

        private static bool Contains(Bounds baseItemBounds, Bounds sortingComponentsBounds)
        {
            return baseItemBounds.Contains(sortingComponentsBounds.min) &&
                   baseItemBounds.Contains(sortingComponentsBounds.max);
        }
    }
}