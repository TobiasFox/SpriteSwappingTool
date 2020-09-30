using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
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
                if (CheckOverlappingSprites(data, filteredSortingComponents, sortingComponent,
                    out var overlappingSprites, out var baseItem))
                {
                    result.overlappingItems = overlappingSprites;
                    result.baseItem = baseItem;
                    break;
                }
            }

            return result;
        }

        private static bool CheckOverlappingSprites(SpriteSortingData data,
            IReadOnlyCollection<SortingComponent> filteredSortingComponents, SortingComponent sortingComponentToCheck,
            out List<OverlappingItem> overlappingComponents, out OverlappingItem baseItem)
        {
            overlappingComponents = new List<OverlappingItem>();
            baseItem = null;
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

            baseItem = new OverlappingItem(sortingComponentToCheck, true);
            overlappingComponents.Insert(0, baseItem);
            Debug.Log("found overlapping with " + overlappingComponents.Count + " sprites");
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
    }
}