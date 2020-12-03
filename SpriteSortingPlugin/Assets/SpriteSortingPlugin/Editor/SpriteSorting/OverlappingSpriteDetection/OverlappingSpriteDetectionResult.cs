using System.Collections.Generic;

namespace SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection
{
    public struct OverlappingSpriteDetectionResult
    {
        public List<SortingComponent> overlappingSortingComponents;
        public SortingComponent baseItem;
    }
}