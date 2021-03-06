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

namespace SpriteSwappingPlugin.UI
{
    public static class UITooltipConstants
    {
        //--sorting criteria
        public const string ContainmentCheckingAlphaTooltip =
            "Completely enclosed SpriteRenderer will be sorted in the background. Consider comparing their transparency by enable this option.\n" +
            "When enabled, the average transparency of the larger Sprite will be compared to the alpha threshold.";

        public const string ContainmentAlphaThresholdTooltip =
            "Specifies the alpha threshold up to which a SpriteRenderer should be rendered in the foreground even though it is completely enclosed by another SpriteRenderer\n" +
            "Range: 0.0 - 1.0";

        public const string ContainmentEncapsulatedSpriteInForegroundTooltip =
            "Control, whether completely enclosed SpriteRenderer will be sorted in the foreground or background. When choosing the background, the contained Sprite may be completely hidden.";

        public const string ContainmentTooltip =
            "Compares if SpriteRenderers are completely enclosed by other SpriteRenderers.\n" +
            "When analyzing the alpha value, a " + nameof(SpriteData) + " asset is required.";

        public const string CameraDistanceTooltip =
            "Compares the distance to the camera. Will be ignored when using orthographic Transparency Sort mode or Default Transparency Sort mode and orthographic camera project.";

        public const string CameraDistanceForegroundSpriteTooltip =
            "Control, whether SpriteRenderers with a shorter distance to the camera will be sorted in the foreground or background.";

        public const string PrimaryColorTooltip =
            "Compares the primary color of given SpriteRenderers and sorts SpriteRenderer in the foreground which are closer to the foreground color.";

        public const string PrimaryColorChannelsTooltip =
            "Specify which color channel should be used. The alpha channel is ignored.";

        public const string PrimaryColorForegroundColorTooltip =
            "Specify the color of SpriteRenderers, which will be sorted in the foreground.";

        public const string PrimaryColorBackgroundColorTooltip =
            "Specify the color of SpriteRenderers, which will be sorted in the background.";

        public const string SizeTooltip =
            "Compares the size of SpriteRenderers.\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string SizeForegroundSpriteTooltip =
            "Control, whether large Sprites will be sorted in the foreground or background.";

        public const string IntersectionAreaTooltip =
            "Calculates the intersection area of overlapping Sprites and sets these intersection areas in relation to the areas of these overlapping Sprites.\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string IntersectionAreaForegroundSpriteTooltip =
            "Control, whether Sprites with a smaller ratio of their own area to the intersection area will be sorted in the foreground or background.";

        public const string SpriteSortPointTooltip =
            "Compares the Sort Points of Sprites by testing if they overlap another Sprite.\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string SpriteSortPointForegroundSpriteTooltip =
            "Control, whether SpriteRenderers will be sorted in the foreground or background if a Sprite's Sort Point overlaps another Sprite.";

        public const string ResolutionTooltip = "Compares Sprites resolutions in pixels.";

        public const string ResolutionForegroundSpriteTooltip =
            "Control, whether Sprites with higher resolution will be sorted in the foreground or background.";

        public const string SharpnessTooltip =
            "Compares the sharpness of Sprites.\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string SharpnessForegroundSpriteTooltip =
            "Control, whether sharper Sprites will be sorted in the foreground or background.";

        public const string PerceivedLightnessTooltip = "Compares only the perceived lightness of Sprites.\n" +
                                                        "Requires a " + nameof(SpriteData) + " asset.";

        public const string PerceivedLightnessForegroundSpriteTooltip =
            "Control, whether lighter Sprites will be sorted in the foreground or background.";

        public const string SortingCriteriaWeightTooltip =
            "Controls the weight of this sorting criterion. The higher the value, the more affects this criterion the resulting order of Sprites.\n" +
            "Minimum: 0.0";

        public const string SortingCriteriaContainmentWeightTooltip =
            "The containment criterion is preferred over all other criteria as the calculation of the order of Sprites differs when this criterion is enabled.";

        //--SpriteDataEditorWindow
        public const string SpriteDataSharpnessTooltip =
            "This value indicates how sharp a Sprite is. The higher the value, the sharper the Sprite.\n" +
            "Minimum: 0.0 ";

        public const string SpriteDataPerceivedLightnessTooltip =
            "This value represents the average perceived lightness of a Sprite, where 0 means dark and 100.0 means bright.\n" +
            "Range: 0.0 - 100.0";

        public const string SpriteDataAverageAlphaTooltip =
            "This value represents the average alpha of a Sprite ignoring completely transparent pixels.";

        public const string SpriteDataPrimaryColorTooltip =
            "This is the primary or average Color of a Sprite ignoring completely transparent pixels.";

        public const string SpriteDataResolutionTooltip = "The Sprite's resolution in pixel.";

        public const string SpriteDataAnalyzeAllTooltip =
            SpriteDataAnalyzeTooltipPrefix + "all visible Sprites in opened scenes " + SpriteDataAnalyzeTooltipSuffix;

        public const string SpriteDataAnalyzeSingleSpriteTooltip =
            SpriteDataAnalyzeTooltipPrefix + "the selected Sprite " + SpriteDataAnalyzeTooltipSuffix;

        private const string SpriteDataAnalyzeTooltipPrefix = "This action will analyze ";

        private const string SpriteDataAnalyzeTooltipSuffix =
            "according to the selected Analysis Types and create a new " + nameof(SpriteData) + " asset.";

        public const string SpriteDataOutlineAnalysisTypeTooltip =
            "Specify the outline types to analyze. The Pixel Perfect outline is the most accurate but needs slightly more time to analyze.";

        public const string SpriteDataAnalysisTypeTooltip = "Specify the types of data to be analyzed.";

        public const string SpriteDataSpriteListTooltip =
            "A list of all Sprites which were analyzed in the current " + nameof(SpriteData) + " asset.";

        public const string SpriteDataOutlineColorTooltip = "Specify the color of the Sprite's outline.";

        public const string SpriteDataPreviewOutlineTooltip =
            "Specify the type of the Sprite outline to be drawn when a Sprite is selected.";

        public const string SpriteDataOutlinePrecisionAABBTooltip = "This shows the already given bounds of a Sprite.";

        public const string SpriteDataOutlinePrecisionOOBBTooltip =
            "This shows the generated OOBB based on the far most outside non-alpha pixel on each image side.";

        public const string SpriteDataOutlinePrecisionPixelPerfectTooltip =
            "This shows the generated pixel perfect outline based on an analysis of the Sprite's alpha.";

        public const string SpriteDataEditModeTooltip =
            "Choose either to see and modify the Sprite outline or the Sprite details.";

        public const string SpriteDataOOBBBorderTooltip =
            "Adjust the border of each side of the OOBB within the range of the Sprite's size.";

        public const string SpriteDataPixelPerfectSimplifyOutlineTooltip =
            "This value is used to evaluate which points should be removed from the outline. A higher value results in a simpler outline (fewer points). A positive value close to zero results in an outline with little to no reduction. A value of zero has no effect.";

        public const string SpriteDataAnalyzingActionDurationTooltip =
            "The time needed for the analysis is affected by the amount of sprites and whether these sprites are readable. If they are not readable, a temporary copy will be created.";

        //-- overlapping item list
        public const string OverlappingItemListBaseItemTooltip =
            "Every identified SpriteRenderer overlaps with this base item SpriteRenderer, and they all had the same sorting options.";

        public const string OverlappingItemListBaseItemSpriteRendererTooltip = "The SpriteRenderer of the base item.";

        public const string OverlappingItemListSpriteRendererTooltip =
            "This SpriteRenderer overlaps with the base item, and they had the same sorting options.";

        public const string OverlappingItemListSortingGroupTooltip =
            "This is the outer most SortingGroup of the SpriteRenderer, which had the same sorting options as the base item.";

        public const string OverlappingItemListBaseItemSortingGroupTooltip =
            "This is the outer most SortingGroup of the base item's SpriteRenderer.";

        public const string OverlappingItemListSortingLayerTooltip = "Change the sorting layer of this SpriteRenderer.";

        public const string OverlappingItemListTotalSortingOrderTooltip =
            "Change the sorting order of this SpriteRenderer.\nThis is the total sorting order of the SpriteRenderer.";

        public const string OverlappingItemListRelativeSortingOrderTooltip =
            "Change the sorting order of this SpriteRenderer.\nThe sorting order is displayed relative to the sorting order when they were unsorted.";

        public const string OverlappingItemListTooltip =
            "All listed SpriteRenderers are overlapping and had the same sorting option (identical sorting layer and sorting order).";

        public const string OverlappingItemListUsingRelativeSortingOrderTooltip =
            "Change displaying the sorting order of SpriteRenderers between their total value or their relative value, when they were unsorted.";


        //--SortingEditor
        public const string SortingEditorSpriteDataAssetTooltip =
            "This asset contains analyzed data of Sprites such as a more accurate outline or other data which is used to generate a sorting suggestion.";

        public const string SortingEditorUsingAutoSortingTooltip =
            "Enable to generate a sorting order suggestion of overlapping and unsorted SpriteRenderer based on given sorting criteria.";

        public const string SortingEditorSortingCriteriaListTooltip =
            "List of adjustable sorting criteria, which are used to generate sorting order suggestions.";

        public const string SortingEditorSortingTypeTooltip =
            "Specify, if either an entire Sorting Layer or a single Sprite is used to identify overlapping SpriteRenderers.";

        public const string SortingEditorUsingGameObjectParentsTooltip =
            "When enabled, only the given GameObject parents are being considered for the analysis.\n" +
            "Otherwise, all active and enabled SpriteRenderers of all open scenes will be used.";

        public const string SortingEditorSingleSpriteRendererTooltip =
            "Select a SpriteRenderer to search for overlapping SpriteRenderers.";

        public const string SortingEditorTransparencySortModeTooltip =
            "Determines how SpriteRenderers are being prioritized and therefore sorted during the process of rendering. Change this mode in the project setting.";

        public const string SortingEditorCameraTooltip =
            "The camera which is used to render SpriteRenderers (especially overlapping ones).";

        public const string SortingEditorMissingCameraTooltip =
            "The camera is necessary to determine the current Transparency Sort Mode and to check distances between SpriteRenderers and the camera.";

        public const string SortingEditorOutlinePrecisionTooltip =
            "Choose the outline precision of Sprites. The more accurate the precision is, the more time is needed to identify overlapping SpriteRenderers.";

        public const string SortingEditorOutlinePrecisionFast =
            "Overlapping SpriteRenderers can be identified very fast. The downside is that the Sprite's outline is not very accurate.";

        public const string SortingEditorOutlinePrecisionAccurate =
            "The Sprite's outline is very accurate, but it slows down the time to identify overlapping SpriteRenderers.";

        public const string SortingEditorOutlinePrecisionAABBTooltip =
            "Even this is the fastest check, its accuracy is not high. Therefore, it could identify some Sprites which are not visually overlapping because their alpha regions are overlapping.";

        public const string SortingEditorOutlinePrecisionOOBBTooltip =
            "This outline is more accurate, but the identification of overlapping SpriteRenderers will take slightly more time.\n" +
            "It considers the rotation of SpriteRenderers and requires a " +
            nameof(SpriteData) + " asset.";

        public const string SortingEditorOutlinePrecisionPixelPerfectTooltip =
            "This outline type has the highest precision, but it will take significantly more time.\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string SortingEditorSortingLayerTooltip =
            "Choose Sorting Layers to search for overlapping SpriteRenderers.";

        public const string SortingEditorScenePreviewDisplaySortingOrderTooltip =
            "When enabled, the current sorting order and the modified sorting order will be displayed in the scene as text.";

        public const string SortingEditorScenePreviewDisplaySortingLayerTooltip =
            "When enabled, the current sorting layer and the modified sorting layer will be displayed in the scene as text.";

        public const string SortingEditorScenePreviewReflectSortingOptionsInSceneTooltip =
            "When enabled, changed sorting options will be reflected directly in the scene.\n" +
            "Any not confirmed sorting option adjustments will be reverted, when closing this window.";

        public const string SortingEditorScenePreviewSpriteOutlineTooltip =
            "When enabled, SpriteRenderer outlines will be drawn in the scene.";

        public const string SortingEditorSpriteAnalyzedDataAddingChoiceTooltip =
            "Specifies whether a new " + nameof(SpriteData) +
            " asset should be created or the analyzed data should be added to a loaded " + nameof(SpriteData) +
            " asset.";

        public const string SortingEditorAnalyzingAllSpritesTooltip =
            "When enabled, Sprites from all active and enabled SpriteRenderers of opened scenes are analyzed. Otherwise, the selected Sprite will be analyzed only.";

        public const string SortingEditorSpriteSwapDescriptionTooltip =
            "The order of rendering overlapping and unsorted SpriteRenderer might differ per frame depending on several criteria. " +
            "The resulting effect is an abrupt swap of the SpriteRenderer in the foreground from such a group of renderers.";

        public const string SortingEditorAnalyzeSurroundingSpriteRendererTooltip =
            "When enabled, it will be searched iteratively in surrounding SpriteRenderer for Sprite swapping issues based on the previously found overlapping items and their adjusted sorting options. Newfound Sprite swapping issues will be solved by incrementing the sorting order of correspondent SpriteRenderers.\n\n" +
            "Although it might take some time this option is recommended as it finds and solves resulting Sprite swapping issues automatically.";

        public const string SortingEditorAnalyzeSurroundingSpriteRendererDurationTooltip =
            "As this check analyzes surrounding SpriteRenderers it can affect many SpriteRenderers depending on the setups of opened scenes.";

        public const string SortingEditorAnalyzeSRorSGWithChangedLayerFirstTooltip =
            "When enabled, SpriteRenderers or Sorting Groups with changed Sorting Layers will be analyzed regarding Sprite sorting issues first.\n" +
            "Functionality is not implemented yet.";
    }
}