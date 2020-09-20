using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSorting
{
    public static class SpriteSortingUtility
    {
        private const float Tolerance = 0.00001f;

        public static SpriteSortingAnalysisResult AnalyzeSpriteSorting(SpriteSortingData data)
        {
            var result = new SpriteSortingAnalysisResult();

            //TODO: consider prefab scene
            var spriteRenderers = Object.FindObjectsOfType<SpriteRenderer>();

            var sortingGroupParents = new Dictionary<int, SortingGroup[]>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                var sortingGroups = spriteRenderer.GetComponentsInParent<SortingGroup>();
                if (sortingGroups.Length > 0)
                {
                    sortingGroupParents.Add(spriteRenderer.GetInstanceID(), sortingGroups);
                }
            }

            foreach (var spriteRendererToCheck in spriteRenderers)
            {
                if (CheckOverlappingSprites(data, spriteRenderers, sortingGroupParents, spriteRendererToCheck,
                    out List<OverlappingItem> overlappingSprites))
                {
                    result.overlappingItems = overlappingSprites;
                    break;
                }
            }

            return result;
        }

        private static bool CheckOverlappingSprites(SpriteSortingData data,
            SpriteRenderer[] allSpriteRenderers, Dictionary<int, SortingGroup[]> sortingGroupParents,
            SpriteRenderer spriteRendererToCheck, out List<OverlappingItem> overlappingSprites)
        {
            overlappingSprites = new List<OverlappingItem>();
            Debug.Log("start search in " + allSpriteRenderers.Length + " sprite renderers for an overlap with " +
                      spriteRendererToCheck.name);

            var sortingLayerToCheck = spriteRendererToCheck.sortingLayerID;
            var sortingOrderToCheck = spriteRendererToCheck.sortingOrder;

            var spriteRendererToCheckHasSortingGroup = sortingGroupParents.TryGetValue(
                spriteRendererToCheck.GetInstanceID(), out var sortingGroupsOfSpriteRendererToCheck);
            if (spriteRendererToCheckHasSortingGroup)
            {
                var outMostSortingGroup =
                    sortingGroupsOfSpriteRendererToCheck[sortingGroupsOfSpriteRendererToCheck.Length - 1];
                sortingLayerToCheck = outMostSortingGroup.sortingLayerID;
                sortingOrderToCheck = outMostSortingGroup.sortingOrder;
            }

            if (!data.selectedLayers.Contains(sortingLayerToCheck))
            {
                return false;
            }

            foreach (var spriteRenderer in allSpriteRenderers)
            {
                if (spriteRenderer == spriteRendererToCheck ||
                    !spriteRenderer.bounds.Intersects(spriteRendererToCheck.bounds))
                {
                    continue;
                }

                //TODO: is z the distance to the camera? if not maybe create something to choose for the user
                if (data.cameraProjectionType == CameraProjectionType.Orthogonal &&
                    Math.Abs(spriteRenderer.transform.position.z - spriteRendererToCheck.transform.position.z) >
                    Tolerance)
                {
                    continue;
                }

                var currentSortingLayer = spriteRenderer.sortingLayerID;
                var currentSortingOrder = spriteRenderer.sortingOrder;
                var spriteRendererHasSortingGroup =
                    sortingGroupParents.TryGetValue(spriteRenderer.GetInstanceID(), out var sortingGroups);

                if (spriteRendererToCheckHasSortingGroup && spriteRendererHasSortingGroup)
                {
                    // both have sorting groups, check them
                    // SortingGroup sortingGroupSpriteRendererToCheck;
                    // SortingGroup sortingGroupSpriteRenderer;

                    var indices =
                        GetIndicesOfFirstDifferenceInSortingGroups(sortingGroupsOfSpriteRendererToCheck, sortingGroups);
                    var sortingLayerOfFirstDifferenceInSortingGroup = spriteRendererToCheck.sortingLayerID;
                    var sortingOrderOfFirstDifferenceInSortingGroup = spriteRendererToCheck.sortingOrder;

                    if (indices[0] >= 0)
                    {
                        var sortingGroupSpriteRendererToCheck = sortingGroupsOfSpriteRendererToCheck[indices[0]];

                        sortingLayerOfFirstDifferenceInSortingGroup = sortingGroupSpriteRendererToCheck.sortingLayerID;
                        sortingOrderOfFirstDifferenceInSortingGroup = sortingGroupSpriteRendererToCheck.sortingOrder;
                    }

                    if (indices[1] >= 0)
                    {
                        var sortingGroupSpriteRenderer = sortingGroups[indices[1]];

                        currentSortingLayer = sortingGroupSpriteRenderer.sortingLayerID;
                        currentSortingOrder = sortingGroupSpriteRenderer.sortingOrder;
                    }

                    if (currentSortingLayer != sortingLayerOfFirstDifferenceInSortingGroup ||
                        currentSortingOrder != sortingOrderOfFirstDifferenceInSortingGroup)
                    {
                        continue;
                    }

                    overlappingSprites.Add(new OverlappingItem(spriteRenderer));


                    // filteredSpriteRenderer.sortingOrder != spriteRendererToCheck.sortingOrder
                    // data.selectedLayers.
                    // if (sortingGroup != null && sortingGroup.sortingLayerID != spriteRendererToCheck.sortingLayerID &&
                    //     sortingGroup.sortingOrder != spriteRendererToCheck.sortingOrder)
                    continue;
                }

                if (spriteRendererHasSortingGroup)
                {
                    var outMostSortingGroup = sortingGroups[sortingGroups.Length - 1];
                    currentSortingLayer = outMostSortingGroup.sortingLayerID;
                    currentSortingOrder = outMostSortingGroup.sortingOrder;
                }

                if ( /*!data.selectedLayers.Contains(currentSortingLayer) ||*/
                    currentSortingLayer != sortingLayerToCheck ||
                    currentSortingOrder != sortingOrderToCheck)
                {
                    continue;
                }

                overlappingSprites.Add(new OverlappingItem(spriteRenderer));
            }

            if (overlappingSprites.Count <= 0)
            {
                return false;
            }

            overlappingSprites.Add(new OverlappingItem(spriteRendererToCheck));
            Debug.Log("found overlapping with " + overlappingSprites.Count + " sprites");
            return true;
        }

        private static int[] GetIndicesOfFirstDifferenceInSortingGroups(
            IReadOnlyList<SortingGroup> sortingGroupsOfSpriteRendererToCheck,
            IReadOnlyList<SortingGroup> sortingGroups)
        {
            int maxLength = sortingGroups.Count > sortingGroupsOfSpriteRendererToCheck.Count
                ? sortingGroups.Count - 1
                : sortingGroupsOfSpriteRendererToCheck.Count - 1;

            int lastIndexSpriteRendererToCheck = sortingGroupsOfSpriteRendererToCheck.Count - 1;
            int lastIndexSpriteRenderer = sortingGroups.Count - 1;

            for (int i = maxLength; i >= 0; i--)
            {
                var sortingGroupSpriteRendererToCheck = i < sortingGroupsOfSpriteRendererToCheck.Count
                    ? sortingGroupsOfSpriteRendererToCheck[i]
                    : null;
                var sortingGroupSpriteRenderer = i < sortingGroups.Count ? sortingGroups[i] : null;

                if (sortingGroupSpriteRendererToCheck != null && sortingGroupSpriteRenderer != null)
                {
                    if (sortingGroupSpriteRendererToCheck == sortingGroupSpriteRenderer)
                    {
                        lastIndexSpriteRendererToCheck--;
                        lastIndexSpriteRenderer--;
                        continue;
                    }

                    // if (i > 0 &&
                    //     sortingGroupSpriteRendererToCheck.sortingLayerID == sortingGroupSpriteRenderer.sortingLayerID &&
                    //     sortingGroupSpriteRendererToCheck.sortingOrder == sortingGroupSpriteRenderer.sortingOrder)
                    // {
                    //     lastIndexSpriteRendererToCheck--;
                    //     lastIndexSpriteRenderer--;
                    //     continue;
                    // }

                    return new int[] {lastIndexSpriteRendererToCheck, lastIndexSpriteRenderer};
                }

                if (sortingGroupSpriteRendererToCheck == null)
                {
                    lastIndexSpriteRendererToCheck--;
                    return new int[] {lastIndexSpriteRendererToCheck, lastIndexSpriteRenderer};
                }

                lastIndexSpriteRenderer--;
                return new int[] {lastIndexSpriteRendererToCheck, lastIndexSpriteRenderer};
            }

            return new int[] {lastIndexSpriteRendererToCheck, lastIndexSpriteRenderer};
        }

        // int i = sortingGroupsOfSpriteRendererToCheck.Length - 1, j = sortingGroups.Length - 1;
        // for (;
        //     i >= 0 && j >= 0;
        //     i--, j--)
        // {
        //     var sortingGroupSpriteRendererToCheck = sortingGroupsOfSpriteRendererToCheck[i];
        //     var sortingGroupSpriteRenderer = sortingGroups[j];
        //
        //     if (sortingGroupSpriteRendererToCheck == sortingGroupSpriteRenderer)
        //     {
        //         continue;
        //     }
        //
        //     break;
        // }
    }
}