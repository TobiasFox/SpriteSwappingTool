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

            var spriteRendererTransform = autoSortingComponent.spriteRenderer.transform;
            var otherSpriteRendererTransform = otherAutoSortingComponent.spriteRenderer.transform;
            var isAutoSortingComponentCloser = false;
            var cameraTransform = autoSortingCalculationData.cameraTransform;
            switch (autoSortingCalculationData.cameraProjectionType)
            {
                case CameraProjectionType.Perspective:

                    var perspectiveDistance = CalculatePerspectiveDistance(spriteRendererTransform, cameraTransform);
                    var otherPerspectiveDistance =
                        CalculatePerspectiveDistance(otherSpriteRendererTransform, cameraTransform);
                    isAutoSortingComponentCloser = perspectiveDistance <= otherPerspectiveDistance;

                    break;
                case CameraProjectionType.Orthographic:
                    var orthographicDistance = CalculateOrthographicDistance(spriteRendererTransform, cameraTransform);
                    var otherOrthographicDistance =
                        CalculateOrthographicDistance(otherSpriteRendererTransform, cameraTransform);
                    isAutoSortingComponentCloser = orthographicDistance <= otherOrthographicDistance;

                    break;
                case CameraProjectionType.Both:
                    break;
            }

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

        private float CalculatePerspectiveDistance(Transform spriteRendererTransform, Transform cameraTransform)
        {
            var distance = spriteRendererTransform.position - cameraTransform.position;
            // distance = distance * -0.5f + transform.position;

            return distance.magnitude;
        }

        private float CalculateOrthographicDistance(Transform spriteRendererTransform, Transform cameraTransform)
        {
            var positionOnCameraPlane =
                new Vector3(spriteRendererTransform.position.x, spriteRendererTransform.position.y,
                    cameraTransform.position.z);
            var distance = spriteRendererTransform.position - positionOnCameraPlane;
            // distance = distance * -0.5f + transform.position;

            return distance.magnitude;
        }
    }
}