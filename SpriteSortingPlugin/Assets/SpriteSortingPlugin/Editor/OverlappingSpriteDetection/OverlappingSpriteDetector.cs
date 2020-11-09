using System;
using System.Collections.Generic;
using SpriteSortingPlugin.OverlappingSprites;
using SpriteSortingPlugin.SAT;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.OverlappingSpriteDetection
{
    public class OverlappingSpriteDetector
    {
        private const float Tolerance = 0.00001f;

        private static SortingComponentShortestDifferenceComparer sortingComponentShortestDifferenceComparer;

        private SpriteDetectionData spriteDetectionData;
        private List<SpriteRenderer> spriteRenderers;
        private List<SortingComponent> filteredSortingComponents;
        private List<int> selectedLayers;

        private bool isCheckingForIdenticalSortingOptions;
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
            isCheckingForIdenticalSortingOptions = true;
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
                    // result.overlappingSortingComponents.Insert(0, result.baseItem);
                    break;
                }
            }

            return result;
        }

        public OverlappingSpriteDetectionResult DetectOverlappingSprites(SpriteRenderer spriteRenderer,
            SpriteDetectionData spriteDetectionData)
        {
            this.spriteDetectionData = spriteDetectionData;
            isCheckingForIdenticalSortingOptions = true;
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

            // result.overlappingSortingComponents.Insert(0, result.baseItem);

            return result;
        }

        public List<SortingComponent> DetectOverlappingSortingComponents(SortingComponent baseItem,
            List<SortingComponent> sortingComponentsToCheck, SpriteDetectionData spriteDetectionData)
        {
            if (baseItem == null || sortingComponentsToCheck == null)
            {
                return null;
            }

            this.spriteDetectionData = spriteDetectionData;
            isCheckingForIdenticalSortingOptions = false;
            filteredSortingComponents = sortingComponentsToCheck;

            DetectOverlappingSortingComponents(baseItem, out var resultList,
                out var selfBaseItem);

            return resultList;
        }
        
        public Dictionary<int, int> AnalyzeSurroundingSpritesAndGetAdjustedSortingOptions(
            List<OverlappingItem> overlappingItems, SpriteDetectionData spriteDetectionData)
        {
            if (overlappingItems == null || overlappingItems.Count <= 0)
            {
                return null;
            }

            InitializeSpriteRendererList(null);
            if (spriteRenderers.Count < 2)
            {
                return null;
            }

            var sortingOptions = new Dictionary<int, int>();
            this.spriteDetectionData = spriteDetectionData;

            var excludingSpriteRendererList = new List<SpriteRenderer>();
            var baseSortingComponents = new List<SortingComponent>();

            foreach (var overlappingItem in overlappingItems)
            {
                excludingSpriteRendererList.Add(overlappingItem.originSpriteRenderer);

                var newSortingOrder = overlappingItem.GetNewSortingOrder();
                if (overlappingItem.originSortingOrder == newSortingOrder)
                {
                    continue;
                }

                //TODO cache sortingComponents for better performance
                var sortingComponent = new SortingComponent(overlappingItem.originSpriteRenderer,
                    overlappingItem.originSortingGroup);
                baseSortingComponents.Add(sortingComponent);
                sortingOptions.Add(sortingComponent.GetInstanceId(), newSortingOrder);
            }

            this.selectedLayers = new List<int> {baseSortingComponents[0].CurrentSortingLayer};

            Debug.Log("start recursion");
            AnalyzeSurroundingSpritesRecursive(baseSortingComponents, excludingSpriteRendererList,
                ref sortingOptions);

            return sortingOptions;
        }

        private static void SortOverlappingSortingComponents(ref List<SortingComponent> overlappingSortingComponents,
            int currentBaseSortingOrder)
        {
            var sortingComponentsCount = overlappingSortingComponents.Count;
            for (var i = sortingComponentsCount - 1; i >= 0; i--)
            {
                if (overlappingSortingComponents[i].CurrentSortingOrder == currentBaseSortingOrder)
                {
                    overlappingSortingComponents.RemoveAt(i);
                }
            }

            if (sortingComponentShortestDifferenceComparer == null)
            {
                sortingComponentShortestDifferenceComparer = new SortingComponentShortestDifferenceComparer();
            }

            sortingComponentShortestDifferenceComparer.baseSortingOrder = currentBaseSortingOrder;

            overlappingSortingComponents.Sort(sortingComponentShortestDifferenceComparer);
        }

        private void AnalyzeSurroundingSpritesRecursive(List<SortingComponent> baseSortingComponents,
            List<SpriteRenderer> excludingSpriteRendererList, ref Dictionary<int, int> sortingOptions)
        {
            Debug.Log("basSorting Count " + baseSortingComponents.Count);
            if (baseSortingComponents.Count <= 0)
            {
                return;
            }

            var tempOverlappingSpriteDetector = new OverlappingSpriteDetector
            {
                spriteRenderers = this.spriteRenderers,
                selectedLayers = this.selectedLayers,
                spriteDetectionData = this.spriteDetectionData,
                isCheckingForIdenticalSortingOptions = false
            };

            tempOverlappingSpriteDetector.FilterSortingComponents(excludingSpriteRendererList);

            foreach (var baseSortingComponent in baseSortingComponents)
            {
                var baseSortingComponentInstanceId = baseSortingComponent.GetInstanceId();
                var isBaseSortingOptionContained = sortingOptions.TryGetValue(baseSortingComponentInstanceId,
                    out var newBaseSortingOrder);

                var currentBaseSortingOrder = baseSortingComponent.CurrentSortingOrder;
                if (!isBaseSortingOptionContained)
                {
                    newBaseSortingOrder = currentBaseSortingOrder;
                    sortingOptions.Add(baseSortingComponentInstanceId, currentBaseSortingOrder);
                }

                Debug.Log("check overlapping sprites against " + baseSortingComponent.spriteRenderer.name);

                var hasOverlappingSortingComponents = tempOverlappingSpriteDetector.DetectOverlappingSortingComponents(
                    baseSortingComponent, out List<SortingComponent> overlappingSprites, out var baseItem);

                if (!hasOverlappingSortingComponents)
                {
                    Debug.Log("found no overlapping");
                    continue;
                }

                Debug.Log("found overlapping sprites " + overlappingSprites.Count);

                SortOverlappingSortingComponents(ref overlappingSprites, currentBaseSortingOrder);

                var newExcludingList = new List<SpriteRenderer>(excludingSpriteRendererList);

                var counter = 0;
                foreach (var overlappingSprite in overlappingSprites)
                {
                    Debug.LogFormat("iteration {0}: check {1} against {2}", counter,
                        baseSortingComponent.spriteRenderer.name, overlappingSprite.spriteRenderer.name);
                    counter++;

                    newExcludingList.Add(overlappingSprite.spriteRenderer);

                    var currentSortingComponentInstanceId = overlappingSprite.GetInstanceId();
                    var currentSortingOrder = overlappingSprite.CurrentSortingOrder;

                    if (currentBaseSortingOrder == currentSortingOrder)
                    {
                        continue;
                    }

                    var isSortingOptionContained = sortingOptions.TryGetValue(currentSortingComponentInstanceId,
                        out var newSortingOrder);

                    if (!isSortingOptionContained)
                    {
                        newSortingOrder = currentSortingOrder;
                        sortingOptions.Add(currentSortingComponentInstanceId, currentSortingOrder);
                    }

                    // first compare current (unchanged or origin) sorting order and then compare their new sorting order values
                    if (currentBaseSortingOrder > currentSortingOrder && newBaseSortingOrder <= newSortingOrder)
                    {
                        newSortingOrder = newBaseSortingOrder - 1;
                    }
                    else if (currentBaseSortingOrder < currentSortingOrder &&
                             newBaseSortingOrder >= newSortingOrder)
                    {
                        newSortingOrder = newBaseSortingOrder + 1;
                    }

                    sortingOptions[currentSortingComponentInstanceId] = newSortingOrder;
                    Debug.Log("go into recursion");

                    tempOverlappingSpriteDetector.AnalyzeSurroundingSpritesRecursive(overlappingSprites,
                        newExcludingList, ref sortingOptions);
                }
            }
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
            var outmostSortingGroup = SortingGroupUtility.GetOutmostActiveSortingGroup(sortingGroupArray);

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

            ResetOverlappingDetectionVariables();

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

                if (isCheckingForIdenticalSortingOptions &&
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
            }

            if (polygonColliderToCheck != null)
            {
                PolygonColliderCacher.DisableCachedCollider(assetGuid, polygonColliderToCheck.GetInstanceID());
            }

            if (overlappingComponents.Count <= 0)
            {
                return false;
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

        private void ResetOverlappingDetectionVariables()
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
                return true;
            }

            var otherAssetGuid =
                AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

            var hasSortingComponentSpriteDataItem =
                spriteDetectionData.spriteData.spriteDataDictionary.TryGetValue(otherAssetGuid,
                    out var spriteDataItem);

            if (!hasSortingComponentSpriteDataItem)
            {
                return true;
            }

            var currentTransform = spriteRenderer.transform;

            switch (spriteDetectionData.outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    if (!spriteDataItem.IsValidOOBB())
                    {
                        return true;
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
                        return true;
                    }

                    var otherPolygonColliderToCheck = PolygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        otherAssetGuid,
                        spriteDataItem, spriteRenderer.transform);

                    var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);
                    PolygonColliderCacher.DisableCachedCollider(otherAssetGuid,
                        otherPolygonColliderToCheck.GetInstanceID());

                    return distance.isOverlapped;
            }

            return true;
        }
    }
}