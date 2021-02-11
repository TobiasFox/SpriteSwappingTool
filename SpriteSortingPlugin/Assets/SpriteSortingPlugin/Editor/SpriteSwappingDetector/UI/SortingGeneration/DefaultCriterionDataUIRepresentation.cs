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

using SpriteSortingPlugin.SortingGeneration;
using SpriteSortingPlugin.SortingGeneration.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSwappingDetector.UI.SortingGeneration
{
    public class DefaultCriterionDataUIRepresentation : CriterionDataBaseUIRepresentation
    {
        private string foregroundSortingName;
        private string foregroundSortingTooltip;

        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = GetSortingCriteriaTitle(DefaultSortingCriterionData.sortingCriterionType);
            tooltip = GetSortingCriterionToolTip(DefaultSortingCriterionData.sortingCriterionType);
            foregroundSortingName =
                GetSortingCriteriaForegroundSortingName(DefaultSortingCriterionData.sortingCriterionType);
            foregroundSortingTooltip =
                GetSortingCriteriaForegroundSortingTooltip(DefaultSortingCriterionData.sortingCriterionType);
        }

        protected override void OnInspectorGuiInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(CalculateIndentSpace);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent(foregroundSortingName, foregroundSortingTooltip));
                        GUILayout.FlexibleSpace();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DefaultSortingCriterionData.isSortingInForeground = GUILayout.Toggle(
                            DefaultSortingCriterionData.isSortingInForeground, "Foreground",
                            Styling.ButtonStyle);

                        DefaultSortingCriterionData.isSortingInForeground = !GUILayout.Toggle(
                            !DefaultSortingCriterionData.isSortingInForeground, "Background",
                            Styling.ButtonStyle);
                    }
                }
            }
        }

        private string GetSortingCriteriaTitle(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = "Size";
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = "Intersection Area";
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = "Sprite Sort Point";
                    break;
                case SortingCriterionType.Resolution:
                    returnString = "Sprite Resolution";
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = "Sprite Sharpness";
                    break;
                case SortingCriterionType.Lightness:
                    returnString = "Perceived Lightness";
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = "Camera distance difference";
                    break;
            }

            return returnString;
        }

        private string GetSortingCriterionToolTip(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = UITooltipConstants.SizeTooltip;
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = UITooltipConstants.IntersectionAreaTooltip;
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = UITooltipConstants.SpriteSortPointTooltip;
                    break;
                case SortingCriterionType.Resolution:
                    returnString = UITooltipConstants.ResolutionTooltip;
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = UITooltipConstants.SharpnessTooltip;
                    break;
                case SortingCriterionType.Lightness:
                    returnString = UITooltipConstants.PerceivedLightnessTooltip;
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = UITooltipConstants.CameraDistanceTooltip;
                    break;
            }

            return returnString;
        }

        private string GetSortingCriteriaForegroundSortingName(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = "Sort large Sprite in";
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = "Sort Sprite with smaller \"area-intersection area\" ratio in";
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = "Sort Sprite with overlapping Sprite Sort Point in";
                    break;
                case SortingCriterionType.Resolution:
                    returnString = "Sort Sprite with higher resolution in";
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = "Sort sharper Sprite in";
                    break;
                case SortingCriterionType.Lightness:
                    returnString = "Sort lighter Sprite in";
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = "Sort closer Sprite in";
                    break;
            }

            return returnString;
        }

        private string GetSortingCriteriaForegroundSortingTooltip(SortingCriterionType sortingCriterionType)
        {
            var returnString = "";
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Size:
                    returnString = UITooltipConstants.SizeForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.IntersectionArea:
                    returnString = UITooltipConstants.IntersectionAreaForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.SortPoint:
                    returnString = UITooltipConstants.SpriteSortPointForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.Resolution:
                    returnString = UITooltipConstants.ResolutionForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.Sharpness:
                    returnString = UITooltipConstants.SharpnessForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.Lightness:
                    returnString = UITooltipConstants.PerceivedLightnessForegroundSpriteTooltip;
                    break;
                case SortingCriterionType.CameraDistance:
                    returnString = UITooltipConstants.CameraDistanceForegroundSpriteTooltip;
                    break;
            }

            return returnString;
        }
    }
}