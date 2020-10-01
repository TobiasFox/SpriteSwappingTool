using System;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
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
        public int sortingLayerDropDownIndex;
        public string sortingLayerName;

        public bool IsItemSelected { get; set; }
        public int OriginSortedIndex { get; set; }
        public bool IsBaseItem { get; private set; }

        public OverlappingItem(SpriteRenderer originSpriteRenderer)
        {
            this.originSpriteRenderer = originSpriteRenderer;
            originSortingGroup = originSpriteRenderer.GetComponentInParent<SortingGroup>();

            if (originSortingGroup != null)
            {
                originSortingLayer = originSortingGroup.sortingLayerID;
                originSortingOrder = originSortingGroup.sortingOrder;
            }
            else
            {
                originSortingLayer = originSpriteRenderer.sortingLayerID;
                originSortingOrder = originSpriteRenderer.sortingOrder;
            }
        }

        public OverlappingItem(SortingComponent sortingComponent, bool isBaseItem = false)
        {
            originSpriteRenderer = sortingComponent.spriteRenderer;
            originSortingGroup = sortingComponent.outmostSortingGroup;
            IsBaseItem = isBaseItem;

            if (originSortingGroup != null)
            {
                originSortingLayer = originSortingGroup.sortingLayerID;
                originSortingOrder = originSortingGroup.sortingOrder;
            }
            else
            {
                originSortingLayer = originSpriteRenderer.sortingLayerID;
                originSortingOrder = originSpriteRenderer.sortingOrder;
            }
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

        public void UpdatePreviewSortingLayer()
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

        public void UpdatePreview()
        {
            UpdatePreviewSortingOrderWithExistingOrder();
            UpdatePreviewSortingLayer();
        }

        public void CleanUpPreview()
        {
            if (previewSpriteRenderer != null)
            {
                Object.DestroyImmediate(previewSpriteRenderer.gameObject);
            }
        }

        public bool HasSortingLayerChanged()
        {
            return originSortingLayer !=
                   SortingLayer.NameToID(SortingLayerUtility.SortingLayerNames[sortingLayerDropDownIndex]);
        }

        public override string ToString()
        {
            return "OI[origin: " + SortingLayer.IDToName(originSortingLayer) + ", " + originSortingOrder +
                   " current: " + sortingLayerName + ", " + (originSortingOrder + sortingOrder) + "(" +
                   originSortingOrder + "+" + sortingOrder + "), baseItem: " + IsBaseItem +
                   ", originSortedIndex:" + OriginSortedIndex + "]";
        }
    }
}