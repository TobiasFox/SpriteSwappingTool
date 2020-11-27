namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class LightnessSortingCriterionData : SortingCriterionData
    {
        public bool isLighterSpriteIsInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<LightnessSortingCriterionData>();
            CopyDataTo(clone);
            clone.isLighterSpriteIsInForeground = isLighterSpriteIsInForeground;
            return clone;
        }
    }
}