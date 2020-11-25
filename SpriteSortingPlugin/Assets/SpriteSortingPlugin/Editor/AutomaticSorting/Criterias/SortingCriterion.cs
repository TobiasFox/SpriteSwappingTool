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

        public int[] Sort(AutoSortingComponent autoSortingComponent, AutoSortingComponent otherAutoSortingComponent,
            AutoSortingCalculationData autoSortingCalculationData)
        {
            this.autoSortingCalculationData = autoSortingCalculationData;

            if (IsUsingSpriteData())
            {
                var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
                spriteDataItemValidator =
                    spriteDataItemValidatorCache.GetOrCreateValidator(autoSortingComponent.OriginSpriteRenderer);
                otherSpriteDataItemValidator =
                    spriteDataItemValidatorCache.GetOrCreateValidator(otherAutoSortingComponent.OriginSpriteRenderer);
            }

            sortingResults = new int[2];

            InternalSort(autoSortingComponent, otherAutoSortingComponent);

            for (int i = 0; i < sortingResults.Length; i++)
            {
                sortingResults[i] *= sortingCriterionData.priority;
            }

            Debug.Log(GetType().Name + " sorted: [" + sortingResults[0] + "," + sortingResults[1] + "]");

            return sortingResults;
        }

        protected abstract void InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent);

        public abstract bool IsUsingSpriteData();
    }
}