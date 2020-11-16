using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class PrimaryColorSortingCriterionData : SortingCriterionData
    {
        public bool isUsingSpriteColor = true;
        public bool isUsingSpriteRendererColor;
        public bool[] isChannelActive = new bool[] {true, true, true};
        public Color backgroundColor;
        public Color foregroundColor;
    }
}