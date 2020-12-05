namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    public class DefaultSortingCriterionData : SortingCriterionData
    {
        public bool isSortingInForeground;
        public SortingCriterionType sortingCriterionType;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<DefaultSortingCriterionData>();
            CopyDataTo(clone);
            clone.isSortingInForeground = isSortingInForeground;
            clone.sortingCriterionType = sortingCriterionType;
            return clone;
        }
    }
}