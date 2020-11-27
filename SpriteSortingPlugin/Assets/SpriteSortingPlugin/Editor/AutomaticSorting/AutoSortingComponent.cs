namespace SpriteSortingPlugin.AutomaticSorting
{
    public class AutoSortingComponent : SortingComponent
    {
        public int sortingOrder;
        public AutoSortingComponent containedByAutoSortingComponent;

        public AutoSortingComponent(SortingComponent sortingComponent) : base(sortingComponent)
        {
            sortingOrder = OriginSortingOrder;
        }
    }
}