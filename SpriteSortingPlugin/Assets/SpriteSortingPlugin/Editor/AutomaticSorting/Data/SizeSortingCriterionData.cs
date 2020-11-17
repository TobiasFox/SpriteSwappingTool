namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class SizeSortingCriterionData : SortingCriterionData
    {
        public bool isLargeSpriteInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<SizeSortingCriterionData>();
            CopyDataTo(clone);
            clone.isLargeSpriteInForeground = isLargeSpriteInForeground;
            return clone;
        }
    }
}