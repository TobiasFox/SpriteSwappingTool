namespace SpriteSortingPlugin.AutomaticSorting
{
    public struct SortingCriteriaComponent
    {
        public SortingCriterionData sortingCriterionData;
        public SortingCriterion<SortingCriterionData> sortingCriterion;
        public CriterionDataBaseEditor<SortingCriterionData> criterionDataBaseEditor;
    }
}