using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSorting
{
    [Serializable]
    public class ReordableSpriteSortingItem
    {
        public SpriteRenderer originSpriteRenderer;
        public SortingGroup originSortingGroup;
        public int originSortingOrder;
        public int originSortingLayer;
        
        public SpriteRenderer tempSpriteRenderer;
        public SortingGroup tempSortingGroup;
        public int sortingOrder;
        public int sortingLayer;

        public ReordableSpriteSortingItem(SpriteRenderer originSpriteRenderer)
        {
            this.originSpriteRenderer = originSpriteRenderer;
            originSortingLayer = originSpriteRenderer.sortingLayerID;
            originSortingOrder = originSpriteRenderer.sortingOrder;
        }

        public ReordableSpriteSortingItem(SortingGroup originSortingGroup)
        {
            this.originSortingGroup = originSortingGroup;
            originSortingLayer = originSortingGroup.sortingLayerID;
            originSortingOrder = originSortingGroup.sortingOrder;
        }
    }
}