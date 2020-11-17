namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class ResolutionSortingCriterionData : SortingCriterionData
    {
        public bool isSpriteWithHigherResolutionInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<ResolutionSortingCriterionData>();
            CopyDataTo(clone);
            clone.isSpriteWithHigherResolutionInForeground = isSpriteWithHigherResolutionInForeground;
            return clone;
        }
    }
}