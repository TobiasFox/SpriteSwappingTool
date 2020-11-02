using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SAT;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
{
    public class OverlappingSpriteDetector
    {
        private const float Tolerance = 0.00001f;

        private SpriteDetectionData spriteDetectionData;
        private List<SpriteRenderer> spriteRenderers;
        private List<SortingComponent> filteredSortingComponents;
        private List<int> selectedLayers;

        private bool hasSpriteData;
        private bool hasSortingComponentToCheckSpriteDataItem;
        private ObjectOrientedBoundingBox oobbToCheck;
        private PolygonCollider2D polygonColliderToCheck;
        private string assetGuid;
        private Bounds boundsToCheck;

        public OverlappingSpriteDetectionResult DetectOverlappingSprites(List<int> selectedLayers,
            List<Transform> gameObjectsParents, SpriteDetectionData spriteDetectionData)
        {
            this.spriteDetectionData = spriteDetectionData;
            this.selectedLayers = selectedLayers;
            var result = new OverlappingSpriteDetectionResult();

            InitializeSpriteRendererList(gameObjectsParents);
            if (spriteRenderers.Count < 2)
            {
                return result;
            }

            FilterSortingComponents();

            Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Count +
                      " to " + filteredSortingComponents.Count);

            //TODO: optimize foreach
            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (DetectOverlappingSortingComponents(sortingComponent, out result.overlappingSortingComponents,
                    out result.baseItem))
                {
                    result.overlappingSortingComponents.Insert(0, result.baseItem);
                    break;
                }
            }

            return result;
        }

        public OverlappingSpriteDetectionResult DetectOverlappingSprites(SpriteRenderer spriteRenderer,
            SpriteDetectionData spriteDetectionData)
        {
            this.spriteDetectionData = spriteDetectionData;

            var result = new OverlappingSpriteDetectionResult();
            selectedLayers = new List<int> {spriteRenderer.sortingLayerID};

            var sortingComponentToCheckIsValid =
                ValidateSortingComponent(spriteRenderer, out var sortingComponentToCheck);
            if (!sortingComponentToCheckIsValid)
            {
                return result;
            }

            InitializeSpriteRendererList(null);
            if (spriteRenderers.Count < 2)
            {
                return result;
            }

            FilterSortingComponents();
            Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Count +
                      " to " + filteredSortingComponents.Count);

            if (!DetectOverlappingSortingComponents(sortingComponentToCheck, out result.overlappingSortingComponents,
                out result.baseItem))
            {
                return result;
            }

            result.overlappingSortingComponents.Insert(0, result.baseItem);

            return result;
        }

        private void FilterSortingComponents(List<SpriteRenderer> excludeRendererList = null)
        {
            filteredSortingComponents = new List<SortingComponent>();
            var isCheckingForExcludingRenderers = excludeRendererList != null;

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (isCheckingForExcludingRenderers && excludeRendererList.Contains(spriteRenderer))
                {
                    continue;
                }

                if (ValidateSortingComponent(spriteRenderer, out var sortingComponent))
                {
                    filteredSortingComponents.Add(sortingComponent);
                }
            }
        }

        private bool ValidateSortingComponent(SpriteRenderer spriteRenderer, out SortingComponent sortingComponent)
        {
            sortingComponent = null;
            if (!spriteRenderer.enabled)
            {
                return false;
            }

            var sortingGroupArray = spriteRenderer.GetComponentsInParent<SortingGroup>();
            var outmostSortingGroup = SpriteSortingUtility.GetOutmostActiveSortingGroup(sortingGroupArray);

            var layerId = outmostSortingGroup == null
                ? spriteRenderer.sortingLayerID
                : outmostSortingGroup.sortingLayerID;

            if (!selectedLayers.Contains(layerId))
            {
                return false;
            }

            sortingComponent = new SortingComponent(spriteRenderer, outmostSortingGroup);
            return true;
        }

        private void InitializeSpriteRendererList(List<Transform> gameObjectsParents)
        {
            if (gameObjectsParents == null || gameObjectsParents.Count == 0)
            {
                spriteRenderers = new List<SpriteRenderer>(Object.FindObjectsOfType<SpriteRenderer>());
                return;
            }

            spriteRenderers = new List<SpriteRenderer>();
            var validTransform = ValidateTransformParents(gameObjectsParents);

            foreach (var parent in validTransform)
            {
                spriteRenderers.AddRange(parent.GetComponentsInChildren<SpriteRenderer>());
            }
        }

        private List<Transform> ValidateTransformParents(List<Transform> gameObjectsParents)
        {
            var validTransforms = new List<Transform>();

            foreach (var gameObjectParent in gameObjectsParents)
            {
                if (gameObjectParent == null || !gameObjectParent.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var isChildOfAnyValidTransform = false;
                foreach (var validTransform in validTransforms)
                {
                    if (!gameObjectParent.IsChildOf(validTransform))
                    {
                        continue;
                    }

                    isChildOfAnyValidTransform = true;
                    break;
                }

                if (isChildOfAnyValidTransform)
                {
                    continue;
                }

                var indicesToRemove = new List<int>();
                for (var i = 0; i < validTransforms.Count; i++)
                {
                    var validTransform = validTransforms[i];
                    if (!validTransform.IsChildOf(gameObjectParent))
                    {
                        continue;
                    }

                    indicesToRemove.Add(i);
                }

                foreach (var indexToRemove in indicesToRemove)
                {
                    validTransforms.RemoveAt(indexToRemove);
                }

                validTransforms.Add(gameObjectParent);
            }

            return validTransforms;
        }

        private bool DetectOverlappingSortingComponents(SortingComponent sortingComponentToCheck,
            out List<SortingComponent> overlappingComponents, out SortingComponent baseItem)
        {
            overlappingComponents = new List<SortingComponent>();
            baseItem = null;
            Debug.Log("start search in " + filteredSortingComponents.Count + " sprite renderers for an overlap with " +
                      sortingComponentToCheck.spriteRenderer.name);

            ResetOverlappingDetectionState();

            SetSpriteOutline(sortingComponentToCheck.spriteRenderer);

            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (sortingComponentToCheck.Equals(sortingComponent))
                {
                    continue;
                }

                if (sortingComponentToCheck.outmostSortingGroup != null &&
                    sortingComponent.outmostSortingGroup != null &&
                    sortingComponentToCheck.outmostSortingGroup == sortingComponent.outmostSortingGroup)
                {
                    continue;
                }

                if (spriteDetectionData.isCheckingForIdenticalSortingOptions &&
                    (sortingComponentToCheck.CurrentSortingLayer != sortingComponent.CurrentSortingLayer ||
                     sortingComponentToCheck.CurrentSortingOrder != sortingComponent.CurrentSortingOrder))
                {
                    continue;
                }

                if (spriteDetectionData.cameraProjectionType == CameraProjectionType.Orthographic && Math.Abs(
                    sortingComponent.spriteRenderer.transform.position.z - boundsToCheck.center.z) > Tolerance)
                {
                    //TODO: is z the distance to the camera? if not maybe create something to choose for the user
                    continue;
                }

                var isOverlapping = IsOverlapping(sortingComponent.spriteRenderer);
                if (!isOverlapping)
                {
                    continue;
                }

                overlappingComponents.Add(sortingComponent);

                if (polygonColliderToCheck != null)
                {
                    PolygonColliderCacher.DisableCachedCollider(assetGuid, polygonColliderToCheck.GetInstanceID());
                }

                if (overlappingComponents.Count <= 0)
                {
                    return false;
                }
            }

            baseItem = sortingComponentToCheck;
            Debug.Log("found " + (overlappingComponents.Count + 1) + " overlapping sprites");
            return true;
        }

        private void SetSpriteOutline(SpriteRenderer spriteRenderer)
        {
            boundsToCheck = spriteRenderer.bounds;

            if (!hasSpriteData)
            {
                return;
            }

            assetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

            hasSortingComponentToCheckSpriteDataItem =
                spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(assetGuid, out var spriteDataItem);

            if (!hasSortingComponentToCheckSpriteDataItem)
            {
                return;
            }

            switch (spriteDetectionData.outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (!spriteDataItem.IsValidOOBB())
                    {
                        return;
                    }

                    oobbToCheck = spriteDataItem.objectOrientedBoundingBox;
                    oobbToCheck.UpdateBox(spriteRenderer.transform);

                    break;
                case OutlinePrecision.PixelPerfect:
                    if (!spriteDataItem.IsValidOutline())
                    {
                        return;
                    }

                    polygonColliderToCheck = PolygonColliderCacher.GetCachedColliderOrCreateNewCollider(assetGuid,
                        spriteDataItem,
                        spriteRenderer.transform);

                    break;
            }
        }

        private void ResetOverlappingDetectionState()
        {
            hasSpriteData = spriteDetectionData.spriteData != null;
            hasSortingComponentToCheckSpriteDataItem = false;
            oobbToCheck = null;
            polygonColliderToCheck = null;
            assetGuid = null;
        }

        private bool IsOverlapping(SpriteRenderer spriteRenderer)
        {
            if (!spriteRenderer.bounds.Intersects(boundsToCheck))
            {
                return false;
            }

            if (!hasSpriteData || !hasSortingComponentToCheckSpriteDataItem)
            {
                return false;
            }

            var otherAssetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

            var hasSortingComponentSpriteDataItem =
                spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(otherAssetGuid,
                    out var spriteDataItem);

            if (!hasSortingComponentSpriteDataItem)
            {
                return false;
            }

            var currentTransform = spriteRenderer.transform;

            switch (spriteDetectionData.outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (!spriteDataItem.IsValidOOBB())
                    {
                        return false;
                    }

                    var otherOOBB = spriteDataItem.objectOrientedBoundingBox;

                    if (oobbToCheck == otherOOBB)
                    {
                        otherOOBB = (ObjectOrientedBoundingBox) otherOOBB.Clone();
                    }

                    otherOOBB.UpdateBox(currentTransform);

                    var isOverlapping = SATCollisionDetection.IsOverlapping(oobbToCheck, otherOOBB);
                    return isOverlapping;

                case OutlinePrecision.PixelPerfect:
                    if (!spriteDataItem.IsValidOutline())
                    {
                        return false;
                    }

                    var otherPolygonColliderToCheck = PolygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        otherAssetGuid,
                        spriteDataItem, spriteRenderer.transform);

                    var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);
                    PolygonColliderCacher.DisableCachedCollider(otherAssetGuid,
                        otherPolygonColliderToCheck.GetInstanceID());

                    return distance.isOverlapped;
            }

            return false;
        }
    }
}