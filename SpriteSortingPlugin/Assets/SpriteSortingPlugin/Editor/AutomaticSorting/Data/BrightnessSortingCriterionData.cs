namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class BrightnessSortingCriterionData : SortingCriterionData
    {
        public bool isUsingSpriteColor = true;
        public bool isUsingSpriteRendererColor;
        public bool isLighterSpriteIsInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<BrightnessSortingCriterionData>();
            CopyDataTo(clone);
            clone.isUsingSpriteColor = isUsingSpriteColor;
            clone.isUsingSpriteRendererColor = isUsingSpriteRendererColor;
            clone.isLighterSpriteIsInForeground = isLighterSpriteIsInForeground;
            return clone;
        }
    }
}