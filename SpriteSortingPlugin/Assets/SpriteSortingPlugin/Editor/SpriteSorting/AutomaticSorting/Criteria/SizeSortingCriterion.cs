using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class SizeSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private PolygonIntersectionAreaAnalyzer polygonIntersectionAreaAnalyzer;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer otherSpriteRenderer;
        private Vector2 spriteSortPoint;
        private Vector2 otherSpriteSortPoint;

        private DefaultSortingCriterionData SizeSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public SizeSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(sortingCriterionData)
        {
        }

        public override bool IsUsingSpriteData()
        {
            return true;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            spriteRenderer = sortingComponent.SpriteRenderer;
            otherSpriteRenderer = otherSortingComponent.SpriteRenderer;

            AnalyzeArea();
        }

        private void AnalyzeArea()
        {
            var area = CalculateArea(spriteRenderer, spriteDataItemValidator);
            var otherArea = CalculateArea(otherSpriteRenderer, otherSpriteDataItemValidator);
            var isAutoSortingComponentIsLarger = area >= otherArea;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingComponentIsLarger ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentIsLarger ? 0 : 1]++;
            }
        }

        private void DrawBoundingBox(Bounds enclosingBoundingBox)
        {
            var points = new Vector2[4];
            points[0] = new Vector2(enclosingBoundingBox.min.x, enclosingBoundingBox.min.y);
            points[1] = new Vector2(enclosingBoundingBox.min.x, enclosingBoundingBox.max.y);
            points[2] = new Vector2(enclosingBoundingBox.max.x, enclosingBoundingBox.max.y);
            points[3] = new Vector2(enclosingBoundingBox.max.x, enclosingBoundingBox.min.y);

            for (int i = 0; i < points.Length; i++)
            {
                Debug.DrawLine(points[i], points[(i + 1) % points.Length], Color.green, 2);
            }
        }

        private float CalculateArea(SpriteRenderer currentSpriteRenderer, SpriteDataItemValidator validator)
        {
            var returnValue = 0f;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    var bounds = currentSpriteRenderer.bounds;
                    returnValue = bounds.size.x * bounds.size.y;
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(currentSpriteRenderer.transform);
                    returnValue = oobb.GetArea();

                    break;
                case OutlinePrecision.PixelPerfect:
                    var spriteDataItem =
                        autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid];

                    returnValue = spriteDataItem.CalculatePolygonArea(currentSpriteRenderer.transform);
                    break;
            }

            return returnValue;
        }
    }
}