using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class CameraDistanceSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private CameraDistanceSortingCriterionData CameraDistanceSortingCriterionData =>
            (CameraDistanceSortingCriterionData) sortingCriterionData;

        public CameraDistanceSortingCriterion(CameraDistanceSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            if (autoSortingCalculationData.cameraProjectionType == CameraProjectionType.Orthographic)
            {
                return;
            }

            var spriteRendererTransform = sortingComponent.spriteRenderer.transform;
            var otherSpriteRendererTransform = otherSortingComponent.spriteRenderer.transform;
            var cameraTransform = autoSortingCalculationData.cameraTransform;

            var perspectiveDistance = CalculatePerspectiveDistance(spriteRendererTransform, cameraTransform);
            var otherPerspectiveDistance = CalculatePerspectiveDistance(otherSpriteRendererTransform, cameraTransform);
            var isAutoSortingComponentCloser = perspectiveDistance <= otherPerspectiveDistance;

            if (CameraDistanceSortingCriterionData.isFurtherAwaySpriteInForeground)
            {
                sortingResults[isAutoSortingComponentCloser ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentCloser ? 0 : 1]++;
            }
        }

        public override bool IsUsingSpriteData()
        {
            return false;
        }

        private float CalculatePerspectiveDistance(Transform spriteRendererTransform, Transform cameraTransform)
        {
            var distance = spriteRendererTransform.position - cameraTransform.position;
            return distance.magnitude;
        }
    }
}