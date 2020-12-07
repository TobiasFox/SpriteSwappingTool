using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria
{
    public class ResolutionSortingCriterion : SortingCriterion<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        public ResolutionSortingCriterion(DefaultSortingCriterionData sortingCriterionData) : base(
            sortingCriterionData)
        {
        }

        protected override void InternalSort(SortingComponent sortingComponent, SortingComponent otherSortingComponent)
        {
            // var spriteResolution = CalculateCurrentSpriteResolution(autoSortingComponent.OriginSpriteRenderer);
            // var otherSpriteResolution = CalculateCurrentSpriteResolution(otherAutoSortingComponent.OriginSpriteRenderer);
            var spriteResolution = CalculatePixelResolution(sortingComponent.SpriteRenderer);
            var otherSpriteResolution = CalculatePixelResolution(otherSortingComponent.SpriteRenderer);

            var hasAutoSortingComponentHigherResolution = spriteResolution >= otherSpriteResolution;

            if (DefaultSortingCriterionData.isSortingInForeground)
            {
                sortingResults[hasAutoSortingComponentHigherResolution ? 0 : 1]++;
            }
            else
            {
                sortingResults[!hasAutoSortingComponentHigherResolution ? 0 : 1]++;
            }
        }

        public override bool IsUsingSpriteData()
        {
            return false;
        }

        private float CalculatePixelResolution(SpriteRenderer spriteRenderer)
        {
            var spriteTexture = spriteRenderer.sprite.texture;
            return spriteTexture.width * spriteTexture.height;
        }

        private float CalculateCurrentSpriteResolution(SpriteRenderer spriteRenderer)
        {
            //TODO only width x height in pixels?
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