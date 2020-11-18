namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class ContainmentSortingCriterionData : DefaultSortingCriterionData
    {
        public ContainmentSortingCriterionData()
        {
            isSortingInForeground = true;
            criterionName = "Containment";
            foregroundSortingName = "is contained sprite in foreground";
        }
    }
}