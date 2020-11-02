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

        private static Dictionary<string, PolygonCollider2D[]> spriteColliderDataDictionary =
            new Dictionary<string, PolygonCollider2D[]>();

        public static SpriteSortingAnalysisResult AnalyzeSpriteSorting(CameraProjectionType cameraProjectionType,
            List<int> selectedLayers, List<Transform> gameObjectsParents = null, SpriteData spriteData = null,
            OutlinePrecision outlinePrecision = OutlinePrecision.ObjectOrientedBoundingBox)
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
                    spriteData, outlinePrecision, out List<OverlappingItem> overlappingSprites, out var baseItem))
                {
                    result.overlappingItems = overlappingSprites;
                    result.baseItem = baseItem;
                    break;
                }
            }

            return result;
        }

        public static SpriteSortingAnalysisResult AnalyzeSpriteSorting(CameraProjectionType cameraProjectionType,
            SpriteRenderer spriteRenderer, SpriteData spriteData = null,
            OutlinePrecision outlinePrecision = OutlinePrecision.ObjectOrientedBoundingBox)
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
                spriteData, outlinePrecision, out List<OverlappingItem> overlappingSprites, out var baseItem))
            {
                return result;
            }

            result.overlappingItems = overlappingSprites;
            result.baseItem = baseItem;

            return result;
        }

        //TODO replace excludeRendererList with dictionary
        public static List<SortingComponent> FilterSortingComponents(List<SpriteRenderer> spriteRenderers,
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
        public static List<SpriteRenderer> InitializeSpriteRendererList(List<Transform> gameObjectsParents)
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
            SpriteData spriteData, OutlinePrecision outlinePrecision,
            out List<OverlappingItem> overlappingComponents, out OverlappingItem baseItem)
        {
            overlappingComponents = null;
            baseItem = null;

            if (!CheckOverlappingSprites(cameraProjectionType, filteredSortingComponents, sortingComponentToCheck,
                spriteData, true, outlinePrecision, out List<SortingComponent> overlappingSortingComponents,
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

        //TODO simplify parameters
        private static bool CheckOverlappingSprites(CameraProjectionType cameraProjectionType,
            IReadOnlyCollection<SortingComponent> filteredSortingComponents, SortingComponent sortingComponentToCheck,
            SpriteData spriteData, bool isCheckingForSameSortingOptions, OutlinePrecision outlinePrecision,
            out List<SortingComponent> overlappingComponents,
            out SortingComponent baseItem)
        {
            overlappingComponents = new List<SortingComponent>();
            baseItem = null;
            Debug.Log("start search in " + filteredSortingComponents.Count + " sprite renderers for an overlap with " +
                      sortingComponentToCheck.spriteRenderer.name);

            var hasSpriteAlphaData = spriteData != null;
            var hasSortingComponentToCheckSpriteDataItem = false;

            ObjectOrientedBoundingBox oobbToCheck = null;
            PolygonCollider2D polygonColliderToCheck = null;
            string assetGuid = null;

            if (hasSpriteAlphaData)
            {
                assetGuid =
                    AssetDatabase.AssetPathToGUID(
                        AssetDatabase.GetAssetPath(sortingComponentToCheck.spriteRenderer.sprite.GetInstanceID()));

                hasSortingComponentToCheckSpriteDataItem =
                    spriteData.spriteDataDictionary.TryGetValue(assetGuid, out var spriteDataItem);

                if (hasSortingComponentToCheckSpriteDataItem)
                {
                    switch (outlinePrecision)
                    {
                        case OutlinePrecision.ObjectOrientedBoundingBox:
                            if (spriteDataItem.IsValidOOBB())
                            {
                                oobbToCheck = spriteDataItem.objectOrientedBoundingBox;
                                oobbToCheck.UpdateBox(sortingComponentToCheck.spriteRenderer.transform);
                            }

                            break;
                        case OutlinePrecision.PixelPerfect:
                            if (spriteDataItem.IsValidOutline())
                            {
                                polygonColliderToCheck = GetCachedColliderOrCreateNewCollider(assetGuid, spriteDataItem,
                                    sortingComponentToCheck.spriteRenderer.transform);
                            }

                            break;
                    }
                }
            }

            var boundsToCheck = sortingComponentToCheck.spriteRenderer.bounds;

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

                if (isCheckingForSameSortingOptions &&
                    (sortingComponentToCheck.CurrentSortingLayer != sortingComponent.CurrentSortingLayer ||
                     sortingComponentToCheck.CurrentSortingOrder != sortingComponent.CurrentSortingOrder))
                {
                    continue;
                }

                if (cameraProjectionType == CameraProjectionType.Orthographic && Math.Abs(
                    sortingComponent.spriteRenderer.transform.position.z - boundsToCheck.center.z) > Tolerance)
                {
                    //TODO: is z the distance to the camera? if not maybe create something to choose for the user
                    continue;
                }

                if (!sortingComponent.spriteRenderer.bounds.Intersects(boundsToCheck))
                {
                    continue;
                }

                //TODO simplify if
                if (hasSpriteAlphaData)
                {
                    if (hasSortingComponentToCheckSpriteDataItem)
                    {
                        var otherAssetGuid =
                            AssetDatabase.AssetPathToGUID(
                                AssetDatabase.GetAssetPath(sortingComponent.spriteRenderer.sprite.GetInstanceID()));

                        var hasSortingComponentSpriteDataItem =
                            spriteData.spriteDataDictionary.TryGetValue(otherAssetGuid,
                                out var spriteDataItem);

                        if (hasSortingComponentSpriteDataItem)
                        {
                            var currentTransform = sortingComponent.spriteRenderer.transform;

                            switch (outlinePrecision)
                            {
                                case OutlinePrecision.ObjectOrientedBoundingBox:
                                    if (spriteDataItem.IsValidOOBB())
                                    {
                                        var otherOOBB = spriteDataItem.objectOrientedBoundingBox;

                                        if (oobbToCheck == otherOOBB)
                                        {
                                            otherOOBB = (ObjectOrientedBoundingBox) otherOOBB.Clone();
                                        }

                                        otherOOBB.UpdateBox(currentTransform);

                                        var isOverlapping = SATCollisionDetection.IsOverlapping(oobbToCheck, otherOOBB);

                                        if (!isOverlapping)
                                        {
                                            continue;
                                        }
                                    }

                                    break;
                                case OutlinePrecision.PixelPerfect:
                                    if (spriteDataItem.IsValidOutline())
                                    {
                                        var otherPolygonColliderToCheck = GetCachedColliderOrCreateNewCollider(
                                            otherAssetGuid,
                                            spriteDataItem, sortingComponent.spriteRenderer.transform);

                                        var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);
                                        DisableCachedCollider(otherAssetGuid,
                                            otherPolygonColliderToCheck.GetInstanceID());

                                        // Debug.DrawLine(distance.pointA,
                                        //     new Vector3(distance.pointA.x, distance.pointA.y, -15), Color.blue, 3);
                                        // Debug.DrawLine(distance.pointB,
                                        //     new Vector3(distance.pointB.x, distance.pointB.y, -15), Color.cyan, 3);
                                        // Debug.DrawLine(distance.pointA, distance.pointB, Color.green);
                                        if (!distance.isOverlapped)
                                        {
                                            continue;
                                        }
                                    }

                                    break;
                            }
                        }
                    }
                }

                overlappingComponents.Add(sortingComponent);
            }

            if (polygonColliderToCheck != null)
            {
                DisableCachedCollider(assetGuid, polygonColliderToCheck.GetInstanceID());
            }

            if (overlappingComponents.Count <= 0)
            {
                return false;
            }

            baseItem = sortingComponentToCheck;
            Debug.Log("found " + (overlappingComponents.Count + 1) + " overlapping sprites");
            return true;
        }

        private static void DisableCachedCollider(string assetGuid, int polygonColliderInstanceId)
        {
            var containsColliderArray =
                spriteColliderDataDictionary.TryGetValue(assetGuid, out var polygonColliderArray);
            if (!containsColliderArray)
            {
                return;
            }

            foreach (var polygonCollider in polygonColliderArray)
            {
                if (polygonCollider == null)
                {
                    continue;
                }

                if (polygonCollider.GetInstanceID() != polygonColliderInstanceId)
                {
                    continue;
                }

                polygonCollider.enabled = false;
                break;
            }
        }

        public static void CleanUp()
        {
            foreach (var polygonColliders in spriteColliderDataDictionary.Values)
            {
                if (polygonColliders == null)
                {
                    continue;
                }

                foreach (var polygonCollider in polygonColliders)
                {
                    if (polygonCollider == null)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(polygonCollider.gameObject);
                }
            }
        }

        private static PolygonCollider2D GetCachedColliderOrCreateNewCollider(string assetGuid,
            SpriteDataItem spriteDataItem, Transform transform)
        {
            var containsColliderArray =
                spriteColliderDataDictionary.TryGetValue(assetGuid, out var polygonColliderArray);
            if (!containsColliderArray)
            {
                polygonColliderArray = new PolygonCollider2D[2];

                var polyColliderGameObject =
                    new GameObject("PolygonCollider " + spriteDataItem.AssetName);

                polyColliderGameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                polyColliderGameObject.transform.localScale = transform.lossyScale;

                var polygonCollider = polyColliderGameObject.AddComponent<PolygonCollider2D>();
                polygonCollider.points = spriteDataItem.outlinePoints.ToArray();
                polygonColliderArray[0] = polygonCollider;
                spriteColliderDataDictionary[assetGuid] = polygonColliderArray;
                return polygonCollider;
            }

            for (var i = 0; i < polygonColliderArray.Length; i++)
            {
                var polygonCollider = polygonColliderArray[i];

                if (polygonCollider == null)
                {
                    var polyColliderGameObject =
                        new GameObject("PolygonCollider " + spriteDataItem.AssetName);

                    polyColliderGameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    polyColliderGameObject.transform.localScale = transform.lossyScale;

                    polygonCollider = polyColliderGameObject.AddComponent<PolygonCollider2D>();
                    polygonCollider.points = spriteDataItem.outlinePoints.ToArray();
                    polygonColliderArray[i] = polygonCollider;

                    spriteColliderDataDictionary[assetGuid] = polygonColliderArray;
                    return polygonCollider;
                }

                if (polygonCollider.enabled)
                {
                    continue;
                }

                polygonCollider.transform.SetPositionAndRotation(transform.position, transform.rotation);
                polygonCollider.transform.localScale = transform.lossyScale;

                polygonCollider.points = spriteDataItem.outlinePoints.ToArray();
                polygonCollider.enabled = true;
                return polygonCollider;
            }

            return null;
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

        public static SortingGroup GetOutmostActiveSortingGroup(IReadOnlyList<SortingGroup> groups)
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
            List<OverlappingItem> overlappingItems, SpriteData spriteData,
            OutlinePrecision outlinePrecision = OutlinePrecision.ObjectOrientedBoundingBox)
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

            Debug.Log("start recursion");
            AnalyzeSurroundingSpritesRecursive(cameraProjectionType, spriteData, spriteRenderers,
                baseSortingComponents, excludingSpriteRendererList, outlinePrecision, ref sortingOptions);

            return sortingOptions;
        }

        private static void AnalyzeSurroundingSpritesRecursive(CameraProjectionType cameraProjectionType,
            SpriteData spriteData, List<SpriteRenderer> spriteRenderers,
            List<SortingComponent> baseSortingComponents, List<SpriteRenderer> excludingSpriteRendererList,
            OutlinePrecision outlinePrecision, ref Dictionary<int, int> sortingOptions)
        {
            var filteredSortingComponents = FilterSortingComponents(spriteRenderers,
                new List<int> {baseSortingComponents[0].CurrentSortingLayer}, excludingSpriteRendererList);

            Debug.Log("basSorting Count " + baseSortingComponents.Count);
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

                Debug.Log("check overlapping sprites agains " + baseSortingComponent.spriteRenderer.name);

                if (!CheckOverlappingSprites(cameraProjectionType, filteredSortingComponents, baseSortingComponent,
                    spriteData, false, outlinePrecision, out List<SortingComponent> overlappingSprites,
                    out var baseItem))
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

                    AnalyzeSurroundingSpritesRecursive(cameraProjectionType, spriteData, spriteRenderers,
                        overlappingSprites, newExcludingList, outlinePrecision, ref sortingOptions);
                }
            }
        }

        private static void SortOverlappingSortingComponents(
            ref List<SortingComponent> overlappingSortingComponents,
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