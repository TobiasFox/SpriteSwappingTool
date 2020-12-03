namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    public class DefaultSortingCriterionData : SortingCriterionData
    {
        public bool isSortingInForeground;
        public string foregroundSortingName;
        public string foregroundSortingTooltip;
        public string criterionName;
        public string criterionTooltip;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<DefaultSortingCriterionData>();
            CopyDataTo(clone);
            clone.isSortingInForeground = isSortingInForeground;
            clone.foregroundSortingName = (string) foregroundSortingName.Clone();
            clone.criterionName = (string) criterionName.Clone();
            clone.criterionTooltip = (string) criterionTooltip.Clone();
            clone.foregroundSortingTooltip = (string) foregroundSortingTooltip.Clone();
            return clone;
        }
    }
}