using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class OverlappingSpriteItem
    {
        public readonly int sortingGroupInstanceId;

        public readonly List<SpriteRenderer> overlappingSprites = new List<SpriteRenderer>();

        public OverlappingSpriteItem(int sortingGroupInstanceId)
        {
            this.sortingGroupInstanceId = sortingGroupInstanceId;
        }

        public override string ToString()
        {
            return "OverlappingSpriteItem[" + sortingGroupInstanceId + ", spriteRenderer: " + overlappingSprites.Count +
                   "]";
        }
    }
}