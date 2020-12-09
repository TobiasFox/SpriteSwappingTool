using System;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    [Serializable]
    public class ContainmentSortingCriterionData : SortingCriterionData
    {
        public bool isCheckingAlpha;
        public float alphaThreshold = 0.2f;
        public bool isSortingEnclosedSpriteInForeground = true;

        public ContainmentSortingCriterionData()
        {
            sortingCriterionType = SortingCriterionType.Containment;
        }

        public override object Clone()
        {
            var clone = new ContainmentSortingCriterionData();
            CopyDataTo(clone);
            clone.isSortingEnclosedSpriteInForeground = isSortingEnclosedSpriteInForeground;
            clone.isCheckingAlpha = isCheckingAlpha;
            clone.alphaThreshold = alphaThreshold;
            return clone;
        }
    }
}