namespace SpriteSortingPlugin.AutomaticSorting
{
    public class AutoSortingComponent
    {
        public readonly SortingComponent sortingComponent;

        public int sortingOrder;
        public SortingComponent containedByAutoSortingComponent;

        public AutoSortingComponent(SortingComponent sortingComponent)
        {
            this.sortingComponent = sortingComponent;
            sortingOrder = sortingComponent.OriginSortingOrder;
        }
    }
}