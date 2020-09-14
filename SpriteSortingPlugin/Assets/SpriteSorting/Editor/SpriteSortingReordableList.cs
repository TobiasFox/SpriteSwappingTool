using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSorting
{
    public class SpriteSortingReordableList : ScriptableObject
    {
        [Serializable]
        public class ReordableSpriteSortingItem
        {
            public SpriteRenderer spriteRenderer;
            public SortingGroup sortingGroup;
            public int sortingOrder;
            public int sortingLayer;
        }
     
        public List<ReordableSpriteSortingItem> reordableSpriteSortingItems = new List<ReordableSpriteSortingItem>();
    }
}