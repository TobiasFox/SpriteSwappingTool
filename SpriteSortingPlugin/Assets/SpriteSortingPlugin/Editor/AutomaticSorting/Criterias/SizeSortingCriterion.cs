using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using SpriteSortingPlugin.OverlappingSprites;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
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

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            spriteRenderer = sortingComponent.spriteRenderer;
            otherSpriteRenderer = otherSortingComponent.spriteRenderer;
            spriteSortPoint = GetSpriteSortPoint(spriteRenderer);
            otherSpriteSortPoint = GetSpriteSortPoint(otherSpriteRenderer);

            AnalyzeSurface();
            AnalyzeSpriteSortPoint();
            AnalyzeEncapsulationBoundingBox();
        }

        private void AnalyzeSurface()
        {
            //surfaceArea
            var surfaceArea = CalculateSurfaceArea(spriteRenderer, spriteDataItemValidator);
            var otherSurfaceArea = CalculateSurfaceArea(otherSpriteRenderer, otherSpriteDataItemValidator);
            var isAutoSortingComponentIsLarger = surfaceArea >= otherSurfaceArea;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingComponentIsLarger ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentIsLarger ? 0 : 1]++;
            }

            //intersection area
            var polygon = GetOutlinePoints(spriteDataItemValidator, spriteRenderer);
            var otherPolygon = GetOutlinePoints(otherSpriteDataItemValidator, otherSpriteRenderer);
            if (polygonIntersectionAreaAnalyzer == null)
            {
                polygonIntersectionAreaAnalyzer = new PolygonIntersectionAreaAnalyzer();
            }

            var intersectionArea = polygonIntersectionAreaAnalyzer.CalculateIntersectionArea(polygon, otherPolygon);

            var autoSortingSurfaceAreaWithoutIntersectionArea = surfaceArea - intersectionArea;
            var otherAutoSortingSurfaceAreaWithoutIntersectionArea = otherSurfaceArea - intersectionArea;
            var isAutoSortingSurfaceAreaWithoutIntersectionAreaLarger = autoSortingSurfaceAreaWithoutIntersectionArea >=
                                                                        otherAutoSortingSurfaceAreaWithoutIntersectionArea;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isAutoSortingSurfaceAreaWithoutIntersectionAreaLarger ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingSurfaceAreaWithoutIntersectionAreaLarger ? 0 : 1]++;
            }
        }

        private void AnalyzeEncapsulationBoundingBox()
        {
            //Distance to encapsulated Bounding Box
            var enclosingBoundingBox = CreateEnclosingBoundingBox(spriteRenderer.bounds, otherSpriteRenderer.bounds);
            var distanceFromSpriteRenderer = Vector2.Distance(enclosingBoundingBox.center, spriteSortPoint);
            var otherDistanceFromSpriteRenderer = Vector2.Distance(enclosingBoundingBox.center, otherSpriteSortPoint);

            var isSpriteRendererCloser = distanceFromSpriteRenderer <= otherDistanceFromSpriteRenderer;

            if (SizeSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isSpriteRendererCloser ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isSpriteRendererCloser ? 0 : 1]++;
            }
        }

        private void AnalyzeSpriteSortPoint()
        {
            //Sprite sort point
            if (ContainsSpriteSortPoint(spriteSortPoint, otherSpriteRenderer, spriteDataItemValidator))
            {
                sortingResults[0]++;
            }

            if (ContainsSpriteSortPoint(otherSpriteSortPoint, spriteRenderer, otherSpriteDataItemValidator))
            {
                sortingResults[1]++;
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

        private Vector2[] GetOutlinePoints(SpriteDataItemValidator validator, SpriteRenderer spriteRenderer)
        {
            Vector2[] outlinePoints;

            switch (validator.GetValidOutlinePrecision(autoSortingCalculationData.outlinePrecision))
            {
                case OutlinePrecision.AxisAlignedBoundingBox:

                    outlinePoints = new Vector2[4];
                    var min = spriteRenderer.bounds.min;
                    var max = spriteRenderer.bounds.max;

                    outlinePoints[0] = new Vector2(min.x, min.y);
                    outlinePoints[1] = new Vector2(max.x, min.y);
                    outlinePoints[2] = new Vector2(max.x, max.y);
                    outlinePoints[3] = new Vector2(min.x, max.y);

                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    var oobb = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .objectOrientedBoundingBox;
                    oobb.UpdateBox(spriteRenderer.transform);

                    outlinePoints = oobb.Points;

                    break;
                case OutlinePrecision.PixelPerfect:

                    var colliderPoints = autoSortingCalculationData.spriteData.spriteDataDictionary[validator.AssetGuid]
                        .outlinePoints;
                    outlinePoints = new Vector2[colliderPoints.Length];

                    for (var i = 0; i < outlinePoints.Length; i++)
                    {
                        var transformedPoint = spriteRenderer.transform.TransformPoint(colliderPoints[i]);
                        outlinePoints[i] = transformedPoint;
                    }

                    break;
                default:
                    outlinePoints = new Vector2[0];
                    break;
            }

            return outlinePoints;
        }

        public override bool IsUsingSpriteData()
        {
            return true;
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

        private float CalculateSurfaceArea(SpriteRenderer currentSpriteRenderer, SpriteDataItemValidator validator)
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
                    returnValue = oobb.GetSurfaceArea();

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