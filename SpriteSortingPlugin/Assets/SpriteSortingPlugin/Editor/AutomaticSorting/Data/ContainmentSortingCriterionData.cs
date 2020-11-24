namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class ContainmentSortingCriterionData : SortingCriterionData
    {
        public bool isCheckingAlpha;
        public float alphaThreshold = 0.2f;
        public bool isSortingEnclosedSpriteInForeground;
        public bool isUsingSpriteColor = true;
        public bool isUsingSpriteRendererColor;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<ContainmentSortingCriterionData>();
            CopyDataTo(clone);
            clone.isSortingEnclosedSpriteInForeground = isSortingEnclosedSpriteInForeground;
            clone.isCheckingAlpha = isCheckingAlpha;
            clone.isUsingSpriteColor = isUsingSpriteColor;
            clone.isUsingSpriteRendererColor = isUsingSpriteRendererColor;
            clone.alphaThreshold = alphaThreshold;
            return clone;
        }
    }
}