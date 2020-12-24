using System;

namespace SpriteSortingPlugin.SpriteSorting.Logging
{
    [Serializable]
    public class SortingSuggestionModificationData
    {
        public ModificationType type;

        public int order;
        public int layerIndex;
        public int itemIndex;
        public bool isRelative;

        public int modifiedOrder;
        public int modifiedLayerIndex;
        public int modifiedItemIndex;
    }
}