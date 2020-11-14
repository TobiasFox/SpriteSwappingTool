using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.OverlappingSprites;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public abstract class SortingCriterion<T> where T : SortingCriterionData
    {
        protected readonly T sortingCriterionData;
        protected SpriteDataItemValidator spriteDataItemValidator;
        protected SpriteDataItemValidator otherSpriteDataItemValidator;
        protected AutoSortingCalculationData autoSortingCalculationData;

        protected SortingCriterion(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
        }

        public int[] Sort(AutoSortingComponent autoSortingComponent, AutoSortingComponent otherAutoSortingComponent,
            AutoSortingCalculationData autoSortingCalculationData)
        {
            this.autoSortingCalculationData = autoSortingCalculationData;

            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(autoSortingComponent.spriteRenderer);
            otherSpriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(autoSortingComponent.spriteRenderer);

            var results = InternalSort(autoSortingComponent, otherAutoSortingComponent);

            for (int i = 0; i < results.Length; i++)
            {
                results[i] *= sortingCriterionData.priority;
            }

            return results;
        }

        protected abstract int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent);
    }
}