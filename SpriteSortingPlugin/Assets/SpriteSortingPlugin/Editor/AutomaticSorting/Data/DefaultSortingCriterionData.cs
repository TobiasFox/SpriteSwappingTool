namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class DefaultSortingCriterionData : SortingCriterionData
    {
        public bool isSortingInForeground;
        public string foregroundSortingName;
        public string criterionName;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<DefaultSortingCriterionData>();
            CopyDataTo(clone);
            clone.isSortingInForeground = isSortingInForeground;
            clone.foregroundSortingName = (string) foregroundSortingName.Clone();
            clone.criterionName = (string) criterionName.Clone();
            return clone;
        }
    }
}