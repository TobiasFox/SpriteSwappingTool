using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public class SizeSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private SizeSortingCriterionData SizeSortingCriterionData => (SizeSortingCriterionData) sortingCriterionData;

        public SizeSortingCriterion(SizeSortingCriterionData sortingCriterionData) : base(sortingCriterionData, true)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent, SpriteData spriteData)
        {
            return null;
        }
    }
}