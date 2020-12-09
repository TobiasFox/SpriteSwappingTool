using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class SpriteSortPointSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private PolygonIntersectionAreaAnalyzer polygonIntersectionAreaAnalyzer;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer otherSpriteRenderer;
        private Vector2 spriteSortPoint;
        private Vector2 otherSpriteSortPoint;

        private DefaultSortingCriterionData SpriteSortCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public SpriteSortPointSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
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
            spriteSortPoint = GetSpriteSortPoint(spriteRenderer);
            otherSpriteSortPoint = GetSpriteSortPoint(otherSpriteRenderer);

            AnalyzeSpriteSortPoint();
            AnalyzeEncapsulationBoundingBox();
        }

        private void AnalyzeSpriteSortPoint()
        {
            if (ContainsSpriteSortPoint(spriteSortPoint, otherSpriteRenderer, spriteDataItemValidator))
            {
                sortingResults[SpriteSortCriterionData.isSortingInForeground ? 0 : 1]++;
            }

            if (ContainsSpriteSortPoint(otherSpriteSortPoint, spriteRenderer, otherSpriteDataItemValidator))
            {
                sortingResults[SpriteSortCriterionData.isSortingInForeground ? 1 : 0]++;
            }
        }

        private void AnalyzeEncapsulationBoundingBox()
        {
            var enclosingBoundingBox = CreateEnclosingBoundingBox(spriteRenderer.bounds, otherSpriteRenderer.bounds);
            var distanceFromSpriteRenderer = Vector2.Distance(enclosingBoundingBox.center, spriteSortPoint);
            var otherDistanceFromSpriteRenderer = Vector2.Distance(enclosingBoundingBox.center, otherSpriteSortPoint);

            var isSpriteRendererCloser = distanceFromSpriteRenderer <= otherDistanceFromSpriteRenderer;

            if (SpriteSortCriterionData.isSortingInForeground)
            {
                sortingResults[isSpriteRendererCloser ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isSpriteRendererCloser ? 0 : 1]++;
            }
        }

        private static Bounds CreateEnclosingBoundingBox(Bounds bounds, Bounds otherBounds)
        {
            var enclosingBoundingBox = new Bounds(bounds.center, bounds.size);
            enclosingBoundingBox.Encapsulate(otherBounds);
            enclosingBoundingBox.SetMinMax(new Vector3(enclosingBoundingBox.min.x, enclosingBoundingBox.min.y, 0),
                new Vector3(enclosingBoundingBox.max.x, enclosingBoundingBox.max.y, 0));

            return enclosingBoundingBox;
        }

        private Vector2 GetSpriteSortPoint(SpriteRenderer currentSpriteRenderer)
        {
            return currentSpriteRenderer.spriteSortPoint == SpriteSortPoint.Center
                ? currentSpriteRenderer.bounds.center
                : currentSpriteRenderer.transform.position;
        }

        private bool ContainsSpriteSortPoint(Vector2 currentSpriteSortPoint, SpriteRenderer spriteRendererToCheck,
            SpriteDataItemValidator validator)
        {
            var returnBool = false;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    returnBool = spriteRendererToCheck.bounds.Contains(currentSpriteSortPoint);
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:

                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(spriteRendererToCheck.transform);
                    returnBool = oobb.Contains(currentSpriteSortPoint);

                    break;
                case OutlinePrecision.PixelPerfect:

                    var spriteDataItem =
                        autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid];
                    var polygonColliderCacher = PolygonColliderCacher.GetInstance();
                    var polygonCollider = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        validator.AssetGuid, spriteDataItem, spriteRendererToCheck.transform);

                    returnBool = polygonCollider.OverlapPoint(currentSpriteSortPoint);

                    polygonColliderCacher.DisableCachedCollider(validator.AssetGuid, polygonCollider.GetInstanceID());
                    break;
            }

            return returnBool;
        }
    }
}