using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSorting
{
    [Serializable]
    public class OverlappingItem
    {
        public SpriteRenderer originSpriteRenderer;
        public SortingGroup originSortingGroup;
        public int originSortingOrder;
        public int originSortingLayer;

        public SpriteRenderer previewSpriteRenderer;
        public SortingGroup previewSortingGroup;
        public int sortingOrder;
        public int sortingLayer;

        public bool IsItemSelected { get; set; }
        public int OriginSortedIndex { get; set; }

        public OverlappingItem(SpriteRenderer originSpriteRenderer)
        {
            this.originSpriteRenderer = originSpriteRenderer;
            originSortingLayer = originSpriteRenderer.sortingLayerID;
            originSortingOrder = originSpriteRenderer.sortingOrder;
        }

        public OverlappingItem(SortingGroup originSortingGroup)
        {
            this.originSortingGroup = originSortingGroup;
            originSortingLayer = originSortingGroup.sortingLayerID;
            originSortingOrder = originSortingGroup.sortingOrder;
        }

        public void UpdatePreviewSortingOrderWithExistingOrder()
        {
            if (previewSortingGroup != null)
            {
                previewSortingGroup.sortingOrder = originSortingOrder + sortingOrder;
            }
            else if (previewSpriteRenderer != null)
            {
                previewSpriteRenderer.sortingOrder = originSortingOrder + sortingOrder;
            }
        }

        public void UpdatePreviewSortingLayer(string sortingLayerName)
        {
            if (previewSortingGroup != null)
            {
                previewSortingGroup.sortingLayerName = sortingLayerName;
            }
            else if (previewSpriteRenderer != null)
            {
                previewSpriteRenderer.sortingLayerName = sortingLayerName;
            }
        }
    }
}