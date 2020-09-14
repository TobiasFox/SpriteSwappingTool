using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSorting
{
    public struct SpriteSortingAnalysisResult
    {
        public List<SpriteRenderer> overlappingRenderers;
        public List<SortingGroup> overlappingSortingGroups;
        public List<ReordableSpriteSortingItem> overlappingItems;
    }
}
