using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct SpriteSortingAnalysisResult
    {
        public List<OverlappingItem> overlappingItems;
        public OverlappingItem baseItem;
    }
}
