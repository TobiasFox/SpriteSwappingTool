namespace SpriteSortingPlugin.AutomaticSorting
{
    public class AutoSortingComponent : SortingComponent
    {
        public int sortingOrder;
        public SortingComponent containedByAutoSortingComponent;

        public AutoSortingComponent(SortingComponent sortingComponent) : base(sortingComponent)
        {
            sortingOrder = OriginSortingOrder;
        }
    }
}