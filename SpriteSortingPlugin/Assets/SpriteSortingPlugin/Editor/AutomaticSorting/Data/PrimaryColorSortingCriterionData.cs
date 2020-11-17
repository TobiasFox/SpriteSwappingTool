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

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<PrimaryColorSortingCriterionData>();
            CopyDataTo(clone);
            clone.isUsingSpriteColor = isUsingSpriteColor;
            clone.isUsingSpriteRendererColor = isUsingSpriteRendererColor;
            clone.isChannelActive = new bool[3];
            for (int i = 0; i < isChannelActive.Length; i++)
            {
                clone.isChannelActive[i] = isChannelActive[i];
            }

            clone.backgroundColor =
                new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);
            clone.foregroundColor =
                new Color(foregroundColor.r, foregroundColor.g, foregroundColor.b, foregroundColor.a);

            return clone;
        }
    }
}