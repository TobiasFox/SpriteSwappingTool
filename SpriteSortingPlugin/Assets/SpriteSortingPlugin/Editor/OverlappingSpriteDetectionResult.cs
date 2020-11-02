using System.Collections.Generic;

namespace SpriteSortingPlugin
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

            if (overlappingItems == null || baseItem == null)
            {
                return;
            }

            overlappingItems = new List<OverlappingItem>();
            overlappingBaseItem = new OverlappingItem(baseItem, true);
            overlappingItems.Add(overlappingBaseItem);

            foreach (var overlappingSortingComponent in overlappingSortingComponents)
            {
                overlappingItems.Add(new OverlappingItem(overlappingSortingComponent));
            }
        }
    }
}