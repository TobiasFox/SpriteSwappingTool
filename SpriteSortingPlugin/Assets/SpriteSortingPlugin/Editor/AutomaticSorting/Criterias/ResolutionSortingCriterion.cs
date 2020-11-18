﻿using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Criterias
{
    public class ResolutionSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public ResolutionSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override int[] InternalSort(AutoSortingComponent autoSortingComponent,
            AutoSortingComponent otherAutoSortingComponent)
        {
            var results = new int[2];

            var spriteResolution = CalculateSpriteResolution(autoSortingComponent.spriteRenderer);
            var otherSpriteResolution = CalculateSpriteResolution(otherAutoSortingComponent.spriteRenderer);
            var hasAutoSortingComponentHigherResolution = spriteResolution >= otherSpriteResolution;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                results[hasAutoSortingComponentHigherResolution ? 0 : 1]++;
            }
            else
            {
                results[!hasAutoSortingComponentHigherResolution ? 0 : 1]++;
            }

            return results;
        }

        public override bool IsUsingSpriteData()
        {
            return false;
        }

        private float CalculateSpriteResolution(SpriteRenderer spriteRenderer)
        {
            //rotate sprite to identity to use correct bounds
            var spriteRendererTransform = spriteRenderer.transform;
            var previousRotation = spriteRendererTransform.rotation;
            spriteRendererTransform.rotation = Quaternion.identity;
            var boundsSize = spriteRenderer.bounds.size;
            spriteRendererTransform.rotation = previousRotation;


            var pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
            var currentSpriteWidth = boundsSize.x * pixelsPerUnit;
            var currentSpriteHeight = boundsSize.y * pixelsPerUnit;

            return currentSpriteHeight * currentSpriteWidth;
        }
    }
}