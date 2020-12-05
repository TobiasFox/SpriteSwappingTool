using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SAT;
using SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection
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

        private SpriteDataItemValidator spriteDataItemValidator;
        private OutlinePrecision currentValidOutlinePrecision;
        private ObjectOrientedBoundingBox oobbToCheck;
        private PolygonCollider2D polygonColliderToCheck;
        private Bounds boundsToCheck;
        private Bounds planeBoundsToCheck;
        private PolygonColliderCacher polygonColliderCacher;

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

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(spriteDetectionData.spriteData);

            //TODO: optimize foreach
            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (DetectOverlappingSortingComponents(sortingComponent, out result.overlappingSortingComponents))
                {
                    // result.overlappingSortingComponents.Insert(0, result.baseItem);
                    result.baseItem = sortingComponent;
                    break;
                }
            }

            DetectAllOverlappingSortingComponents(result.overlappingSortingComponents);

            spriteDataItemValidatorCache.Clear();
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

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(spriteDetectionData.spriteData);

            if (!DetectOverlappingSortingComponents(sortingComponentToCheck, out result.overlappingSortingComponents))
            {
                return result;
            }

            result.baseItem = sortingComponentToCheck;
            // result.overlappingSortingComponents.Insert(0, result.baseItem);

            DetectAllOverlappingSortingComponents(result.overlappingSortingComponents);

            spriteDataItemValidatorCache.Clear();
            return result;
        }

        private void DetectAllOverlappingSortingComponents(List<SortingComponent> allSortingComponents)
        {
            for (var i = 0; i < allSortingComponents.Count; i++)
            {
                var sortingComponent = allSortingComponents[i];
                filteredSortingComponents.Clear();

                for (var j = i + 1; j < allSortingComponents.Count; j++)
                {
                    var sortingComponentToCheck = allSortingComponents[j];

                    if (sortingComponentToCheck.IsOverlapping(sortingComponent))
                    {
                        continue;
                    }

                    filteredSortingComponents.Add(sortingComponentToCheck);
                }

                DetectOverlappingSortingComponents(sortingComponent, out var overlappingSortingComponents);
            }
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
            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(spriteDetectionData.spriteData);

            DetectOverlappingSortingComponents(baseItem, out var resultList);

            spriteDataItemValidatorCache.Clear();
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
                excludingSpriteRendererList.Add(overlappingItem.sortingComponent.spriteRenderer);

                var newSortingOrder = overlappingItem.GetNewSortingOrder();
                if (overlappingItem.originSortingOrder == newSortingOrder)
                {
                    continue;
                }

                var sortingComponent = overlappingItem.sortingComponent;
                baseSortingComponents.Add(sortingComponent);
                sortingOptions.Add(sortingComponent.GetInstanceId(), newSortingOrder);
            }

            this.selectedLayers = new List<int> {baseSortingComponents[0].OriginSortingLayer};

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(spriteDetectionData.spriteData);

            // Debug.Log("start recursion");
            AnalyzeSurroundingSpritesRecursive(baseSortingComponents, excludingSpriteRendererList,
                ref sortingOptions);

            spriteDataItemValidatorCache.Clear();
            return sortingOptions;
        }

        private static void SortOverlappingSortingComponents(ref List<SortingComponent> overlappingSortingComponents,
            int currentBaseSortingOrder)
        {
            var sortingComponentsCount = overlappingSortingComponents.Count;
            for (var i = sortingComponentsCount - 1; i >= 0; i--)
            {
                if (overlappingSortingComponents[i].OriginSortingOrder == currentBaseSortingOrder)
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
            // Debug.Log("basSorting Count " + baseSortingComponents.Count);
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

                var currentBaseSortingOrder = baseSortingComponent.OriginSortingOrder;
                if (!isBaseSortingOptionContained)
                {
                    newBaseSortingOrder = currentBaseSortingOrder;
                    sortingOptions.Add(baseSortingComponentInstanceId, currentBaseSortingOrder);
                }

                Debug.Log("check overlapping sprites against " + baseSortingComponent.spriteRenderer.name);

                var hasOverlappingSortingComponents = tempOverlappingSpriteDetector.DetectOverlappingSortingComponents(
                    baseSortingComponent, out List<SortingComponent> overlappingSprites);

                if (!hasOverlappingSortingComponents)
                {
                    // Debug.Log("found no overlapping");
                    continue;
                }

                // Debug.Log("found overlapping sprites " + overlappingSprites.Count);

                SortOverlappingSortingComponents(ref overlappingSprites, currentBaseSortingOrder);

                var newExcludingList = new List<SpriteRenderer>(excludingSpriteRendererList);

                var counter = 0;
                foreach (var overlappingSprite in overlappingSprites)
                {
                    // Debug.LogFormat("iteration {0}: check {1} against {2}", counter,
                    // baseSortingComponent.spriteRenderer.name, overlappingSprite.spriteRenderer.name);
                    counter++;

                    newExcludingList.Add(overlappingSprite.spriteRenderer);

                    var currentSortingComponentInstanceId = overlappingSprite.GetInstanceId();
                    var currentSortingOrder = overlappingSprite.OriginSortingOrder;

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
                    // Debug.Log("go into recursion");

                    tempOverlappingSpriteDetector.AnalyzeSurroundingSpritesRecursive(overlappingSprites,
                        newExcludingList, ref sortingOptions);
                }
            }
        }

        private void InitializeSpriteRendererList(List<Transform> gameObjectsParents)
        {
            if (gameObjectsParents == null || gameObjectsParents.Count <= 0)
            {
                spriteRenderers = new List<SpriteRenderer>(Object.FindObjectsOfType<SpriteRenderer>());
                return;
            }

            var validTransform = ValidateTransformParents(gameObjectsParents);
            if (validTransform.Count <= 0)
            {
                spriteRenderers = new List<SpriteRenderer>(Object.FindObjectsOfType<SpriteRenderer>());
                return;
            }

            spriteRenderers = new List<SpriteRenderer>();
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
            out List<SortingComponent> overlappingComponents)
        {
            overlappingComponents = new List<SortingComponent>();

            if (sortingComponentToCheck == null || filteredSortingComponents == null ||
                filteredSortingComponents.Count <= 0)
            {
                return false;
            }

            Debug.Log("start search in " + filteredSortingComponents.Count + " sprite renderers for an overlap with " +
                      sortingComponentToCheck.spriteRenderer.name);
            if (polygonColliderToCheck == null)
            {
                polygonColliderCacher = PolygonColliderCacher.GetInstance();
            }

            ResetOverlappingDetectionVariables();

            SetSpriteOutline(sortingComponentToCheck.spriteRenderer);

            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (sortingComponentToCheck.Equals(sortingComponent))
                {
                    continue;
                }

                if (sortingComponentToCheck.sortingGroup != null &&
                    sortingComponent.sortingGroup != null &&
                    sortingComponentToCheck.sortingGroup == sortingComponent.sortingGroup)
                {
                    continue;
                }

                if (isCheckingForIdenticalSortingOptions &&
                    (sortingComponentToCheck.OriginSortingLayer != sortingComponent.OriginSortingLayer ||
                     sortingComponentToCheck.OriginSortingOrder != sortingComponent.OriginSortingOrder))
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

                sortingComponent.AddOverlappingSortingComponent(sortingComponentToCheck);
                sortingComponentToCheck.AddOverlappingSortingComponent(sortingComponent);

                overlappingComponents.Add(sortingComponent);
            }

            if (polygonColliderToCheck != null)
            {
                polygonColliderCacher.DisableCachedCollider(spriteDataItemValidator.AssetGuid,
                    polygonColliderToCheck.GetInstanceID());
            }

            if (overlappingComponents.Count <= 0)
            {
                return false;
            }

            Debug.Log("found " + (overlappingComponents.Count + 1) + " overlapping sprites");
            return true;
        }

        private Bounds CreatePlaneBounds(Bounds originBounds)
        {
            return new Bounds(new Vector3(originBounds.center.x, originBounds.center.y, 0),
                new Vector3(originBounds.size.x, originBounds.size.y, 0));
        }

        private void SetSpriteOutline(SpriteRenderer spriteRenderer)
        {
            boundsToCheck = spriteRenderer.bounds;
            planeBoundsToCheck = CreatePlaneBounds(boundsToCheck);

            spriteDataItemValidator = SpriteDataItemValidatorCache.GetInstance().GetOrCreateValidator(spriteRenderer);
            currentValidOutlinePrecision =
                spriteDataItemValidator.GetValidOutlinePrecision(spriteDetectionData.outlinePrecision);

            if (spriteDetectionData.spriteData == null)
            {
                return;
            }

            var assetGuid = spriteDataItemValidator.AssetGuid;
            switch (spriteDataItemValidator.GetValidOutlinePrecision(spriteDetectionData.outlinePrecision))
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    oobbToCheck = spriteDetectionData.spriteData.spriteDataDictionary[assetGuid]
                        .objectOrientedBoundingBox;
                    oobbToCheck.UpdateBox(spriteRenderer.transform);
                    break;
                case OutlinePrecision.PixelPerfect:
                    var spriteDataItem = spriteDetectionData.spriteData.spriteDataDictionary[assetGuid];
                    polygonColliderToCheck = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(assetGuid,
                        spriteDataItem, spriteRenderer.transform);
                    break;
            }
        }

        private void ResetOverlappingDetectionVariables()
        {
            oobbToCheck = null;
            polygonColliderToCheck = null;
        }

        private bool IsOverlapping(SpriteRenderer spriteRenderer)
        {
            var otherPlaneBounds = CreatePlaneBounds(spriteRenderer.bounds);

            if (!planeBoundsToCheck.Intersects(otherPlaneBounds))
            {
                return false;
            }

            var otherValidator = SpriteDataItemValidatorCache.GetInstance().GetOrCreateValidator(spriteRenderer);
            var otherValidOutlinePrecision =
                otherValidator.GetValidOutlinePrecision(spriteDetectionData.outlinePrecision);

            if ((currentValidOutlinePrecision != otherValidOutlinePrecision) ||
                (currentValidOutlinePrecision == OutlinePrecision.AxisAlignedBoundingBox &&
                 otherValidOutlinePrecision == OutlinePrecision.AxisAlignedBoundingBox))
            {
                //axis aligned check or fallback if one of them is not existing
                return true;
            }

            var currentTransform = spriteRenderer.transform;
            switch (currentValidOutlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    var otherOOBB = spriteDetectionData.spriteData.spriteDataDictionary[otherValidator.AssetGuid]
                        .objectOrientedBoundingBox;

                    if (oobbToCheck == otherOOBB)
                    {
                        otherOOBB = (ObjectOrientedBoundingBox) otherOOBB.Clone();
                    }

                    otherOOBB.UpdateBox(currentTransform);

                    var isOverlapping = SATCollisionDetection.IsOverlapping(oobbToCheck, otherOOBB);
                    return isOverlapping;

                case OutlinePrecision.PixelPerfect:

                    var spriteDataItem = spriteDetectionData.spriteData.spriteDataDictionary[otherValidator.AssetGuid];
                    var otherPolygonColliderToCheck = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        otherValidator.AssetGuid, spriteDataItem, spriteRenderer.transform);

                    var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);
                    polygonColliderCacher.DisableCachedCollider(otherValidator.AssetGuid,
                        otherPolygonColliderToCheck.GetInstanceID());

                    return distance.isOverlapped;
            }

            return true;
        }
    }
}