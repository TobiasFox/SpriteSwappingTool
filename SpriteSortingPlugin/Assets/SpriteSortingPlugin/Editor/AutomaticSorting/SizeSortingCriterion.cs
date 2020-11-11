namespace SpriteSortingPlugin.AutomaticSorting
{
    public class SizeSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        public SizeSortingCriterion(SizeSortingCriterionData sortingCriterionData) : base(sortingCriterionData, true)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent overlappingItem,
            AutoSortingComponent otherOverlappingItem, SpriteData spriteData)
        {
            return null;
        }
    }
}