namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class CameraDistanceSortingCriterionData : SortingCriterionData
    {
        public bool isFurtherAwaySpriteInForeground;

        public override SortingCriterionData Copy()
        {
            var clone = CreateInstance<CameraDistanceSortingCriterionData>();
            CopyDataTo(clone);
            clone.isFurtherAwaySpriteInForeground = isFurtherAwaySpriteInForeground;
            return clone;
        }
    }
}