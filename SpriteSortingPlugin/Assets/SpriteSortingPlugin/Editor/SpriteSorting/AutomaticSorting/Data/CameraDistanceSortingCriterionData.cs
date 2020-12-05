namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    public class CameraDistanceSortingCriterionData : SortingCriterionData
    {
        public bool isCloserSpriteInForeground = true;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<CameraDistanceSortingCriterionData>();
            CopyDataTo(clone);
            clone.isCloserSpriteInForeground = isCloserSpriteInForeground;
            return clone;
        }
    }
}