using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SAT;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
{
    public static class SpriteSortingUtility
    {
        private const float Tolerance = 0.00001f;

        private static SortingComponentShortestDifferenceComparer sortingComponentShortestDifferenceComparer;

        public static SpriteSortingAnalysisResult AnalyzeSpriteSorting(CameraProjectionType cameraProjectionType,
            List<int> selectedLayers, List<Transform> gameObjectsParents = null, SpriteAlphaData spriteAlphaData = null)
        {
            var result = new SpriteSortingAnalysisResult();

            var spriteRenderers = InitializeSpriteRendererList(gameObjectsParents);
            if (spriteRenderers.Count < 2)
            {
                return result;
            }

            var filteredSortingComponents = FilterSortingComponents(spriteRenderers, selectedLayers);

            Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Count +
                      " to " + filteredSortingComponents.Count);

            //TODO: optimize foreach
            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (CheckOverlappingSprites(cameraProjectionType, filteredSortingComponents, sortingComponent,
                    spriteAlphaData, out List<OverlappingItem> overlappingSprites, out var baseItem))
                {
                    result.overlappingItems = overlappingSprites;
                    result.baseItem = baseItem;
                    break;
                }
            }

            return result;
        }

        public static SpriteSortingAnalysisResult AnalyzeSpriteSorting(CameraProjectionType cameraProjectionType,
            SpriteRenderer spriteRenderer, SpriteAlphaData spriteAlphaData = null)
        {
            var result = new SpriteSortingAnalysisResult();
            var selectedLayers = new List<int> {spriteRenderer.sortingLayerID};

            var sortingComponentToCheckIsValid =
                ValidateSortingComponent(selectedLayers, spriteRenderer, out var sortingComponentToCheck);
            if (!sortingComponentToCheckIsValid)
            {
                return result;
            }

            var spriteRenderers = InitializeSpriteRendererList(null);
            if (spriteRenderers.Count < 2)
            {
                return result;
            }

            var filteredSortingComponents = FilterSortingComponents(spriteRenderers, selectedLayers);
            Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Count +
                      " to " + filteredSortingComponents.Count);

            if (!CheckOverlappingSprites(cameraProjectionType, filteredSortingComponents, sortingComponentToCheck,
                spriteAlphaData, out List<OverlappingItem> overlappingSprites, out var baseItem))
            {
                return result;
            }

            result.overlappingItems = overlappingSprites;
            result.baseItem = baseItem;

            return result;
        }

        //TODO replace excludeRendererList with dictionary
        private static List<SortingComponent> FilterSortingComponents(List<SpriteRenderer> spriteRenderers,
            List<int> selectedLayers, List<SpriteRenderer> excludeRendererList = null)
        {
            var filteredSortingComponents = new List<SortingComponent>();
            var isCheckingForExcludingRenderers = excludeRendererList != null;

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (isCheckingForExcludingRenderers && excludeRendererList.Contains(spriteRenderer))
                {
                    continue;
                }

                if (ValidateSortingComponent(selectedLayers, spriteRenderer, out var sortingComponent))
                {
                    filteredSortingComponents.Add(sortingComponent);
                }
            }

            return filteredSortingComponents;
        }

        private static bool ValidateSortingComponent(List<int> selectedLayers, SpriteRenderer spriteRenderer,
            out SortingComponent sortingComponent)
        {
            sortingComponent = null;
            if (!spriteRenderer.enabled)
            {
                return false;
            }

            var sortingGroupArray = spriteRenderer.GetComponentsInParent<SortingGroup>();
            var outmostSortingGroup = GetOutmostActiveSortingGroup(sortingGroupArray);

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

        //TODO: consider prefab scene
        private static List<SpriteRenderer> InitializeSpriteRendererList(List<Transform> gameObjectsParents)
        {
            if (gameObjectsParents == null || gameObjectsParents.Count == 0)
            {
                return new List<SpriteRenderer>(Object.FindObjectsOfType<SpriteRenderer>());
            }

            var spriteRenderers = new List<SpriteRenderer>();
            var validTransform = ValidateTransformParents(gameObjectsParents);

            foreach (var parent in validTransform)
            {
                spriteRenderers.AddRange(parent.GetComponentsInChildren<SpriteRenderer>());
            }

            return spriteRenderers;
        }

        private static List<Transform> ValidateTransformParents(List<Transform> gameObjectsParents)
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

        private static bool CheckOverlappingSprites(CameraProjectionType cameraProjectionType,
            IReadOnlyCollection<SortingComponent> filteredSortingComponents, SortingComponent sortingComponentToCheck,
            SpriteAlphaData spriteAlphaData, out List<OverlappingItem> overlappingComponents,
            out OverlappingItem baseItem)
        {
            overlappingComponents = null;
            baseItem = null;

            if (!CheckOverlappingSprites(cameraProjectionType, filteredSortingComponents, sortingComponentToCheck,
                spriteAlphaData, true, out List<SortingComponent> overlappingSortingComponents,
                out SortingComponent baseSortingComponent))
            {
                return false;
            }

            overlappingComponents = new List<OverlappingItem>();
            baseItem = new OverlappingItem(baseSortingComponent, true);
            overlappingComponents.Add(baseItem);

            foreach (var sortingComponent in overlappingSortingComponents)
            {
                overlappingComponents.Add(new OverlappingItem(sortingComponent));
            }

            return true;
        }

        private static bool CheckOverlappingSprites(CameraProjectionType cameraProjectionType,
            IReadOnlyCollection<SortingComponent> filteredSortingComponents, SortingComponent sortingComponentToCheck,
            SpriteAlphaData spriteAlphaData, bool isCheckingForSameSortingOptions,
            out List<SortingComponent> overlappingComponents,
            out SortingComponent baseItem)
        {
            overlappingComponents = new List<SortingComponent>();
            baseItem = null;
            Debug.Log("start search in " + filteredSortingComponents.Count + " sprite renderers for an overlap with " +
                      sortingComponentToCheck.spriteRenderer.name);

            var isUsingOOBB = spriteAlphaData != null;
            var hasSortingComponentToCheckOOBB = false;

            ObjectOrientedBoundingBox oobbToCheck = null;

            if (isUsingOOBB)
            {
                var assetGuid =
                    AssetDatabase.AssetPathToGUID(
                        AssetDatabase.GetAssetPath(sortingComponentToCheck.spriteRenderer.sprite.GetInstanceID()));
                hasSortingComponentToCheckOOBB =
                    spriteAlphaData.objectOrientedBoundingBoxDictionary.TryGetValue(assetGuid, out oobbToCheck);

                if (hasSortingComponentToCheckOOBB)
                {
                    oobbToCheck.UpdateBox(sortingComponentToCheck.spriteRenderer.transform);
                }
            }

            var boundsToCheck = sortingComponentToCheck.spriteRenderer.bounds;

            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (sortingComponentToCheck.Equals(sortingComponent))
                {
                    continue;
                }

                if (!sortingComponent.spriteRenderer.bounds.Intersects(boundsToCheck))
                {
                    continue;
                }

                if (isUsingOOBB && hasSortingComponentToCheckOOBB)
                {
                    var assetGuid =
                        AssetDatabase.AssetPathToGUID(
                            AssetDatabase.GetAssetPath(sortingComponent.spriteRenderer.sprite.GetInstanceID()));

                    var hasSortingComponentOOBB =
                        spriteAlphaData.objectOrientedBoundingBoxDictionary.TryGetValue(assetGuid,
                            out var sortingComponentOOBB);

                    if (hasSortingComponentOOBB)
                    {
                        if (oobbToCheck == sortingComponentOOBB)
                        {
                            sortingComponentOOBB = (ObjectOrientedBoundingBox) oobbToCheck.Clone();
                        }

                        sortingComponentOOBB.UpdateBox(sortingComponent.spriteRenderer.transform);

                        var isOverlapping = SATCollisionDetection.IsOverlapping(oobbToCheck, sortingComponentOOBB);

                        if (!isOverlapping)
                        {
                            continue;
                        }
                    }
                }

                if (sortingComponentToCheck.outmostSortingGroup != null &&
                    sortingComponent.outmostSortingGroup != null &&
                    sortingComponentToCheck.outmostSortingGroup == sortingComponent.outmostSortingGroup)
                {
                    continue;
                }

                if (cameraProjectionType == CameraProjectionType.Orthogonal && Math.Abs(
                    sortingComponent.spriteRenderer.transform.position.z - boundsToCheck.center.z) > Tolerance)
                {
                    //TODO: is z the distance to the camera? if not maybe create something to choose for the user
                    continue;
                }

                if (isCheckingForSameSortingOptions &&
                    (sortingComponentToCheck.CurrentSortingLayer != sortingComponent.CurrentSortingLayer ||
                     sortingComponentToCheck.CurrentSortingOrder != sortingComponent.CurrentSortingOrder))
                {
                    continue;
                }

                overlappingComponents.Add(sortingComponent);
            }

            if (overlappingComponents.Count <= 0)
            {
                return false;
            }

            baseItem = sortingComponentToCheck;
            Debug.Log("found " + (overlappingComponents.Count + 1) + " overlapping sprites");
            return true;
        }

        public static List<SortingGroup> FilterSortingGroups(IEnumerable<SortingGroup> groups)
        {
            var list = new List<SortingGroup>();

            foreach (var sortingGroup in groups)
            {
                if (!sortingGroup.enabled)
                {
                    continue;
                }

                list.Add(sortingGroup);
            }

            return list;
        }

        private static SortingGroup GetOutmostActiveSortingGroup(IReadOnlyList<SortingGroup> groups)
        {
            for (var i = groups.Count - 1; i >= 0; i--)
            {
                var sortingGroup = groups[i];
                if (!sortingGroup.enabled)
                {
                    continue;
                }

                return sortingGroup;
            }

            return null;
        }

        public static Dictionary<int, int> AnalyzeSurroundingSprites(CameraProjectionType cameraProjectionType,
            List<OverlappingItem> overlappingItems, SpriteAlphaData spriteAlphaData)
        {
            if (overlappingItems == null || overlappingItems.Count <= 0)
            {
                return null;
            }

            var spriteRenderers = InitializeSpriteRendererList(null);
            if (spriteRenderers.Count < 2)
            {
                return null;
            }

            var sortingOptions = new Dictionary<int, int>();

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

            AnalyzeSurroundingSpritesRecursive(cameraProjectionType, spriteAlphaData, spriteRenderers,
                baseSortingComponents, excludingSpriteRendererList, ref sortingOptions);

            return sortingOptions;
        }

        private static void AnalyzeSurroundingSpritesRecursive(CameraProjectionType cameraProjectionType,
            SpriteAlphaData spriteAlphaData, List<SpriteRenderer> spriteRenderers,
            List<SortingComponent> baseSortingComponents, List<SpriteRenderer> excludingSpriteRendererList,
            ref Dictionary<int, int> sortingOptions)
        {
            var filteredSortingComponents = FilterSortingComponents(spriteRenderers,
                new List<int> {baseSortingComponents[0].CurrentSortingLayer}, excludingSpriteRendererList);

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

                if (!CheckOverlappingSprites(cameraProjectionType, filteredSortingComponents, baseSortingComponent,
                    spriteAlphaData, false, out List<SortingComponent> overlappingSprites, out var baseItem))
                {
                    continue;
                }

                SortOverlappingSortingComponents(ref overlappingSprites, currentBaseSortingOrder);

                var newExcludingList = new List<SpriteRenderer>(excludingSpriteRendererList);

                foreach (var overlappingSprite in overlappingSprites)
                {
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
                    else if (currentBaseSortingOrder < currentSortingOrder && newBaseSortingOrder >= newSortingOrder)
                    {
                        newSortingOrder = newBaseSortingOrder + 1;
                    }

                    sortingOptions[currentSortingComponentInstanceId] = newSortingOrder;

                    AnalyzeSurroundingSpritesRecursive(cameraProjectionType, spriteAlphaData, spriteRenderers,
                        overlappingSprites, newExcludingList, ref sortingOptions);
                }
            }
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
    }
}