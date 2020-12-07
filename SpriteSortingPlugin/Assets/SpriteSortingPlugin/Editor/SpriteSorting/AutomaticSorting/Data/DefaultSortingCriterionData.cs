using System;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    [Serializable]
    public class DefaultSortingCriterionData : SortingCriterionData
    {
        public bool isSortingInForeground;

        public override object Clone()
        {
            var clone = new DefaultSortingCriterionData();
            CopyDataTo(clone);
            clone.isSortingInForeground = isSortingInForeground;
            return clone;
        }
    }
}