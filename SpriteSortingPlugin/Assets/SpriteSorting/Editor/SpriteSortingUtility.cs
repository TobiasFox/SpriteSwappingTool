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
            if (spriteRenderers.Length < 2)
            {
                return result;
            }

            var filteredSortingComponents = new List<SortingComponent>();
            var spriteDictionary = new Dictionary<int, Dictionary<int, SpriteRenderer>>();

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (!spriteRenderer.enabled)
                {
                    continue;
                }

                var sortingGroupArray = spriteRenderer.GetComponentsInParent<SortingGroup>();
                var outmostSortingGroup = GetOutmostActiveSortingGroup(sortingGroupArray);

                if (outmostSortingGroup == null)
                {
                    if (!data.selectedLayers.Contains(spriteRenderer.sortingLayerID))
                    {
                        continue;
                    }

                    filteredSortingComponents.Add(new SortingComponent(spriteRenderer));
                    continue;
                }

                if (!data.selectedLayers.Contains(outmostSortingGroup.sortingLayerID))
                {
                    continue;
                }

                filteredSortingComponents.Add(new SortingComponent(spriteRenderer, outmostSortingGroup));
            }

            Debug.Log("filtered spriteRenderers with SortingGroup with no parent: from " + spriteRenderers.Length +
                      " to " + filteredSortingComponents.Count);

            //TODO: optimize foreach
            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (CheckOverlappingSprites(data, filteredSortingComponents, sortingComponent, ref spriteDictionary,
                    out var overlappingSprites))
                {
                    // var overlappingSpriteList = new List<OverlappingSpriteItem>();
                    // foreach (var spriteDictionaryItem in spriteDictionary)
                    // {
                    //     var overlappingSpriteItem = new OverlappingSpriteItem(spriteDictionaryItem.Key);
                    //     foreach (var spriteRenderer in spriteDictionaryItem.Value.Values)
                    //     {
                    //         overlappingSpriteItem.overlappingSprites.Add(spriteRenderer);
                    //     }
                    //
                    //     overlappingSpriteList.Add(overlappingSpriteItem);
                    // }

                    // var overlappingSpriteList = new List<List<SpriteRenderer>>();
                    // foreach (var spriteDictionaries in spriteDictionary.Values)
                    // {
                    //     overlappingSpriteList.Add(new List<SpriteRenderer>(spriteDictionaries.Values));
                    // }

                    // result.overlappingSpriteList = overlappingSpriteList;
                    result.overlappingItems = overlappingSprites;
                    break;
                }
            }

            return result;
        }

        private static bool CheckOverlappingSprites(SpriteSortingData data,
            IReadOnlyCollection<SortingComponent> filteredSortingComponents, SortingComponent sortingComponentToCheck,
            ref Dictionary<int, Dictionary<int, SpriteRenderer>> spriteDictionary,
            out List<OverlappingItem> overlappingComponents)
        {
            overlappingComponents = new List<OverlappingItem>();
            Debug.Log("start search in " + filteredSortingComponents.Count + " sprite renderers for an overlap with " +
                      sortingComponentToCheck.spriteRenderer.name);

            foreach (var sortingComponent in filteredSortingComponents)
            {
                if (sortingComponentToCheck.Equals(sortingComponent))
                {
                    continue;
                }

                if (!sortingComponent.spriteRenderer.bounds.Intersects(sortingComponentToCheck.spriteRenderer.bounds))
                {
                    continue;
                }

                if (sortingComponentToCheck.sortingGroup != null && sortingComponent.sortingGroup != null &&
                    sortingComponentToCheck.sortingGroup == sortingComponent.sortingGroup)
                {
                    // UpdateSpriteDictionary(ref spriteDictionary, sortingComponentToCheck.sortingGroup.GetInstanceID(),
                    // sortingComponent.spriteRenderer, sortingComponentToCheck.spriteRenderer);

                    continue;
                }

                //TODO: is z the distance to the camera? if not maybe create something to choose for the user
                if (data.cameraProjectionType == CameraProjectionType.Orthogonal &&
                    Math.Abs(sortingComponent.spriteRenderer.transform.position.z -
                             sortingComponentToCheck.spriteRenderer.transform.position.z) >
                    Tolerance)
                {
                    continue;
                }

                if (sortingComponentToCheck.CurrentSortingLayer != sortingComponent.CurrentSortingLayer ||
                    sortingComponentToCheck.CurrentSortingOrder != sortingComponent.CurrentSortingOrder)
                {
                    continue;
                }

                overlappingComponents.Add(new OverlappingItem(sortingComponent));
            }

            if (overlappingComponents.Count <= 0)
            {
                return false;
            }

            overlappingComponents.Add(new OverlappingItem(sortingComponentToCheck));
            Debug.Log("found overlapping with " + overlappingComponents.Count + " sprites");
            return true;
        }

        // private static void UpdateSpriteDictionary(
        //     ref Dictionary<int, Dictionary<int, SpriteRenderer>> spriteDictionary,
        //     SpriteRenderer spriteRenderer)
        // {
        //     var isSpriteRendererContained =
        //         spriteDictionary.TryGetValue(spriteRenderer.GetInstanceID(), out var overlappingDictionary);
        //     if (!isSpriteRendererContained)
        //     {
        //         overlappingDictionary = new Dictionary<int, SpriteRenderer>();
        //         overlappingDictionary.Add();
        //     }
        //
        //     foreach (var renderer in spriteRendererList)
        //     {
        //         if (renderer == spriteRenderer)
        //         {
        //             return;
        //         }
        //     }
        //
        //     //needs to check, if renderer is already contained
        //
        //     spriteRendererList.Add(spriteRenderer);
        //     spriteDictionary[spriteRenderer.GetInstanceID()] = spriteRendererList;
        // }

        private static void UpdateSpriteDictionary(
            ref Dictionary<int, Dictionary<int, SpriteRenderer>> spriteDictionary, int sortingGroupInstanceId,
            params SpriteRenderer[] renderers)
        {
            var isSpriteRendererContained =
                spriteDictionary.TryGetValue(sortingGroupInstanceId, out var overlappingDictionary);
            if (!isSpriteRendererContained)
            {
                overlappingDictionary = new Dictionary<int, SpriteRenderer>();
                foreach (var spriteRenderer in renderers)
                {
                    overlappingDictionary.Add(spriteRenderer.GetInstanceID(), spriteRenderer);
                }

                spriteDictionary[sortingGroupInstanceId] = overlappingDictionary;

                return;
            }

            foreach (var spriteRenderer in renderers)
            {
                if (overlappingDictionary.ContainsKey(spriteRenderer.GetInstanceID()))
                {
                    continue;
                }

                overlappingDictionary.Add(spriteRenderer.GetInstanceID(), spriteRenderer);
            }
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
    }
}