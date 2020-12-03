using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites
{
    public class OverlappingItemIdentityComparer : Comparer<OverlappingItem>
    {
        public override int Compare(OverlappingItem x, OverlappingItem y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var sortingLayerComparison = SortingLayer.GetLayerValueFromName(x.sortingLayerName)
                .CompareTo(SortingLayer.GetLayerValueFromName(y.sortingLayerName));

            if (sortingLayerComparison < 0 || sortingLayerComparison > 0)
            {
                return sortingLayerComparison;
            }

            return y.sortingOrder.CompareTo(x.sortingOrder);
        }
    }
}