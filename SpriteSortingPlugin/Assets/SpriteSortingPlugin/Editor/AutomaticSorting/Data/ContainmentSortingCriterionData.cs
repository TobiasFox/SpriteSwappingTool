namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class ContainmentSortingCriterionData : SortingCriterionData
    {
        public bool isCheckingAlpha;
        public float alphaThreshold = 0.2f;
        public bool isSortingEnclosedSpriteInForeground = true;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<ContainmentSortingCriterionData>();
            CopyDataTo(clone);
            clone.isSortingEnclosedSpriteInForeground = isSortingEnclosedSpriteInForeground;
            clone.isCheckingAlpha = isCheckingAlpha;
            clone.alphaThreshold = alphaThreshold;
            return clone;
        }
    }
}