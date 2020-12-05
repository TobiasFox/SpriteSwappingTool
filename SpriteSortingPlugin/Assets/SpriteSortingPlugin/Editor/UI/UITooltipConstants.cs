namespace SpriteSortingPlugin.UI
{
    public static class UITooltipConstants
    {
        //--sorting criteria
        public const string ContainmentCheckingAlphaTooltip =
            "Completely enclosed SpriteRenderer will be sorted in the background. Consider comparing their transparency by enable this option.\n" +
            "When enabled, the average transparency of the larger sprite will be compared to the alpha threshold.";

        public const string ContainmentAlphaThresholdTooltip =
            "Specifies the alpha threshold up to which a SpriteRenderer should be rendered in the foreground even though it is completely enclosed by another SpriteRenderer\n" +
            "Range: 0.0 - 1.0";

        public const string ContainmentEncapsulatedSpriteInForegroundTooltip =
            "When enabled, completely enclosed SpriteRenderers will be sorted in the foreground. Otherwise, the enclosed SpriteRenderer will be sorted in the background and may be completely hidden.";

        public const string ContainmentTooltip =
            "Compares if SpriteRenderers are completely enclosed by other SpriteRenderers.\n" +
            "When analyzing the alpha value, a " + nameof(SpriteData) + " asset is required.";

        public const string CameraDistanceTooltip =
            "Compares the distance to the camera. Will be ignored when using orthographic Transparency Sort mode or Default Transparency Sort mode and orthographic camera project.";

        public const string CameraDistanceForegroundSpriteTooltip =
            "When enabled, SpriteRenderer with a shorter distance to the camera will be sorted in the foreground.";

        public const string PrimaryColorTooltip =
            "Compares the primary color of given SpriteRenderers and sorts SpriteRenderer in the foreground which are closer to the foreground color.";

        public const string PrimaryColorChannelsTooltip = "Specify which color channels should be used.";

        public const string PrimaryColorForegroundColorTooltip =
            "Specify the color of SpriteRenderers, which will be sorted in the foreground.";

        public const string PrimaryColorBackgroundColorTooltip =
            "Specify the color of SpriteRenderers, which will be sorted in the background.";

        public const string SizeTooltip =
            "Compares the size, their relation as well as the intersection area of SpriteRenderers.\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string SizeForegroundSpriteTooltip =
            "When enabled, larger sprites will be sorted in the foreground.";

        public const string ResolutionTooltip = "Compares sprites resolutions in pixel.";

        public const string ResolutionForegroundSpriteTooltip =
            "When enabled, sprites with higher resolutions will be sorted in the foreground.";

        public const string SharpnessTooltip =
            "Compares the sharpness of sprites by analyzing their amount of edges (rapid intensity changes).\n" +
            "Requires a " + nameof(SpriteData) + " asset.";

        public const string SharpnessForegroundSpriteTooltip =
            "When enabled, sharper sprites will be sorted in the foreground.";

        public const string PerceivedLightnessTooltip = "Compares only the perceived lightness of Sprites.\n" +
                                                        "Requires a " + nameof(SpriteData) + " asset.";

        public const string PerceivedLightnessForegroundSpriteTooltip =
            "When enabled, lighter sprites will be sorted in the foreground.";
        
        public const string SortingCriteriaWeightTooltip =
            "Controls the weight of this sorting criterion. The higher the value, the more affects this criterion the resulting order of sprites.\n" +
            "Minimum: 0.0";
        
        public const string SortingCriteriaContainmentWeightTooltip =
            "The containment criterion is preferred over all other criteria as the calculation of the order of sprites differs when this criterion is enabled.";

        //--SpriteDataEditorWindow
        public const string SpriteDataSharpnessTooltip =
            "This value indicates how sharp a sprite is. The higher the value, the sharper the sprite.\n" +
            "Minimum: 0.0 ";

        public const string SpriteDataPerceivedLightnessTooltip =
            "This value represents the average perceived lightness of a sprite, where 0 means dark and 100.0 means bright.\n" +
            "Range: 0.0 - 100.0";

        public const string SpriteDataAverageAlphaTooltip =
            "This value represents the average alpha of a sprite ignoring completely transparent pixels.";

        public const string SpriteDataPrimaryColorTooltip =
            "This is the primary or average Color of a sprite ignoring completely transparent pixels.";

        public const string SpriteDataResolutionTooltip = "The sprite's resolution in pixel.";

        public const string SpriteDataAnalyzeAllTooltip =
            SpriteDataAnalyzeTooltipPrefix + "all visible sprites in opened scenes " + SpriteDataAnalyzeTooltipSuffix;

        public const string SpriteDataAnalyzeSingleSpriteTooltip =
            SpriteDataAnalyzeTooltipPrefix + "the selected sprite " + SpriteDataAnalyzeTooltipSuffix;

        private const string SpriteDataAnalyzeTooltipPrefix = "This action will analyze ";

        private const string SpriteDataAnalyzeTooltipSuffix =
            "according to the selected Analysis Types and create a new " + nameof(SpriteData) + " asset.";

        public const string SpriteDataOutlineAnalysisTypeTooltip =
            "Specify the outline types to analyze. The Pixel Perfect outline is the most accurate but needs slightly more time to analyze.";

        public const string SpriteDataAnalysisTypeTooltip = "Specify the types of data to be analyzed.";

        public const string SpriteDataSpriteListTooltip =
            "A list of all Sprites which were analyzed in the current " + nameof(SpriteData) + " asset.";

        public const string SpriteDataOutlineColorTooltip = "Specify the color of the sprite's outline.";

        public const string SpriteDataPreviewOutlineTooltip =
            "Specify the type of the sprite outline to be drawn when a sprite is selected.";

        public const string SpriteDataOutlinePrecisionAABBTooltip = "This shows the already given bounds of a sprite.";

        public const string SpriteDataOutlinePrecisionOOBBTooltip =
            "This shows the generated OOBB based on the far most outside non-alpha pixel on each image side.";

        public const string SpriteDataOutlinePrecisionPixelPerfectTooltip =
            "This shows the generated pixel perfect outline based on an analysis of the sprite's alpha.";

        public const string SpriteDataEditModeTooltip =
            "Choose either to see and modify the sprite outline or the sprite details.";

        public const string SpriteDataOOBBBorderTooltip =
            "Adjust the border of each side of the OOBB within the range of the sprite's size.";

        public const string SpriteDataPixelPerfectSimplifyOutlineTooltip =
            "This value is used to evaluate which points should be removed from the outline. A higher value results in a simpler outline (fewer points). A positive value close to zero results in an outline with little to no reduction. A value of zero has no effect.";

        public const string SpriteDataAnalyzingActionDurationTooltip =
            "Depending on the amount of sprites to be analyzed this action may take some time.";

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
            "This asset contains analyzed data of sprites such as a more accurate outline or other data which is used to generate a sorting suggestion.";

        public const string SortingEditorUsingAutoSortingTooltip =
            "Enable to generate an automatic sorting order suggestion of overlapping and unsorted SpriteRenderer based on given sorting criteria.";

        public const string SortingEditorSortingTypeTooltip =
            "Specify, if either a completely Sorting Layer or a single sprite is used to identify overlapping SpriteRenderers.";

        public const string SortingEditorUsingGameObjectParentsTooltip =
            "Search for overlapping SpriteRenderer in the given GameObjects only.";

        public const string SortingEditorSingleSpriteRendererTooltip =
            "Select a SpriteRenderer to search for overlapping SpriteRenderers.";

        public const string SortingEditorTransparencySortModeTooltip =
            "Determines how SpriteRenderers are being prioritized and therefore sorted during the process of rendering. Change this mode in the project setting.";

        public const string SortingEditorCameraTooltip =
            "The camera which is used to render SpriteRenderers (especially overlapping ones).";

        public const string SortingEditorMissingCameraTooltip =
            "The camera is necessary to determine the current Transparency Sort Mode and to check distances between SpriteRenderers and the camera.";

        public const string SortingEditorOutlinePrecisionTooltip =
            "Choose the outline precision of sprites. The more accurate the precision is, the more time is needed to identify overlapping SpriteRenderers.";

        public const string SortingEditorOutlinePrecisionFast =
            "Overlapping SpriteRenderers can be identified very fast. The downside is that the sprite's outline is not very accurate.";

        public const string SortingEditorOutlinePrecisionAccurate =
            "The sprite's outline is very accurate, but it slows down the time to identify overlapping SpriteRenderers.";

        public const string SortingEditorOutlinePrecisionAABBTooltip =
            "Even this is the fastest check, its accuracy is not high. Therefore, it could identify some sprites which are not visually overlapping because their alpha regions are overlapping.";

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

        public const string SortingEditorScenePreviewSpriteOutlineTooltip =
            "When enabled, SpriteRenderer outlines will be drawn in the scene.";

        public const string SortingEditorSpriteAnalyzedDataAddingChoiceTooltip =
            "Specifies whether a new " + nameof(SpriteData) +
            " asset should be created or the analyzed data should be added to a loaded " + nameof(SpriteData) +
            " asset.";

        public const string SortingEditorAnalyzingAllSpritesTooltip =
            "When enabled, sprites from all active and enabled SpriteRenderers of opened scenes are analyzed. Otherwise, the selected Sprite will be analyzed only.";

        public const string SortingEditorSpriteSwapDescriptionTooltip =
            "The order of rendering overlapping and unsorted SpriteRenderer might differ per frame depending on several criteria. " +
            "The resulting effect is an abrupt swap of the SpriteRenderer in the foreground from such a group of renderers.";
    }
}