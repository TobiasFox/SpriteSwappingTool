﻿using System;
using System.Collections.Generic;
using UnityEngine;
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
            var filteredSpriteRenderers = new List<SpriteRenderer>();

            foreach (var currentRenderer in spriteRenderers)
            {
                if (data.selectedLayers.Contains(currentRenderer.sortingLayerID))
                {
                    filteredSpriteRenderers.Add(currentRenderer);
                }
            }

            foreach (var filteredSpriteRenderer in filteredSpriteRenderers)
            {
                if (CheckOverlappingSprites(data.cameraProjectionType, filteredSpriteRenderers, filteredSpriteRenderer,
                    out List<SpriteSortingReordableList.ReordableSpriteSortingItem> overlappingSprites))
                {
                    result.overlappingItems = overlappingSprites;
                    break;
                }
            }

            return result;
        }

        private static bool CheckOverlappingSprites(CameraProjectionType cameraProjectionType,
            List<SpriteRenderer> filteredSpriteRenderers, SpriteRenderer spriteRendererToCheck,
            out List<SpriteSortingReordableList.ReordableSpriteSortingItem> overlappingSprites)
        {
            overlappingSprites = new List<SpriteSortingReordableList.ReordableSpriteSortingItem>();
            Debug.Log("start search in " + filteredSpriteRenderers.Count + " sprite renderers for an overlap with " +
                      spriteRendererToCheck.name);
            foreach (var filteredSpriteRenderer in filteredSpriteRenderers)
            {
                if (filteredSpriteRenderer == null || filteredSpriteRenderer == spriteRendererToCheck ||
                    filteredSpriteRenderer.sortingOrder != spriteRendererToCheck.sortingOrder ||
                    !filteredSpriteRenderer.bounds.Intersects(spriteRendererToCheck.bounds))
                {
                    continue;
                }

                if (cameraProjectionType == CameraProjectionType.Orthogonal &&
                    Math.Abs(filteredSpriteRenderer.transform.position.z - spriteRendererToCheck.transform.position.z) >
                    Tolerance)
                {
                    continue;
                }

                overlappingSprites.Add(new SpriteSortingReordableList.ReordableSpriteSortingItem
                    {spriteRenderer = filteredSpriteRenderer});
            }

            if (overlappingSprites.Count <= 0)
            {
                return false;
            }

            overlappingSprites.Add(new SpriteSortingReordableList.ReordableSpriteSortingItem
                {spriteRenderer = spriteRendererToCheck});
            Debug.Log("found overlapping with " + overlappingSprites.Count + " sprites");
            return true;
        }
    }
}