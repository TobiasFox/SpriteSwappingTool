using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using SpriteSortingPlugin.OverlappingSprites;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class SizeSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData SizeSortingCriterionData => (DefaultSortingCriterionData) sortingCriterionData;

        public SizeSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(sortingCriterionData)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            var spriteRenderer = autoSortingComponent.OriginSpriteRenderer;
            var otherSpriteRenderer = otherAutoSortingComponent.OriginSpriteRenderer;
            var spriteSortPoint = GetSpriteSortPoint(spriteRenderer);
            var otherSpriteSortPoint = GetSpriteSortPoint(otherSpriteRenderer);

            //surfaceArea
            var surfaceArea = CalculateSurfaceArea(spriteRenderer, spriteDataItemValidator);
            var otherSurfaceArea = CalculateSurfaceArea(otherSpriteRenderer, otherSpriteDataItemValidator);
            var isAutoSortingComponentIsLarger = surfaceArea >= otherSurfaceArea;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                results[isAutoSortingComponentIsLarger ? 0 : 1]++;
            }
            else
            {
                results[!isAutoSortingComponentIsLarger ? 0 : 1]++;
            }


            //Sprite sort point
            if (ContainsSpriteSortPoint(spriteSortPoint, otherSpriteRenderer, spriteDataItemValidator))
            {
                results[0]++;
            }

            if (ContainsSpriteSortPoint(otherSpriteSortPoint, spriteRenderer, otherSpriteDataItemValidator))
            {
                results[1]++;
            }

            //Distance to encapsulated Bounding Box
            var bounds = spriteRenderer.bounds;
            var enclosingBoundingBox = new Bounds(bounds.center, bounds.size);
            enclosingBoundingBox.Encapsulate(otherSpriteRenderer.bounds);
            enclosingBoundingBox.SetMinMax(new Vector3(enclosingBoundingBox.min.x, enclosingBoundingBox.min.y, 0),
                new Vector3(enclosingBoundingBox.max.x, enclosingBoundingBox.max.y, 0));

            var distanceFromSpriteRenderer = Vector2.Distance(enclosingBoundingBox.center, spriteSortPoint);
            var otherDistanceFromSpriteRenderer = Vector2.Distance(enclosingBoundingBox.center, otherSpriteSortPoint);

            var isSpriteRendererCloser = distanceFromSpriteRenderer <= otherDistanceFromSpriteRenderer;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                results[isSpriteRendererCloser ? 0 : 1]++;
            }
            else
            {
                results[!isSpriteRendererCloser ? 0 : 1]++;
            }

            return results;
        }

        public override bool IsUsingSpriteData()
        {
            return false;
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

        private Vector2 GetSpriteSortPoint(SpriteRenderer spriteRenderer)
        {
            return spriteRenderer.spriteSortPoint == SpriteSortPoint.Center
                ? spriteRenderer.bounds.center
                : spriteRenderer.transform.position;
        }

        private bool ContainsSpriteSortPoint(Vector2 spriteSortPoint, SpriteRenderer otherSpriteRenderer,
            SpriteDataItemValidator validator)
        {
            var returnBool = false;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    returnBool = otherSpriteRenderer.bounds.Contains(spriteSortPoint);
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:

                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(otherSpriteRenderer.transform);
                    returnBool = oobb.Contains(spriteSortPoint);

                    break;
                case OutlinePrecision.PixelPerfect:

                    var spriteDataItem = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid];
                    var polygonColliderCacher = PolygonColliderCacher.GetInstance();
                    var polygonCollider = polygonColliderCacher.GetCachedColliderOrCreateNewCollider(
                        validator.AssetGuid, spriteDataItem, otherSpriteRenderer.transform);

                    returnBool = polygonCollider.OverlapPoint(spriteSortPoint);

                    polygonColliderCacher.DisableCachedCollider(validator.AssetGuid, polygonCollider.GetInstanceID());
                    break;
            }

            return returnBool;
        }

        private float CalculateSurfaceArea(SpriteRenderer spriteRenderer, SpriteDataItemValidator validator)
        {
            var returnValue = 0f;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    var bounds = spriteRenderer.bounds;
                    returnValue = bounds.size.x * bounds.size.y;
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(spriteRenderer.transform);
                    returnValue = oobb.GetSurfaceArea();

                    break;
                case OutlinePrecision.PixelPerfect:
                    var spriteDataItem = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid];

                    returnValue = spriteDataItem.CalculatePolygonArea(spriteRenderer.transform);
                    break;
            }

            return returnValue;
        }
    }
}