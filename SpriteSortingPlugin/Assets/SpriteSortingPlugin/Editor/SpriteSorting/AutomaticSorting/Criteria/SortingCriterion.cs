using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public abstract class SortingCriterion<T> where T : SortingCriterionData
    {
        protected readonly T sortingCriterionData;
        protected SpriteDataItemValidator spriteDataItemValidator;
        protected SpriteDataItemValidator otherSpriteDataItemValidator;
        protected AutoSortingCalculationData autoSortingCalculationData;
        protected int[] sortingResults;
        protected SortingCriterionType sortingCriterionType;

        public SortingCriterionType SortingCriterionType => sortingCriterionType;

        protected SortingCriterion(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
        }

        public float[] Sort(SortingComponent sortingComponent, SortingComponent otherSortingComponent,
            AutoSortingCalculationData autoSortingCalculationData)
        {
            this.autoSortingCalculationData = autoSortingCalculationData;
            sortingResults = new int[2];
            var weightedResult = new float[2];

            if (IsUsingSpriteData())
            {
                var isValidForSorting = ValidateSortingComponentsForSorting(sortingComponent, otherSortingComponent);
                if (!isValidForSorting)
                {
                    return weightedResult;
                }
            }

            InternalSort(sortingComponent, otherSortingComponent);

            for (var i = 0; i < sortingResults.Length; i++)
            {
                weightedResult[i] = sortingResults[i] * sortingCriterionData.priority;
            }

            // Debug.Log(GetType().Name + " sorted: [" + sortingResults[0] + "," + sortingResults[1] + "]");

            return weightedResult;
        }

        private bool ValidateSortingComponentsForSorting(SortingComponent sortingComponent,
            SortingComponent otherSortingComponent)
        {
            var spriteDataItemValidatorCache = SpriteDataItemValidatorCache.GetInstance();
            spriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(sortingComponent.SpriteRenderer);
            otherSpriteDataItemValidator =
                spriteDataItemValidatorCache.GetOrCreateValidator(otherSortingComponent.SpriteRenderer);

            if (spriteDataItemValidator == null || otherSpriteDataItemValidator == null)
            {
                return false;
            }

            var isContainingSpriteData = autoSortingCalculationData.spriteData.spriteDataDictionary.ContainsKey(
                spriteDataItemValidator
                    .AssetGuid);
            var isContainingOtherSpriteData =
                autoSortingCalculationData.spriteData.spriteDataDictionary.ContainsKey(spriteDataItemValidator
                    .AssetGuid);

            return isContainingSpriteData && isContainingOtherSpriteData;
        }

        protected abstract void InternalSort(SortingComponent sortingComponent,
            SortingComponent otherSortingComponent);

        public abstract bool IsUsingSpriteData();
    }
}