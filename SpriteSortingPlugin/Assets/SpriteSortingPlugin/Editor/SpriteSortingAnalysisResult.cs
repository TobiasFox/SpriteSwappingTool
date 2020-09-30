using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct SpriteSortingAnalysisResult
    {
        public List<SpriteRenderer> overlappingRenderers;
        public List<SortingGroup> overlappingSortingGroups;
        public List<OverlappingItem> overlappingItems;
        public OverlappingItem baseItem;
        public List<SortingComponent> overlappingSortingComponents;
        // public List<OverlappingSpriteItem> overlappingSpriteList;
    }
}
