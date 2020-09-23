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

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (!spriteRenderer.enabled)
                {
                    continue;
                }

                var sortingGroupArray = spriteRenderer.GetComponentsInParent<SortingGroup>();
                var sortingGroups = FilterSortingGroups(sortingGroupArray);

                if (sortingGroups.Count <= 0)
                {
                    if (!data.selectedLayers.Contains(spriteRenderer.sortingLayerID))
                    {
                        continue;
                    }

                    filteredSortingComponents.Add(new SortingComponent(spriteRenderer));
                    continue;
                }

                var outmostSortingGroup = sortingGroups[sortingGroups.Count - 1];
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
                    out var overlappingSprites))
                {
                    result.overlappingItems = overlappingSprites;
                    break;
                }
            }

            return result;
        }

        private static bool CheckOverlappingSprites(SpriteSortingData data,
            IReadOnlyCollection<SortingComponent> filteredSortingComponents, SortingComponent sortingComponentToCheck,
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

                if (sortingComponentToCheck.sortingGroup != null && sortingComponent.sortingGroup != null &&
                    sortingComponentToCheck.sortingGroup == sortingComponent.sortingGroup)
                {
                    continue;
                }

                if (!sortingComponent.spriteRenderer.bounds.Intersects(sortingComponentToCheck.spriteRenderer.bounds))
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

            overlappingComponents.Add(new OverlappingItem(sortingComponentToCheck));
            Debug.Log("found overlapping with " + overlappingComponents.Count + " sprites");
            return true;
        }

        private static List<SortingGroup> FilterSortingGroups(SortingGroup[] groups)
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
    }
}