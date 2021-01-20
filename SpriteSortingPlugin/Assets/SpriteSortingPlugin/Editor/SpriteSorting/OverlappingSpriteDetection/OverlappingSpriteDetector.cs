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

using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SAT;
using SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection
{
    public class OverlappingSpriteDetector
    {
        private const float Tolerance = 0.00001f;

        private static SortingComponentShortestDifferenceComparer sortingComponentShortestDifferenceComparer;

        private static Dictionary<SortingComponent, List<SortingComponent>>
            cachedOverlappingSortingComponentDictionaryForSurroundingSpriteAnalysis;

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

            if (filteredSortingComponents.Count < 2)
            {
                return result;
            }

            // Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Count +
            // " to " + filteredSortingComponents.Count);

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
            
            if (filteredSortingComponents.Count < 2)
            {
                return result;
            }
            // Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Count +
            // " to " + filteredSortingComponents.Count);

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
            var startTime = EditorApplication.timeSinceStartup;

            var sortingOptions = new Dictionary<int, int>();
            if (overlappingItems == null || overlappingItems.Count <= 0)
            {
                return sortingOptions;
            }

            InitializeSpriteRendererList(null);
            if (spriteRenderers.Count < 2)
            {
                return sortingOptions;
            }

            this.spriteDetectionData = spriteDetectionData;

            var excludingSpriteRendererList = new List<SpriteRenderer>();
            var baseSortingComponents = new List<SortingComponent>();
            var isSelectedLayersSet = false;

            foreach (var overlappingItem in overlappingItems)
            {
                excludingSpriteRendererList.Add(overlappingItem.SortingComponent.SpriteRenderer);

                var newSortingOrder = overlappingItem.GetNewSortingOrder();
                if (!overlappingItem.HasSortingLayerChanged() && overlappingItem.originSortingOrder == newSortingOrder)
                {
                    continue;
                }

                var sortingComponent = overlappingItem.SortingComponent;
                baseSortingComponents.Add(sortingComponent);
                sortingOptions.Add(sortingComponent.GetInstanceId(), newSortingOrder);

                if (!isSelectedLayersSet)
                {
                    isSelectedLayersSet = true;
                    this.selectedLayers = new List<int>() {SortingLayer.NameToID(overlappingItem.sortingLayerName)};
                }
            }

            if (baseSortingComponents.Count <= 0)
            {
                return sortingOptions;
            }

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidatorCache.UpdateSpriteData(spriteDetectionData.spriteData);
            cachedOverlappingSortingComponentDictionaryForSurroundingSpriteAnalysis =
                new Dictionary<SortingComponent, List<SortingComponent>>();

            // Debug.Log("start recursion");
            AnalyzeSurroundingSpritesRecursive(baseSortingComponents, excludingSpriteRendererList,
                ref sortingOptions);

            spriteDataItemValidatorCache.Clear();

            var timeDif = EditorApplication.timeSinceStartup - startTime;
            Debug.LogFormat("Analyzed surrounding SpriteRenderers in {0} seconds", Math.Round(timeDif, 2));
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

                // Debug.Log("check overlapping sprites against " + baseSortingComponent.SpriteRenderer.name);

                if (!cachedOverlappingSortingComponentDictionaryForSurroundingSpriteAnalysis.TryGetValue(
                    baseSortingComponent, out var foundOverlappingSprites))
                {
                    tempOverlappingSpriteDetector.FilterSortingComponents();
                    tempOverlappingSpriteDetector.DetectOverlappingSortingComponents(baseSortingComponent,
                        out foundOverlappingSprites);

                    cachedOverlappingSortingComponentDictionaryForSurroundingSpriteAnalysis.Add(baseSortingComponent,
                        foundOverlappingSprites);
                }

                var hasOverlappingSortingComponents = foundOverlappingSprites.Count > 0;

                if (!hasOverlappingSortingComponents)
                {
                    // Debug.Log("found no overlapping");
                    continue;
                }

                //exclude and sort overlappings
                var overlappingSprites = ExcludeSortingComponents(foundOverlappingSprites, excludingSpriteRendererList);
                SortOverlappingSortingComponents(ref overlappingSprites, currentBaseSortingOrder);

                // Debug.Log("found overlapping sprites " + overlappingSprites.Count);

                var newExcludingList = new List<SpriteRenderer>(excludingSpriteRendererList);

                foreach (var overlappingSprite in overlappingSprites)
                {
                    // Debug.LogFormat("iteration {0}: check {1} against {2}", counter,
                    // baseSortingComponent.spriteRenderer.name, overlappingSprite.spriteRenderer.name);

                    newExcludingList.Add(overlappingSprite.SpriteRenderer);

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
                    }

                    // first compare current (unchanged or origin) sorting order and then compare their new sorting order values
                    var isBaseSortingOrderHigherThanOverlappingItem = currentBaseSortingOrder > currentSortingOrder;
                    var isUpdatedSortingOrderHigherThanOverlappingItem = newBaseSortingOrder <= newSortingOrder;

                    var isBaseSortingOrderLowerThanOverlappingItem = currentBaseSortingOrder < currentSortingOrder;
                    var isUpdatedSortingOrderLowerThanOverlappingItem = newBaseSortingOrder >= newSortingOrder;


                    if (isBaseSortingOrderHigherThanOverlappingItem && isUpdatedSortingOrderHigherThanOverlappingItem)
                    {
                        newSortingOrder = newBaseSortingOrder - 1;
                    }
                    else if (isBaseSortingOrderLowerThanOverlappingItem &&
                             isUpdatedSortingOrderLowerThanOverlappingItem)
                    {
                        newSortingOrder = newBaseSortingOrder + 1;
                    }
                    else
                    {
                        //sorting order doesnt need to adjust. Therefore no need to add a recursive check
                        continue;
                    }

                    if (!isSortingOptionContained)
                    {
                        sortingOptions.Add(currentSortingComponentInstanceId, currentSortingOrder);
                    }
                    else
                    {
                        sortingOptions[currentSortingComponentInstanceId] = newSortingOrder;
                    }

                    sortingOptions[currentSortingComponentInstanceId] = newSortingOrder;
                    // Debug.Log("go into recursion");

                    tempOverlappingSpriteDetector.AnalyzeSurroundingSpritesRecursive(overlappingSprites,
                        newExcludingList, ref sortingOptions);
                }
            }
        }

        private static List<SortingComponent> ExcludeSortingComponents(List<SortingComponent> overlappingSprites,
            List<SpriteRenderer> excludeList)
        {
            var returnList = new List<SortingComponent>();

            foreach (var overlappingSprite in overlappingSprites)
            {
                var isContaining = false;
                foreach (var spriteRenderer in excludeList)
                {
                    if (overlappingSprite.SpriteRenderer != spriteRenderer)
                    {
                        continue;
                    }

                    isContaining = true;
                    break;
                }

                if (!isContaining)
                {
                    returnList.Add(overlappingSprite);
                }
            }

            return returnList;
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

            if (!spriteRenderer.gameObject.activeInHierarchy || !spriteRenderer.enabled ||
                spriteRenderer.sprite == null)
            {
                return false;
            }

            var sortingGroupArray = spriteRenderer.GetComponentsInParent<SortingGroup>();
            var outermostSortingGroup = SortingGroupUtility.GetOutmostActiveSortingGroup(sortingGroupArray);

            var layerId = outermostSortingGroup == null
                ? spriteRenderer.sortingLayerID
                : outermostSortingGroup.sortingLayerID;

            if (!selectedLayers.Contains(layerId))
            {
                return false;
            }

            sortingComponent = new SortingComponent(spriteRenderer, outermostSortingGroup);
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

            // Debug.Log("start search in " + filteredSortingComponents.Count + " sprite renderers for an overlap with " +
            // sortingComponentToCheck.SpriteRenderer.name);
            if (polygonColliderToCheck == null)
            {
                polygonColliderCacher = PolygonColliderCacher.GetInstance();
            }

            ResetOverlappingDetectionVariables();

            SetSpriteOutline(sortingComponentToCheck.SpriteRenderer);

            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (sortingComponentToCheck.Equals(sortingComponent))
                {
                    continue;
                }

                if (sortingComponentToCheck.SortingGroup != null &&
                    sortingComponent.SortingGroup != null &&
                    sortingComponentToCheck.SortingGroup == sortingComponent.SortingGroup)
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
                    sortingComponent.SpriteRenderer.transform.position.z - boundsToCheck.center.z) > Tolerance)
                {
                    //TODO: is z the distance to the camera? if not maybe create something to choose for the user
                    continue;
                }

                var isOverlapping = IsOverlapping(sortingComponent.SpriteRenderer);
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

            // Debug.Log("found " + (overlappingComponents.Count + 1) + " overlapping sprites");
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

            if (spriteDataItemValidator == null)
            {
                currentValidOutlinePrecision = OutlinePrecision.AxisAlignedBoundingBox;
                return;
            }

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

            if (otherValidator == null)
            {
                return true;
            }

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