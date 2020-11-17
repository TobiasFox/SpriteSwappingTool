using SpriteSortingPlugin.AutomaticSorting.Criterias;
using SpriteSortingPlugin.AutomaticSorting.CustomEditors;

namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public struct SortingCriteriaComponent
    {
        public SortingCriterionData sortingCriterionData;
        public SortingCriterion<SortingCriterionData> sortingCriterion;
        public CriterionDataBaseEditor<SortingCriterionData> criterionDataBaseEditor;
        public bool isActive;
    }
}