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

        public int[] Sort(AutoSortingComponent autoSortingComponent, AutoSortingComponent otherAutoSortingComponent,
            SpriteData spriteData)
        {
            if (isInitializingData)
            {
                InitializeData();
            }

            return InternalSort(autoSortingComponent, otherAutoSortingComponent, spriteData);
        }

        protected abstract int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent, SpriteData spriteData);

        private void InitializeData()
        {
        }
    }
}