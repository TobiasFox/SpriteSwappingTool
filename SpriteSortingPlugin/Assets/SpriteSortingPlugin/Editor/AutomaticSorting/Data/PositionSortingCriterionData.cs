namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class PositionSortingCriterionData : SortingCriterionData
    {
        public bool isFurtherAwaySpriteInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<PositionSortingCriterionData>();
            CopyDataTo(clone);
            clone.isFurtherAwaySpriteInForeground = isFurtherAwaySpriteInForeground;
            return clone;
        }
    }
}