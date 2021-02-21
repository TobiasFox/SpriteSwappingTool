#region license

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

using SpriteSwappingPlugin.SortingGeneration.Criteria;
using SpriteSwappingPlugin.SortingGeneration.Data;
using SpriteSwappingPlugin.SpriteSwappingDetector.UI.SortingGeneration;

namespace SpriteSwappingPlugin.SortingGeneration
{
    public static class SortingCriteriaComponentFactory
    {
        public static SortingCriteriaComponent CreateSortingCriteriaComponent(SortingCriterionType type)
        {
            var sortingCriteriaComponent = new SortingCriteriaComponent();

            switch (type)
            {
                case SortingCriterionType.Size:
                    CreateSizeDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.CameraDistance:
                    CreateCameraDistanceDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Resolution:
                    CreateResolutionDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Sharpness:
                    CreateSharpnessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Lightness:
                    CreateLightnessDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.PrimaryColor:
                    CreatePrimaryColorDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.Containment:
                    CreateContainmentDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.IntersectionArea:
                    CreateIntersectionAreaDataAndCriterion(ref sortingCriteriaComponent);
                    break;
                case SortingCriterionType.SortPoint:
                    CreateSortPointDataAndCriterion(ref sortingCriteriaComponent);
                    break;
            }

            var criterionDataBaseEditor =
                SortingCriterionDataUIRepresentationFactory.CreateUIRepresentation(sortingCriteriaComponent
                    .sortingCriterionData);
            criterionDataBaseEditor.Initialize(sortingCriteriaComponent.sortingCriterionData);
            sortingCriteriaComponent.criterionDataBaseUIRepresentation = criterionDataBaseEditor;

            return sortingCriteriaComponent;
        }

        private static void CreateSizeDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Size
            };
            sortingCriteriaComponent.sortingCriterion = new SizeSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateIntersectionAreaDataAndCriterion(
            ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.IntersectionArea, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new IntersectionAreaCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSortPointDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.SortPoint, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new SpriteSortPointSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateCameraDistanceDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.CameraDistance, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new CameraDistanceSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateResolutionDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Resolution, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new ResolutionSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateSharpnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Sharpness, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new SharpnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreatePrimaryColorDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new PrimaryColorSortingCriterionData();
            sortingCriteriaComponent.sortingCriterion = new PrimaryColorSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateLightnessDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new DefaultSortingCriterionData
            {
                sortingCriterionType = SortingCriterionType.Lightness, isSortingInForeground = true
            };
            sortingCriteriaComponent.sortingCriterion = new BrightnessSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }

        private static void CreateContainmentDataAndCriterion(ref SortingCriteriaComponent sortingCriteriaComponent)
        {
            var sortingCriterionData = new ContainmentSortingCriterionData();
            sortingCriteriaComponent.sortingCriterion = new ContainmentSortingCriterion(sortingCriterionData);
            sortingCriteriaComponent.sortingCriterionData = sortingCriterionData;
        }
    }
}