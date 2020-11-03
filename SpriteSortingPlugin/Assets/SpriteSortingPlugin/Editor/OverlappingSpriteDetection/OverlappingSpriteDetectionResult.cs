using System.Collections.Generic;
using SpriteSortingPlugin.OverlappingSprites;

namespace SpriteSortingPlugin.OverlappingSpriteDetection
{
    public struct OverlappingSpriteDetectionResult
    {
        public List<SortingComponent> overlappingSortingComponents;
        public SortingComponent baseItem;

        public void ConvertToOverlappingItems(out List<OverlappingItem> overlappingItems,
            out OverlappingItem overlappingBaseItem)
        {
            overlappingItems = null;
            overlappingBaseItem = null;

            if (overlappingSortingComponents == null || baseItem == null)
            {
                return;
            }

            overlappingItems = new List<OverlappingItem>();
            overlappingBaseItem = new OverlappingItem(baseItem, true);

            foreach (var overlappingSortingComponent in overlappingSortingComponents)
            {
                overlappingItems.Add(new OverlappingItem(overlappingSortingComponent));
            }
        }
    }
}