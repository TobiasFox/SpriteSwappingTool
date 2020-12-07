using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.UI.AutoSorting;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    public struct SortingCriteriaComponent
    {
        public SortingCriterionData sortingCriterionData;
        public SortingCriterion<SortingCriterionData> sortingCriterion;
        public CriterionDataBaseUIRepresentation<SortingCriterionData> criterionDataBaseUIRepresentation;
    }
}