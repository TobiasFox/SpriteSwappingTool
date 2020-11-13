using SpriteSortingPlugin.OverlappingSprites;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public abstract class SortingCriterion<T> where T : SortingCriterionData
    {
        protected readonly T sortingCriterionData;
        protected SpriteDataItemValidator spriteDataItemValidator;
        protected SpriteDataItemValidator otherSpriteDataItemValidator;
        protected SpriteData spriteData;
        protected OutlinePrecision preferredOutlinePrecision;

        protected SortingCriterion(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
        }

        public int[] Sort(AutoSortingComponent autoSortingComponent, AutoSortingComponent otherAutoSortingComponent,
            SpriteData spriteData, OutlinePrecision preferredOutlinePrecision)
        {
            this.spriteData = spriteData;
            this.preferredOutlinePrecision = preferredOutlinePrecision;

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(autoSortingComponent.spriteRenderer);
            otherSpriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(autoSortingComponent.spriteRenderer);

            return InternalSort(autoSortingComponent, otherAutoSortingComponent);
        }

        protected abstract int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent);
    }
}