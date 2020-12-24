using System;

namespace SpriteSortingPlugin.SpriteSorting.Logging
{
    [Serializable]
    public class OverlappingItemLoggingData
    {
        public string spriteRendererName;
        public int originSortingOrder;
        public int originSortingLayerIndex;
        public int originAutoSortingOrder;
        public bool isBaseItem;
    }
}