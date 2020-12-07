using System.Collections.Generic;

namespace SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites
{
    public class OverlappingItemIndexComparer : Comparer<OverlappingItem>
    {
        public override int Compare(OverlappingItem x, OverlappingItem y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.originSortedIndex.CompareTo(y.originSortedIndex);
        }
    }
}