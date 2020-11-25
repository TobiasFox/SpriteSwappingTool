﻿using SpriteSortingPlugin.AutomaticSorting.Data;
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

        protected override void InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            if (autoSortingCalculationData.cameraProjectionType == CameraProjectionType.Orthographic)
            {
                return;
            }

            var spriteRendererTransform = autoSortingComponent.OriginSpriteRenderer.transform;
            var otherSpriteRendererTransform = otherAutoSortingComponent.OriginSpriteRenderer.transform;
            var cameraTransform = autoSortingCalculationData.cameraTransform;

            var perspectiveDistance = CalculatePerspectiveDistance(spriteRendererTransform, cameraTransform);
            var otherPerspectiveDistance =
                CalculatePerspectiveDistance(otherSpriteRendererTransform, cameraTransform);
            var isAutoSortingComponentCloser = perspectiveDistance <= otherPerspectiveDistance;

            if (PositionSortingCriterionData.isFurtherAwaySpriteInForeground)
            {
                sortingResults[isAutoSortingComponentCloser ? 1 : 0]++;
            }
            else
            {
                sortingResults[!isAutoSortingComponentCloser ? 1 : 0]++;
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