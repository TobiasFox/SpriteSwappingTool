﻿#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

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
            sortingCriterionType = DefaultSortingCriterionData.sortingCriterionType;
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