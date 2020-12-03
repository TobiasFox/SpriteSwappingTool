using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    public class PrimaryColorSortingCriterionData : SortingCriterionData
    {
        public bool[] activeChannels = new bool[] {true, true, true};
        public Color backgroundColor;
        public Color foregroundColor;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<PrimaryColorSortingCriterionData>();
            CopyDataTo(clone);
            clone.activeChannels = new bool[3];
            for (var i = 0; i < activeChannels.Length; i++)
            {
                clone.activeChannels[i] = activeChannels[i];
            }

            clone.backgroundColor =
                new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);
            clone.foregroundColor =
                new Color(foregroundColor.r, foregroundColor.g, foregroundColor.b, foregroundColor.a);

            return clone;
        }
    }
}