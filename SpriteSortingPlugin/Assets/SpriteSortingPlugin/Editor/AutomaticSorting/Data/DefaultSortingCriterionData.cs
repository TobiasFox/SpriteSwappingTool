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
            clone.isSortingInForeground = isSortingInForeground;
            clone.foregroundSortingName = foregroundSortingName;
            clone.criterionName = criterionName;
            return clone;
        }
    }
}