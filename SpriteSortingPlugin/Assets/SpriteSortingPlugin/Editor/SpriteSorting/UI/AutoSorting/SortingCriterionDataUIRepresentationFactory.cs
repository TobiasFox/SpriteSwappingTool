using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public static class SortingCriterionDataUIRepresentationFactory
    {
        public static CriterionDataBaseUIRepresentation<SortingCriterionData> CreateUIRepresentation(SortingCriterionData data,
            bool isShowingInInspector = false)
        {
            CriterionDataBaseUIRepresentation<SortingCriterionData> uiRepresentation = null;
            switch (data)
            {
                case DefaultSortingCriterionData _:
                    uiRepresentation = new DefaultCriterionDataUIRepresentation();
                    break;
                case ContainmentSortingCriterionData _:
                    uiRepresentation = new ContainmentCriterionDataUIRepresentation();
                    break;
                case PrimaryColorSortingCriterionData _:
                    uiRepresentation = new PrimaryColorCriterionDataUIRepresentation();
                    break;
            }

            if (uiRepresentation == null)
            {
                return null;
            }

            uiRepresentation.Initialize(data, isShowingInInspector);
            return uiRepresentation;
        }
    }
}