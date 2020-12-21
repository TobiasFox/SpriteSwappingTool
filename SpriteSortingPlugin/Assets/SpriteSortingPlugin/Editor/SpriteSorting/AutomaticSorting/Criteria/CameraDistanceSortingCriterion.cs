using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class CameraDistanceSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public CameraDistanceSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
            sortingCriterionType = DefaultSortingCriterionData.sortingCriterionType;
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            if (autoSortingCalculationData.cameraProjectionType == CameraProjectionType.Orthographic)
            {
                return;
            }

            var spriteRendererTransform = sortingComponent.SpriteRenderer.transform;
            var otherSpriteRendererTransform = otherSortingComponent.SpriteRenderer.transform;
            var cameraTransform = autoSortingCalculationData.cameraTransform;

            var perspectiveDistance = CalculatePerspectiveDistance(spriteRendererTransform, cameraTransform);
            var otherPerspectiveDistance = CalculatePerspectiveDistance(otherSpriteRendererTransform, cameraTransform);
            var isSortingComponentCloser = perspectiveDistance <= otherPerspectiveDistance;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                sortingResults[isSortingComponentCloser ? 0 : 1]++;
            }
            else
            {
                sortingResults[!isSortingComponentCloser ? 0 : 1]++;
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