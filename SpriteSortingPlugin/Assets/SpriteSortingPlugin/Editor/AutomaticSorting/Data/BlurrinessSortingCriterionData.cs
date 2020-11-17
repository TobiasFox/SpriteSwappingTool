namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class BlurrinessSortingCriterionData : SortingCriterionData
    {
        public bool isMoreBlurrySpriteInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<BlurrinessSortingCriterionData>();
            CopyDataTo(clone);
            clone.isMoreBlurrySpriteInForeground = isMoreBlurrySpriteInForeground;
            return clone;
        }
    }
}