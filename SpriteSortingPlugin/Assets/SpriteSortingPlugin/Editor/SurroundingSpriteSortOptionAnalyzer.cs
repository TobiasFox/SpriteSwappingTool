using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SurroundingSpriteSortOptionAnalyzer
    {
        private SortingComponentShortestDifferenceComparer sortingComponentShortestDifferenceComparer;
        private SpriteDetectionData spriteDetectionData;

        public Dictionary<int, int> AnalyzeSurroundingSprites(List<OverlappingItem> overlappingItems,
            SpriteDetectionData spriteDetectionData)
        {
            if (overlappingItems == null || overlappingItems.Count <= 0)
            {
                return null;
            }

            var spriteRenderers = SpriteSortingUtility.InitializeSpriteRendererList(null);
            if (spriteRenderers.Count < 2)
            {
                return null;
            }

            this.spriteDetectionData = spriteDetectionData;
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
            AnalyzeSurroundingSpritesRecursive(spriteRenderers, baseSortingComponents, excludingSpriteRendererList,
                ref sortingOptions);

            return sortingOptions;
        }

        private void AnalyzeSurroundingSpritesRecursive(List<SpriteRenderer> spriteRenderers,
            List<SortingComponent> baseSortingComponents, List<SpriteRenderer> excludingSpriteRendererList,
            ref Dictionary<int, int> sortingOptions)
        {
            var filteredSortingComponents = SpriteSortingUtility.FilterSortingComponents(spriteRenderers,
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

                var overlappingSpriteDetector = new OverlappingSpriteDetector();

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

                    AnalyzeSurroundingSpritesRecursive(spriteRenderers, overlappingSprites, newExcludingList,
                        ref sortingOptions);
                }
            }
        }

        private void SortOverlappingSortingComponents(ref List<SortingComponent> overlappingSortingComponents,
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