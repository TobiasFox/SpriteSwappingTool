namespace SpriteSortingPlugin.AutomaticSorting
{
    public abstract class SortingCriterion<T> where T : SortingCriterionData
    {
        protected T sortingCriterionData;
        private bool isInitializingData;

        public SortingCriterion(T sortingCriterionData, bool isInitializingData)
        {
            this.sortingCriterionData = sortingCriterionData;
            this.isInitializingData = isInitializingData;
        }

        public int[] Sort(AutoSortingComponent overlappingItem, AutoSortingComponent otherOverlappingItem,
            SpriteData spriteData)
        {
            if (isInitializingData)
            {
                InitializeData();
            }

            return InternalSort(overlappingItem, otherOverlappingItem, spriteData);
        }

        protected abstract int[] InternalSort(AutoSortingComponent overlappingItem,
            AutoSortingComponent otherOverlappingItem, SpriteData spriteData);

        private void InitializeData()
        {
        }
    }
}