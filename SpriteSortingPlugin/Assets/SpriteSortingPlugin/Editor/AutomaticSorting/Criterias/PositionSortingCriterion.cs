using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class PositionSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private PositionSortingCriterionData PositionSortingCriterionData =>
            (PositionSortingCriterionData) sortingCriterionData;

        public PositionSortingCriterion(PositionSortingCriterionData sortingCriterionData) : base(sortingCriterionData)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            if (autoSortingCalculationData.cameraProjectionType == CameraProjectionType.Orthographic)
            {
                return results;
            }

            var spriteRendererTransform = autoSortingComponent.spriteRenderer.transform;
            var otherSpriteRendererTransform = otherAutoSortingComponent.spriteRenderer.transform;
            var cameraTransform = autoSortingCalculationData.cameraTransform;

            var perspectiveDistance = CalculatePerspectiveDistance(spriteRendererTransform, cameraTransform);
            var otherPerspectiveDistance =
                CalculatePerspectiveDistance(otherSpriteRendererTransform, cameraTransform);
            var isAutoSortingComponentCloser = perspectiveDistance <= otherPerspectiveDistance;

            if (PositionSortingCriterionData.isFurtherAwaySpriteInForeground)
            {
                results[isAutoSortingComponentCloser ? 1 : 0]++;
            }
            else
            {
                results[!isAutoSortingComponentCloser ? 1 : 0]++;
            }

            return results;
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