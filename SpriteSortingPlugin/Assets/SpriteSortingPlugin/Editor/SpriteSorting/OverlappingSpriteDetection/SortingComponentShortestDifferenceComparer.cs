using System;
using System.Collections.Generic;

namespace SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection
{
    public class SortingComponentShortestDifferenceComparer : Comparer<SortingComponent>
    {
        public int baseSortingOrder;

        public override int Compare(SortingComponent x, SortingComponent y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var sortingOrderDifX = Math.Abs(baseSortingOrder - x.OriginSortingOrder);
            var sortingOrderDifY = Math.Abs(baseSortingOrder - y.OriginSortingOrder);

            return sortingOrderDifX.CompareTo(sortingOrderDifY);
        }
    }
}