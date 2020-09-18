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
            SpriteRenderer spriteRendererToCheck,
            out List<OverlappingItem> overlappingSprites)
        {
            overlappingSprites = new List<OverlappingItem>();
            Debug.Log("start search in " + allSpriteRenderers.Length + " sprite renderers for an overlap with " +
                      spriteRendererToCheck.name);

            var sortingLayerToCheck = spriteRendererToCheck.sortingLayerID;
            var sortingOrderToCheck = spriteRendererToCheck.sortingOrder;

            var isSortingGroupContained = sortingGroupParents.TryGetValue(spriteRendererToCheck.GetInstanceID(),
                out var sortingGroupsOfSpriteRendererToCheck);
            if (isSortingGroupContained)
            {
                var outMostSortingGroup =
                    sortingGroupsOfSpriteRendererToCheck[sortingGroupsOfSpriteRendererToCheck.Length - 1];
                sortingLayerToCheck = outMostSortingGroup.sortingLayerID;
                sortingOrderToCheck = outMostSortingGroup.sortingOrder;
            }

            foreach (var spriteRenderer in allSpriteRenderers)
            {
                if (spriteRenderer == spriteRendererToCheck ||
                    !spriteRenderer.bounds.Intersects(spriteRendererToCheck.bounds))
                {
                    continue;
                }

                if (data.cameraProjectionType == CameraProjectionType.Orthogonal &&
                    Math.Abs(spriteRenderer.transform.position.z - spriteRendererToCheck.transform.position.z) >
                    Tolerance)
                {
                    continue;
                }

                var spriteRendererHasSortingGroupParents =
                    sortingGroupParents.TryGetValue(spriteRenderer.GetInstanceID(), out var sortingGroups);

                if (!spriteRendererHasSortingGroupParents || !isSortingGroupContained)
                {
                    //sprite has no SortingGroup in parent
                    if (!data.selectedLayers.Contains(sortingLayerToCheck) ||
                        spriteRenderer.sortingLayerID != sortingLayerToCheck ||
                        spriteRenderer.sortingOrder != sortingOrderToCheck)
                    {
                        continue;
                    }

                    overlappingSprites.Add(new OverlappingItem(spriteRenderer));
                    continue;
                }

                //sprite has SortingGroups in parents

                SortingGroup sortingGroupSpriteRendererToCheck;
                SortingGroup sortingGroupSpriteRenderer;

                for (int i = sortingGroupsOfSpriteRendererToCheck.Length - 1, j = sortingGroups.Length - 1;
                    i >= 0 && j >= 0;
                    i--, j--)
                {
                    sortingGroupSpriteRendererToCheck = sortingGroupsOfSpriteRendererToCheck[i];
                    sortingGroupSpriteRenderer = sortingGroups[j];

                    if (sortingGroupSpriteRendererToCheck != null && sortingGroupSpriteRenderer != null &&
                        sortingGroupSpriteRendererToCheck == sortingGroupSpriteRenderer)
                    {
                        continue;
                    }
                }

                // filteredSpriteRenderer.sortingOrder != spriteRendererToCheck.sortingOrder
                // data.selectedLayers.

                // if (sortingGroup != null && sortingGroup.sortingLayerID != spriteRendererToCheck.sortingLayerID &&
                //     sortingGroup.sortingOrder != spriteRendererToCheck.sortingOrder)
                // {
                //     continue;
                // }
            }

            if (overlappingSprites.Count <= 0)
            {
                return false;
            }

            overlappingSprites.Add(new OverlappingItem(spriteRendererToCheck));
            Debug.Log("found overlapping with " + overlappingSprites.Count + " sprites");
            return true;
        }
    }
}