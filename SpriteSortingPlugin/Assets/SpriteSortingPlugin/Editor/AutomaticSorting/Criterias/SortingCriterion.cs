using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.OverlappingSprites;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public abstract class SortingCriterion<T> where T : SortingCriterionData
    {
        protected readonly T sortingCriterionData;
        protected SpriteDataItemValidator spriteDataItemValidator;
        protected SpriteDataItemValidator otherSpriteDataItemValidator;
        protected AutoSortingCalculationData autoSortingCalculationData;
        protected int[] sortingResults;

        protected SortingCriterion(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
        }

        public int[] Sort(SortingComponent sortingComponent, SortingComponent otherSortingComponent,
            AutoSortingCalculationData autoSortingCalculationData)
        {
            this.autoSortingCalculationData = autoSortingCalculationData;

            if (IsUsingSpriteData())
            {
                var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
                spriteDataItemValidator =
                    spriteDataItemValidatorCache.GetOrCreateValidator(sortingComponent.spriteRenderer);
                otherSpriteDataItemValidator =
                    spriteDataItemValidatorCache.GetOrCreateValidator(otherSortingComponent.spriteRenderer);
            }

            sortingResults = new int[2];

            //TODO validate key in map before calling method
            InternalSort(sortingComponent, otherSortingComponent);

            for (var i = 0; i < sortingResults.Length; i++)
            {
                sortingResults[i] *= sortingCriterionData.priority;
            }

            Debug.Log(GetType().Name + " sorted: [" + sortingResults[0] + "," + sortingResults[1] + "]");

            return sortingResults;
        }

        protected abstract void InternalSort(SortingComponent sortingComponent,
            SortingComponent otherSortingComponent);

        public abstract bool IsUsingSpriteData();
    }
}