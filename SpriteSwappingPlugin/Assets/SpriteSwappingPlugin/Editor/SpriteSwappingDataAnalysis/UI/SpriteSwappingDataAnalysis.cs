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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpriteSwappingPlugin.SpriteAnalyzer;
using SpriteSwappingPlugin.SpriteSwappingDataAnalysis.AnalyzeActions;
using SpriteSwappingPlugin.UI;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace SpriteSwappingPlugin.SpriteSwappingDataAnalysis.UI
{
    public class SpriteSwappingDataAnalysis : EditorWindow, ISerializationCallbackReceiver
    {
        private const int MinWidthRightContentBar = 200;
        private const float LineSpacing = 1.5f;
        private const float RightAreaOffset = 3f;
        private const float LoadingSpriteDataAssetPercentageWidth = 0.39f;

        private static readonly string[] DefaultSaveFolderPath = new string[]
        {
            "Assets",
            "SpriteSwappingPlugin",
            "SpriteDataAssets"
        };

        private static readonly Array OutlinePrecisionTypes = Enum.GetValues(typeof(OutlinePrecision));

        // private SerializedObject serializedObject;

        [SerializeField] private SpriteData spriteData;
        [SerializeField] private Sprite spriteToAnalyze;
        private float lastToolbarWidth;
        private Sprite selectedSprite;
        private float selectedSpriteAspectRatio;

        private string searchString = "";
        private bool hasLoadedSpriteDataAsset;
        private Color outlineColor = Color.blue;

        private List<SpriteDataItem> spriteDataList;
        private ReorderableList reorderableSpriteList;
        private SearchField searchField;
        private Vector2 leftBarScrollPosition;
        private float lastHeight;

        private OutlineAnalysisType outlineAnalysisType = OutlineAnalysisType.All;
        private SpriteDataAnalysisType spriteDataAnalysisType = SpriteDataAnalysisType.All;
        private OutlinePrecision outlinePrecision;
        private float outlinePrecisionSliderValue;
        private SpriteDataAnalysisType[] spriteAnalyzerTypes;
        private bool isAnalyzingAllSprites = true;
        private bool isExpandingAnalyzeOptions;
        private SpriteAnalyzedDataAddingChoice spriteAnalyzedDataAddingChoice;
        private float expandedAnalyzeOptionsHeight = -1;
        private float collapsedAnalyzeOptionsHeight = -1;

        private SpriteDataItem selectedSpriteDataItem;

        private bool isDisplayingSpriteOutline = true;

        private SpriteDataAnalyzerContext spriteDataAnalyzerContext;
        private SpriteAnalyzeInputData spriteAnalyzeInputData;

        private SpriteOutlineAnalyzer spriteOutlineAnalyzer;
        private float outlineTolerance = 0.5f;
        private Dictionary<string, Vector2[]> originOutlines;
        private List<OriginOutlineWrapper> serializedOutlines;
        private SimplifiedOutlineToleranceErrorAppearance simplifiedOutlineToleranceErrorAppearance;


        [MenuItem(GeneralData.Name + "/" + GeneralData.DataAnalysisName +
                  " " + GeneralData.DataAnalysisShortcut, false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSwappingDataAnalysis>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite " + GeneralData.DataAnalysisName);
            minSize = new Vector2(720, minSize.y);
            ResetSpriteList();

            spriteAnalyzerTypes = (SpriteDataAnalysisType[]) Enum.GetValues(typeof(SpriteDataAnalysisType));
        }

        private void ResetSpriteList()
        {
            if (spriteDataList == null)
            {
                spriteDataList = new List<SpriteDataItem>();
            }
            else
            {
                spriteDataList.Clear();
            }
        }

        private void OnEnable()
        {
            // if (serializedObject == null)
            // {
            //     serializedObject = new SerializedObject(this);
            // }

            if (searchField == null)
            {
                searchField = new SearchField();
            }

            if (hasLoadedSpriteDataAsset)
            {
                if (reorderableSpriteList == null)
                {
                    InitReordableSpriteList();
                }
            }
        }

        private void OnGUI()
        {
            // serializedObject.Update();
            DrawToolbar();
            // serializedObject.ApplyModifiedProperties();

            var leftBarWidth = position.width / 4;
            if (leftBarWidth < MinWidthRightContentBar)
            {
                leftBarWidth = MinWidthRightContentBar;
            }

            if (Event.current.type == EventType.Repaint)
            {
                lastHeight = GUILayoutUtility.GetLastRect().height;
            }

            using (new EditorGUI.DisabledScope(!hasLoadedSpriteDataAsset))
            {
                DrawLeftContentBar(leftBarWidth);
            }

            var rightAreaRect = new Rect(leftBarWidth + RightAreaOffset, lastHeight + RightAreaOffset,
                position.width - leftBarWidth - RightAreaOffset, position.height - lastHeight - RightAreaOffset);

            using (new GUILayout.AreaScope(rightAreaRect, GUIContent.none,
                new GUIStyle {normal = {background = Styling.SpriteDataEditorOutlinePreviewBackgroundTexture}}))
            {
                if (selectedSpriteDataItem != null)
                {
                    if (isDisplayingSpriteOutline)
                    {
                        DrawOutlineContent(rightAreaRect);

                        if (outlinePrecision == OutlinePrecision.PixelPerfect &&
                            !originOutlines.ContainsKey(selectedSpriteDataItem.AssetGuid))
                        {
                            originOutlines.Add(selectedSpriteDataItem.AssetGuid,
                                selectedSpriteDataItem.outlinePoints);
                        }
                    }
                    else
                    {
                        DrawSpriteDetails(rightAreaRect);
                    }
                }
            }
        }

        private void DrawLeftContentBar(float leftBarWidth)
        {
            var leftAreaRect = new Rect(0, lastHeight, leftBarWidth, position.height);
            using (new GUILayout.AreaScope(leftAreaRect))
            {
                using (var scrollScope = new EditorGUILayout.ScrollViewScope(leftBarScrollPosition))
                {
                    leftBarScrollPosition = scrollScope.scrollPosition;
                    using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                    {
                        GUILayout.Label(
                            new GUIContent("Edit Sprite Data", UITooltipConstants.SpriteDataEditModeTooltip),
                            Styling.CenteredStyleBold, GUILayout.ExpandWidth(true));

                        using (new EditorGUILayout.HorizontalScope(Styling.HelpBoxStyle))
                        {
                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                GUILayout.Toggle(isDisplayingSpriteOutline, "Sprite outline", Styling.ButtonStyle);
                                if (changeScope.changed)
                                {
                                    isDisplayingSpriteOutline = true;
                                }
                            }

                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                GUILayout.Toggle(!isDisplayingSpriteOutline, "Sprite Details", Styling.ButtonStyle);
                                if (changeScope.changed)
                                {
                                    isDisplayingSpriteOutline = false;
                                }
                            }
                        }

                        if (isDisplayingSpriteOutline)
                        {
                            EditorGUILayout.Space();

                            GUILayout.Label(new GUIContent("Preview Outline",
                                UITooltipConstants.SpriteDataPreviewOutlineTooltip));

                            using (new EditorGUI.IndentLevelScope())
                            {
                                DrawOutlinePrecisionSlider();
                            }

                            GUILayout.Label(new GUIContent("Outline Color",
                                UITooltipConstants.SpriteDataOutlineColorTooltip));
                            using (new EditorGUI.IndentLevelScope())
                            {
                                outlineColor = EditorGUILayout.ColorField(outlineColor);
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.Label(new GUIContent("Sprites", UITooltipConstants.SpriteDataSpriteListTooltip),
                        Styling.CenteredStyleBold, GUILayout.ExpandWidth(true));
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        searchString = searchField.OnGUI(searchString);
                        if (changeScope.changed)
                        {
                            if (spriteData != null && spriteData.spriteDataDictionary.Count > 0)
                            {
                                FilterSpriteDataList();
                            }
                        }
                    }

                    if (reorderableSpriteList == null)
                    {
                        InitReordableSpriteList();
                    }

                    reorderableSpriteList.DoLayoutList();
                }
            }
        }

        private void DrawOutlinePrecisionSlider()
        {
            using (var verticalScope = new EditorGUILayout.VerticalScope())
            {
                var sliderContentRect =
                    GUILayoutUtility.GetRect(verticalScope.rect.width, EditorGUIUtility.singleLineHeight);
                sliderContentRect = EditorGUI.IndentedRect(sliderContentRect);

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    outlinePrecisionSliderValue = GUI.HorizontalSlider(sliderContentRect, outlinePrecisionSliderValue,
                        0, OutlinePrecisionTypes.Length - 1);

                    if (changeScope.changed)
                    {
                        outlinePrecisionSliderValue = (int) Math.Round(outlinePrecisionSliderValue);
                        outlinePrecision = (OutlinePrecision) outlinePrecisionSliderValue;
                    }
                }

                var selectedLabel =
                    new GUIContent($"Selected: {ObjectNames.NicifyVariableName(outlinePrecision.ToString())}");

                switch (outlinePrecision)
                {
                    case OutlinePrecision.AxisAlignedBoundingBox:
                        selectedLabel.tooltip =
                            UITooltipConstants.SpriteDataOutlinePrecisionAABBTooltip;
                        break;
                    case OutlinePrecision.ObjectOrientedBoundingBox:
                        selectedLabel.tooltip =
                            UITooltipConstants.SpriteDataOutlinePrecisionOOBBTooltip;
                        break;
                    case OutlinePrecision.PixelPerfect:
                        selectedLabel.tooltip = UITooltipConstants
                            .SpriteDataOutlinePrecisionPixelPerfectTooltip;
                        break;
                }

                EditorGUILayout.LabelField(selectedLabel);
            }
        }

        private void DrawToolbar()
        {
            using (var toolbarScope = new EditorGUILayout.HorizontalScope())
            {
                if (Event.current.type == EventType.Repaint)
                {
                    lastToolbarWidth = toolbarScope.rect.width;
                }

                var currentLoadingSpriteDataPercentageWidth = lastToolbarWidth * LoadingSpriteDataAssetPercentageWidth;
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle,
                    GUILayout.Width(currentLoadingSpriteDataPercentageWidth)))
                {
                    GUILayout.Label("Sprite Data Asset", Styling.CenteredStyle);

                    EditorGUIUtility.labelWidth = 110;
                    spriteData = EditorGUILayout.ObjectField(new GUIContent(GUIContent.none), spriteData,
                        typeof(SpriteData), false, GUILayout.MinWidth(290)) as SpriteData;
                    EditorGUIUtility.labelWidth = 0;

                    if (GUILayout.Button($"Load existing {nameof(SpriteData)}"))
                    {
                        LoadSpriteDataList();
                    }
                }

                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle, GUILayout.ExpandWidth(true)))
                {
                    GUILayout.Label("Sprite Data Analysis", Styling.CenteredStyle);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (var changeScope = new EditorGUI.ChangeCheckScope())
                        {
                            isExpandingAnalyzeOptions =
                                EditorGUILayout.Foldout(isExpandingAnalyzeOptions, "Analyzing options", true);
                            if (changeScope.changed)
                            {
                                SetAnalyzeOptionsHeightDependingOnFoldoutExpand();
                            }
                        }

                        GUILayout.Label(new GUIContent("Analysis might take some time.", Styling.InfoIcon,
                            UITooltipConstants.SpriteDataAnalyzingActionDurationTooltip), GUILayout.ExpandWidth(false));
                    }

                    if (isExpandingAnalyzeOptions)
                    {
                        EditorGUI.indentLevel++;

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUIUtility.labelWidth = 100;
                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                spriteDataAnalysisType = (SpriteDataAnalysisType) EditorGUILayout.EnumFlagsField(
                                    new GUIContent("Analysis Type", UITooltipConstants.SpriteDataAnalysisTypeTooltip),
                                    spriteDataAnalysisType, GUILayout.ExpandWidth(true));
                                if (changeScope.changed)
                                {
                                    if (!spriteDataAnalysisType.HasFlag(SpriteDataAnalysisType.Outline))
                                    {
                                        spriteDataAnalysisType |= SpriteDataAnalysisType.Outline;
                                    }
                                }
                            }

                            EditorGUIUtility.labelWidth = 95;
                            outlineAnalysisType = (OutlineAnalysisType) EditorGUILayout.EnumFlagsField(
                                new GUIContent("Outline Type", UITooltipConstants.SpriteDataOutlineAnalysisTypeTooltip),
                                outlineAnalysisType, GUILayout.MinWidth(200));

                            EditorGUIUtility.labelWidth = 0;
                        }

                        using (new EditorGUI.DisabledScope(!hasLoadedSpriteDataAsset))
                        {
                            if (!hasLoadedSpriteDataAsset)
                            {
                                spriteAnalyzedDataAddingChoice = SpriteAnalyzedDataAddingChoice.NewSpriteData;
                            }

                            EditorGUIUtility.labelWidth = 175;
                            spriteAnalyzedDataAddingChoice =
                                (SpriteAnalyzedDataAddingChoice) EditorGUILayout.EnumPopup(
                                    new GUIContent("Adding analyzed data to",
                                        UITooltipConstants.SortingEditorSpriteAnalyzedDataAddingChoiceTooltip),
                                    spriteAnalyzedDataAddingChoice);
                            EditorGUIUtility.labelWidth = 0;
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUIUtility.labelWidth = 105;
                            isAnalyzingAllSprites =
                                EditorGUILayout.ToggleLeft(new GUIContent("Is analyzing all Sprites",
                                        UITooltipConstants.SortingEditorAnalyzingAllSpritesTooltip),
                                    isAnalyzingAllSprites, GUILayout.ExpandWidth(false));

                            EditorGUIUtility.labelWidth = 0;
                            using (new EditorGUI.DisabledScope(isAnalyzingAllSprites))
                            {
                                spriteToAnalyze =
                                    (Sprite) EditorGUILayout.ObjectField(spriteToAnalyze, typeof(Sprite), false);
                            }
                        }

                        EditorGUI.indentLevel--;
                    }

                    var buttonTextBuilder = new StringBuilder("Analyze &");

                    switch (spriteAnalyzedDataAddingChoice)
                    {
                        case SpriteAnalyzedDataAddingChoice.NewSpriteData:
                            buttonTextBuilder.Append("generate new ");
                            break;
                        case SpriteAnalyzedDataAddingChoice.CurrentlyLoaded:
                            buttonTextBuilder.Append("add to existing ");
                            break;
                    }

                    buttonTextBuilder.Append(nameof(SpriteData));

                    if (GUILayout.Button(new GUIContent(buttonTextBuilder.ToString(),
                        isAnalyzingAllSprites
                            ? UITooltipConstants.SpriteDataAnalyzeAllTooltip
                            : UITooltipConstants.SpriteDataAnalyzeSingleSpriteTooltip)))
                    {
                        AnalyzeSprites();
                    }
                }
            }

            GetDynamicHeightsOfAnalyzeOptionFoldout();
        }

        //hack for not getting delayed ui updating of collapsed/expanded foldout
        private void SetAnalyzeOptionsHeightDependingOnFoldoutExpand()
        {
            if (isExpandingAnalyzeOptions && expandedAnalyzeOptionsHeight > -1)
            {
                lastHeight = expandedAnalyzeOptionsHeight;
            }
            else if (!isExpandingAnalyzeOptions && collapsedAnalyzeOptionsHeight > -1)
            {
                lastHeight = collapsedAnalyzeOptionsHeight;
            }
            else
            {
                //happens only the first time the foldout is collapsed/expanded
                EditorApplication.delayCall += RepaintDelayed;
            }
        }

        //hack for getting dynamic height of analyze options foldout. Otherwise a visual delay from updating the GUI is visible
        private void GetDynamicHeightsOfAnalyzeOptionFoldout()
        {
            if (expandedAnalyzeOptionsHeight >= 0 && collapsedAnalyzeOptionsHeight >= 0)
            {
                return;
            }

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var currentFoldoutHeight = GUILayoutUtility.GetLastRect().height;
            if (isExpandingAnalyzeOptions)
            {
                expandedAnalyzeOptionsHeight = currentFoldoutHeight;
            }

            else
            {
                collapsedAnalyzeOptionsHeight = currentFoldoutHeight;
            }
        }

        private void RepaintDelayed()
        {
            Repaint();
        }

        private void DrawSpriteDetails(Rect rightAreaRect)
        {
            var hasSelectedDataItem = selectedSpriteDataItem != null;

            using (new EditorGUILayout.VerticalScope(GUILayout.Height(210)))
            {
                EditorGUILayout.Space();
                EditorGUI.DrawTextureTransparent(new Rect(rightAreaRect.width / 2 - 75, rightAreaRect.y, 150, 150),
                    selectedSprite != null ? selectedSprite.texture : Texture2D.grayTexture, ScaleMode.ScaleToFit);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    var resolutionLabel = EditorGUILayout.GetControlRect();
                    var resolutionContentLabel = EditorGUI.PrefixLabel(resolutionLabel,
                        new GUIContent("Resolution", UITooltipConstants.SpriteDataResolutionTooltip));
                    var spriteSize = hasSelectedDataItem
                        ? new int[] {selectedSprite.texture.width, selectedSprite.texture.height}
                        : new int[] {0, 0};
                    EditorGUI.MultiIntField(resolutionContentLabel,
                        new GUIContent[] {new GUIContent("Width"), new GUIContent("height")}, spriteSize);
                }
            }

            GUILayout.Space(1.5f);

            var analyzeButtonWidth = GUILayout.Width(rightAreaRect.width / 8);

            using (new EditorGUI.DisabledGroupScope(!hasSelectedDataItem))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        var sharpness = hasSelectedDataItem ? selectedSpriteDataItem.spriteAnalysisData.sharpness : 0;
                        sharpness = EditorGUILayout.DoubleField(
                            new GUIContent("Sharpness", UITooltipConstants.SpriteDataSharpnessTooltip),
                            sharpness);
                        if (changeScope.changed)
                        {
                            selectedSpriteDataItem.spriteAnalysisData.sharpness = Math.Max(0, sharpness);
                        }
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        FillSpriteAnalyzeDataForUpdating(selectedSpriteDataItem.AssetGuid);
                        AnalyzeSprite(SpriteAnalysisType.Sharpness);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        var perceivedLightness = hasSelectedDataItem
                            ? selectedSpriteDataItem.spriteAnalysisData.perceivedLightness
                            : 0;
                        perceivedLightness = EditorGUILayout.Slider(
                            new GUIContent("Perceived Lightness",
                                UITooltipConstants.SpriteDataPerceivedLightnessTooltip),
                            perceivedLightness, 0, 100);
                        if (changeScope.changed)
                        {
                            selectedSpriteDataItem.spriteAnalysisData.perceivedLightness = perceivedLightness;
                        }
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        FillSpriteAnalyzeDataForUpdating(selectedSpriteDataItem.AssetGuid);
                        AnalyzeSprite(SpriteAnalysisType.Lightness);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        var averageAlpha = hasSelectedDataItem
                            ? selectedSpriteDataItem.spriteAnalysisData.averageAlpha
                            : 0;
                        averageAlpha = EditorGUILayout.Slider(
                            new GUIContent("Average alpha", UITooltipConstants.SpriteDataAverageAlphaTooltip),
                            averageAlpha, 0, 1);
                        if (changeScope.changed)
                        {
                            selectedSpriteDataItem.spriteAnalysisData.averageAlpha = averageAlpha;
                        }
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        FillSpriteAnalyzeDataForUpdating(selectedSpriteDataItem.AssetGuid);
                        AnalyzeSprite(SpriteAnalysisType.AverageAlpha);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        var primaryColor = hasSelectedDataItem
                            ? selectedSpriteDataItem.spriteAnalysisData.primaryColor
                            : Color.black;
                        primaryColor = EditorGUILayout.ColorField(
                            new GUIContent("Primary Color", UITooltipConstants.SpriteDataPrimaryColorTooltip),
                            primaryColor);
                        if (changeScope.changed)
                        {
                            selectedSpriteDataItem.spriteAnalysisData.primaryColor = primaryColor;
                        }
                    }

                    if (GUILayout.Button("Analyze", analyzeButtonWidth))
                    {
                        FillSpriteAnalyzeDataForUpdating(selectedSpriteDataItem.AssetGuid);
                        AnalyzeSprite(SpriteAnalysisType.PrimaryColor);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("", "");
                    if (GUILayout.Button("Analyze All", analyzeButtonWidth))
                    {
                        FillSpriteAnalyzeDataForUpdating(selectedSpriteDataItem.AssetGuid);
                        AnalyzeSprite(SpriteAnalysisType.Lightness, SpriteAnalysisType.Sharpness,
                            SpriteAnalysisType.PrimaryColor, SpriteAnalysisType.AverageAlpha);
                    }
                }
            }
        }

        private void AnalyzeSprite(params SpriteAnalysisType[] currentSpriteAnalyzerTypes)
        {
            if (spriteDataAnalyzerContext == null)
            {
                spriteDataAnalyzerContext = new SpriteDataAnalyzerContext();
            }

            spriteDataAnalyzerContext.ClearSpriteAnalyzeActions();
            foreach (var spriteAnalyzerType in currentSpriteAnalyzerTypes)
            {
                spriteDataAnalyzerContext.AddSpriteAnalyzeAction(spriteAnalyzerType);
            }

            spriteData = spriteDataAnalyzerContext.Analyze(spriteAnalyzeInputData);
        }

        private void DrawOutlineContent(Rect rightAreaRect)
        {
            var textureRect = new Rect(0, 0, rightAreaRect.width, rightAreaRect.height);
            EditorGUI.DrawTextureTransparent(textureRect, selectedSprite.texture, ScaleMode.ScaleToFit);
            var rectWithAlphaBorders = CalculateRectWithAlphaBorders(textureRect);
            Handles.color = outlineColor;
            switch (outlinePrecision)
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    DrawAABB(rectWithAlphaBorders);
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    DrawOOBB(rectWithAlphaBorders, rightAreaRect);
                    break;
                case OutlinePrecision.PixelPerfect:
                    DrawOutline(rectWithAlphaBorders, rightAreaRect);
                    break;
            }
        }

        private void DrawAABB(Rect rectWithAlphaBorders)
        {
            var lineDifferenceFactors = new Vector2(1, 1);
            Handles.DrawWireCube(rectWithAlphaBorders.center, rectWithAlphaBorders.size);
            Handles.DrawWireCube(rectWithAlphaBorders.center, rectWithAlphaBorders.size + lineDifferenceFactors);
            Handles.DrawWireCube(rectWithAlphaBorders.center, rectWithAlphaBorders.size - lineDifferenceFactors);
        }

        private void DrawOutline(Rect rectWithAlphaBorders, Rect rightAreaRect)
        {
            if (!selectedSpriteDataItem.IsValidOutline())
            {
                return;
            }

            var scaleXFactor = rectWithAlphaBorders.width / selectedSprite.bounds.size.x;
            var scaleYFactor = rectWithAlphaBorders.height / selectedSprite.bounds.size.y;
            var scale = new Vector2(scaleYFactor, -scaleXFactor);

            var lineDifferenceFactors = new Vector2(0.5f, 0.5f);
            var lastPoint = selectedSpriteDataItem.outlinePoints[0] * scale + rectWithAlphaBorders.center;
            for (var i = 1; i < selectedSpriteDataItem.outlinePoints.Length; i++)
            {
                var nextPoint = selectedSpriteDataItem.outlinePoints[i] * scale + rectWithAlphaBorders.center;

                Handles.DrawLine(lastPoint, nextPoint);
                Handles.DrawLine(lastPoint + lineDifferenceFactors, nextPoint + lineDifferenceFactors);
                Handles.DrawLine(lastPoint - lineDifferenceFactors, nextPoint - lineDifferenceFactors);
                lastPoint = nextPoint;
            }

            var outlineRect = new Rect(0, position.height - lastHeight, rightAreaRect.width, 75);
            outlineRect.y -= outlineRect.height;
            using (new GUILayout.AreaScope(outlineRect))
            {
                var alphaRectangleBorderRect = new Rect(0, 0, rightAreaRect.width, 75);
                EditorGUI.DrawRect(alphaRectangleBorderRect, Styling.TransparentBackgroundColor);

                DrawOutlineBorderSettings();
            }
        }

        private void DrawOutlineBorderSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Outline Options");
                GUILayout.FlexibleSpace();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Re-Analyze", GUILayout.Width(90)))
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "reanalyzed outline");
                    FillSpriteAnalyzeDataForUpdating(selectedSpriteDataItem.AssetGuid);
                    spriteAnalyzeInputData.outlineAnalysisType = OutlineAnalysisType.PixelPerfect;
                    AnalyzeSprite(SpriteAnalysisType.Outline);

                    simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
                }

                if (GUILayout.Button("Reset (last loaded Outline)", GUILayout.Width(160)))
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "Reset to last loaded outline");
                    var isContainingOutline =
                        originOutlines.TryGetValue(selectedSpriteDataItem.AssetGuid, out var originOutline);
                    if (isContainingOutline)
                    {
                        selectedSpriteDataItem.outlinePoints = originOutline;
                        simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    outlineTolerance = EditorGUILayout.FloatField(new GUIContent("Outline Tolerance",
                        UITooltipConstants.SpriteDataPixelPerfectSimplifyOutlineTooltip), outlineTolerance);
                    if (changeScope.changed)
                    {
                        if (outlineTolerance < 0)
                        {
                            outlineTolerance = 0;
                        }
                    }
                }

                if (GUILayout.Button("Simplify", GUILayout.Width(253)))
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "simplified outline");
                    SimplifyOutline();
                }
            }

            if (simplifiedOutlineToleranceErrorAppearance != SimplifiedOutlineToleranceErrorAppearance.Nothing)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    var outlineErrorContent = new GUIContent();

                    switch (simplifiedOutlineToleranceErrorAppearance)
                    {
                        case SimplifiedOutlineToleranceErrorAppearance.NotValid:
                            outlineErrorContent.image = Styling.WarnIcon;
                            outlineErrorContent.text =
                                "Simplified Outline was not valid, please lower the outline tolerance";
                            break;
                        case SimplifiedOutlineToleranceErrorAppearance.ReplacedByOOBB:
                            outlineErrorContent.text =
                                "Simplified Outline was not valid and replaced by current OOBB. Please lower the outline tolerance next time.";
                            break;
                    }

                    GUILayout.Label(outlineErrorContent);
                }
            }

            GUILayout.FlexibleSpace();
        }

        private void SimplifyOutline()
        {
            if (spriteOutlineAnalyzer == null)
            {
                spriteOutlineAnalyzer = new SpriteOutlineAnalyzer();
            }

            var isSimplified = spriteOutlineAnalyzer.Simplify(selectedSpriteDataItem.outlinePoints,
                outlineTolerance, out var simplifiedPoints);

            if (isSimplified)
            {
                selectedSpriteDataItem.outlinePoints = simplifiedPoints;
            }
            else if (selectedSpriteDataItem.IsValidOOBB())
            {
                var colliderPoints = new Vector2[5];
                var oobbLocalWorldPoints = selectedSpriteDataItem.objectOrientedBoundingBox.LocalWorldPoints;
                colliderPoints[4] = new Vector2(oobbLocalWorldPoints[0].x, oobbLocalWorldPoints[0].y);

                for (var i = 0; i < oobbLocalWorldPoints.Length; i++)
                {
                    var point = oobbLocalWorldPoints[i];
                    colliderPoints[i] = new Vector2(point.x, point.y);
                }

                selectedSpriteDataItem.outlinePoints = colliderPoints;
            }
            else
            {
                simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.NotValid;
            }
        }

        private void DrawOOBB(Rect rectWithAlphaBorders, Rect rightAreaRect)
        {
            if (!selectedSpriteDataItem.IsValidOOBB())
            {
                return;
            }

            var scaleXFactor = rectWithAlphaBorders.width / selectedSprite.bounds.size.x;
            var scaleYFactor = rectWithAlphaBorders.height / selectedSprite.bounds.size.y;

            var oobb = selectedSpriteDataItem.objectOrientedBoundingBox;
            var alphaRectangleBorder = oobb.AlphaRectangleBorder;

            var newBoundsWidth = scaleXFactor * (alphaRectangleBorder.spriteWidth - alphaRectangleBorder.leftBorder -
                                                 alphaRectangleBorder.rightBorder) / alphaRectangleBorder.pixelPerUnit;

            var newBoundsHeight = scaleYFactor * (alphaRectangleBorder.spriteHeight - alphaRectangleBorder.topBorder -
                                                  alphaRectangleBorder.bottomBorder) /
                                  alphaRectangleBorder.pixelPerUnit;

            var scaledSize = new Vector2(newBoundsWidth, newBoundsHeight);

            var rectCenter = rectWithAlphaBorders.center - new Vector2(oobb.BoundsCenterOffset.x * scaleXFactor,
                oobb.BoundsCenterOffset.y * scaleYFactor);
            var lineDifferenceFactors = new Vector2(1, 1);
            Handles.DrawWireCube(rectCenter, scaledSize);
            Handles.DrawWireCube(rectCenter, scaledSize + lineDifferenceFactors);
            Handles.DrawWireCube(rectCenter, scaledSize - lineDifferenceFactors);

            var outlineRect = new Rect(0, position.height - lastHeight, rightAreaRect.width, 100);
            outlineRect.y -= outlineRect.height;
            using (new GUILayout.AreaScope(outlineRect))
            {
                var alphaRectangleBorderRect = new Rect(0, 0, rightAreaRect.width, 125);
                EditorGUI.DrawRect(alphaRectangleBorderRect, Styling.TransparentBackgroundColor);

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    DrawAlphaRectangleBorderSettings(alphaRectangleBorderRect);

                    if (changeScope.changed)
                    {
                        Undo.RegisterCompleteObjectUndo(spriteData, "changed OOBB size");
                        oobb.UpdateBoxSizeWithBorder();
                    }
                }
            }
        }

        private Rect CalculateRectWithAlphaBorders(Rect textureRect)
        {
            var rectAspectRatio = textureRect.width / textureRect.height;

            Rect displayingTextureRect;

            // set rect and sprite in ratio and adjust bounding box according to the ScalingMode ScaleToFit, from UnityEngine.GUI.CalculateScaledTextureRects
            //https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Modules/IMGUI/GUI.cs#L262
            if (rectAspectRatio > selectedSpriteAspectRatio)
            {
                //rectangle is longer than sprite's width
                var num2 = selectedSpriteAspectRatio / rectAspectRatio;

                displayingTextureRect =
                    new Rect(textureRect.xMin + (float) ((double) textureRect.width * (1.0 - (double) num2) * 0.5),
                        textureRect.yMin, num2 * textureRect.width, textureRect.height);
            }
            else
            {
                var num3 = rectAspectRatio / selectedSpriteAspectRatio;

                displayingTextureRect = new Rect(textureRect.xMin,
                    textureRect.yMin + (float) ((double) textureRect.height * (1.0 - (double) num3) * 0.5),
                    textureRect.width, num3 * textureRect.height);
            }

            return displayingTextureRect;
        }

        private void DrawAlphaRectangleBorderSettings(Rect rect)
        {
            var alphaBorder = selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder;
            var intFieldLength = rect.width / 3f;
            var halfSpriteWidth = alphaBorder.spriteWidth / 2;

            var halfSpriteHeight = alphaBorder.spriteHeight / 2;

            EditorGUI.LabelField(new Rect(rect.width / 2 - 45, rect.y, 90, EditorGUIUtility.singleLineHeight),
                "Adjust Borders");
            if (GUI.Button(new Rect(rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), "Reset"))
            {
                selectedSpriteDataItem.objectOrientedBoundingBox.ResetAlphaRectangleBorder();
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            alphaBorder.topBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                alphaBorder.topBorder, 0, halfSpriteHeight);
            rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;

            {
                alphaBorder.leftBorder = EditorGUI.IntSlider(
                    new Rect(rect.x, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                    alphaBorder.leftBorder, 0, halfSpriteWidth);

                var oobbIconContent = new GUIContent(Styling.MoveIcon, UITooltipConstants.SpriteDataOOBBBorderTooltip);
                EditorGUI.LabelField(new Rect(rect.width / 2 - Styling.MoveIcon.width,
                    rect.y - (Styling.MoveIcon.height / 2f) + EditorGUIUtility.singleLineHeight / 2f,
                    Styling.MoveIcon.width, EditorGUIUtility.singleLineHeight * 2), oobbIconContent);

                alphaBorder.rightBorder = EditorGUI.IntSlider(
                    new Rect(rect.x + 2 * intFieldLength, rect.y, intFieldLength,
                        EditorGUIUtility.singleLineHeight),
                    alphaBorder.rightBorder, 0, halfSpriteWidth);

                rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;
            }

            alphaBorder.bottomBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                alphaBorder.bottomBorder, 0, halfSpriteHeight);
            selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder = alphaBorder;
        }

        private void FilterSpriteDataList()
        {
            searchString = searchString.Trim();
            spriteDataList.Clear();
            foreach (var spriteDataItem in spriteData.spriteDataDictionary.Values)
            {
                if (searchString.Length == 0 || spriteDataItem.AssetName.Contains(searchString))
                {
                    spriteDataList.Add(spriteDataItem);
                }
            }
        }

        private void LoadSpriteDataList()
        {
            simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
            if (originOutlines == null)
            {
                originOutlines = new Dictionary<string, Vector2[]>();
            }
            else
            {
                originOutlines.Clear();
            }

            hasLoadedSpriteDataAsset = spriteData != null;

            if (!hasLoadedSpriteDataAsset)
            {
                selectedSpriteDataItem = null;
                selectedSprite = null;
                selectedSpriteAspectRatio = 0;
                ResetSpriteList();
                return;
            }

            FilterSpriteDataList();
        }

        private void AnalyzeSprites()
        {
            if (outlineAnalysisType == OutlineAnalysisType.Nothing &&
                spriteDataAnalysisType == SpriteDataAnalysisType.Nothing ||
                (!isAnalyzingAllSprites && spriteToAnalyze == null))
            {
                return;
            }

            var lastSelectedSpriteDataItemGuid = selectedSpriteDataItem?.AssetGuid;
            reorderableSpriteList.index = -1;
            selectedSpriteDataItem = null;
            selectedSprite = null;
            selectedSpriteAspectRatio = 0;

            FillSpriteAnalyzeInputDataForAdding();
            var currentSpriteAnalyzerTypes = CreateSpriteAnalyzerTypeArray();
            AnalyzeSprite(currentSpriteAnalyzerTypes);

            SaveOrReSaveSpriteData();

            LoadSpriteDataList();
            if (string.IsNullOrEmpty(lastSelectedSpriteDataItemGuid))
            {
                if (reorderableSpriteList.count > 0)
                {
                    reorderableSpriteList.index = 0;
                    OnSpriteSelected(reorderableSpriteList);
                }
            }
            else
            {
                RestoreSpriteDataItemSelection(lastSelectedSpriteDataItemGuid);
            }

            simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
        }

        private void RestoreSpriteDataItemSelection(string lastSelectedSpriteDataItemGuid)
        {
            if (string.IsNullOrEmpty(lastSelectedSpriteDataItemGuid))
            {
                return;
            }

            for (var i = 0; i < spriteDataList.Count; i++)
            {
                var spriteDataItem = spriteDataList[i];
                if (!spriteDataItem.AssetGuid.Equals(lastSelectedSpriteDataItemGuid))
                {
                    continue;
                }

                reorderableSpriteList.index = i;
                OnSpriteSelected(reorderableSpriteList);
                return;
            }
        }

        private void SaveOrReSaveSpriteData()
        {
            var assetPathAndName = "";
            var forceReserializeAssetsOptions = ForceReserializeAssetsOptions.ReserializeAssets;

            switch (spriteAnalyzedDataAddingChoice)
            {
                case SpriteAnalyzedDataAddingChoice.NewSpriteData:

                    var tempAssetPath =
                        Path.Combine(Path.Combine(DefaultSaveFolderPath), $"{nameof(SpriteData)}.asset");
                    Directory.CreateDirectory(Path.Combine(DefaultSaveFolderPath));

                    assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(tempAssetPath);
                    forceReserializeAssetsOptions = ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata;
                    break;
                case SpriteAnalyzedDataAddingChoice.CurrentlyLoaded:
                    assetPathAndName = AssetDatabase.GetAssetPath(spriteAnalyzeInputData.spriteData.GetInstanceID());
                    break;
                default:
                    var tempAssetPath2 =
                        Path.Combine(Path.Combine(DefaultSaveFolderPath), $"{nameof(SpriteData)}.asset");
                    Directory.CreateDirectory(Path.Combine(DefaultSaveFolderPath));

                    assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(tempAssetPath2);
                    break;
            }

            if (spriteAnalyzedDataAddingChoice == SpriteAnalyzedDataAddingChoice.NewSpriteData)
            {
                AssetDatabase.CreateAsset(spriteData, assetPathAndName);
            }

            AssetDatabase.ForceReserializeAssets(new string[] {assetPathAndName}, forceReserializeAssetsOptions);
            AssetDatabase.Refresh();
        }

        private void FillSpriteAnalyzeInputDataForAdding()
        {
            switch (spriteAnalyzedDataAddingChoice)
            {
                case SpriteAnalyzedDataAddingChoice.CurrentlyLoaded:
                    spriteAnalyzeInputData.spriteData = spriteData;
                    break;
                default:
                    spriteAnalyzeInputData.spriteData = null;
                    break;
            }

            spriteAnalyzeInputData.assetGuid = null;
            spriteAnalyzeInputData.sprite = isAnalyzingAllSprites ? null : spriteToAnalyze;
            spriteAnalyzeInputData.outlineAnalysisType = outlineAnalysisType;
        }

        private void FillSpriteAnalyzeDataForUpdating(string assetGuid)
        {
            spriteAnalyzeInputData.assetGuid = assetGuid;
            spriteAnalyzeInputData.sprite = null;
            spriteAnalyzeInputData.spriteData = spriteData;
        }

        private SpriteAnalysisType[] CreateSpriteAnalyzerTypeArray()
        {
            var typeList = new List<SpriteAnalysisType>();
            if (spriteDataAnalysisType == SpriteDataAnalysisType.Nothing)
            {
                return typeList.ToArray();
            }

            foreach (var spriteAnalyzerType in spriteAnalyzerTypes)
            {
                if (spriteAnalyzerType == SpriteDataAnalysisType.Nothing ||
                    spriteAnalyzerType == SpriteDataAnalysisType.All ||
                    !spriteDataAnalysisType.HasFlag(spriteAnalyzerType))
                {
                    continue;
                }

                var type = SpriteAnalysisType.Outline;

                switch (spriteAnalyzerType)
                {
                    case SpriteDataAnalysisType.Sharpness:
                        type = SpriteAnalysisType.Sharpness;
                        break;
                    case SpriteDataAnalysisType.Lightness:
                        type = SpriteAnalysisType.Lightness;
                        break;
                    case SpriteDataAnalysisType.PrimaryColor:
                        type = SpriteAnalysisType.PrimaryColor;
                        break;
                    case SpriteDataAnalysisType.AverageAlpha:
                        type = SpriteAnalysisType.AverageAlpha;
                        break;
                }

                typeList.Add(type);
            }

            return typeList.ToArray();
        }

        private void InitReordableSpriteList()
        {
            reorderableSpriteList = new ReorderableList(spriteDataList, typeof(string), false, false, false, false);
            reorderableSpriteList.onSelectCallback += OnSpriteSelected;
            reorderableSpriteList.drawElementCallback += DrawElementCallBack;
            reorderableSpriteList.drawElementBackgroundCallback += DrawElementBackgroundCallBack;
        }

        private void DrawElementBackgroundCallBack(Rect rect, int index, bool isActive, bool isFocused)
        {
            Color color;
            if (isActive)
            {
                color = Styling.ListElementActiveColor;
            }

            else if (isFocused)
            {
                color = Styling.ListElementFocussingColor;
            }

            else
            {
                color = index % 2 == 0
                    ? Styling.ListElementBackground1
                    : Styling.ListElementBackground2;
            }

            EditorGUI.DrawRect(rect, color);
        }

        private void DrawElementCallBack(Rect rect, int index, bool isActive, bool isFocused)
        {
            var oobb = spriteDataList[index];
            EditorGUI.LabelField(rect, oobb.AssetName);
        }

        private void OnSpriteSelected(ReorderableList list)
        {
            if (list.index < 0)
            {
                return;
            }

            selectedSpriteDataItem = spriteDataList[list.index];
            var path = AssetDatabase.GUIDToAssetPath(selectedSpriteDataItem.AssetGuid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            selectedSprite = sprite;
            selectedSpriteAspectRatio = (float) selectedSprite.texture.width / (float) selectedSprite.texture.height;

            simplifiedOutlineToleranceErrorAppearance = SimplifiedOutlineToleranceErrorAppearance.Nothing;
        }

        private void OnDestroy()
        {
            reorderableSpriteList.onSelectCallback = null;
            reorderableSpriteList.drawElementCallback = null;
            reorderableSpriteList.drawElementBackgroundCallback = null;
            reorderableSpriteList = null;
        }

        public void OnBeforeSerialize()
        {
            if (originOutlines == null)
            {
                return;
            }

            serializedOutlines = new List<OriginOutlineWrapper>(originOutlines.Count);

            foreach (var outline in originOutlines)
            {
                serializedOutlines.Add(new OriginOutlineWrapper(outline.Key, outline.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            if (serializedOutlines == null)
            {
                return;
            }

            originOutlines = new Dictionary<string, Vector2[]>();
            foreach (var serializedOutline in serializedOutlines)
            {
                originOutlines.Add(serializedOutline.assetGuid, serializedOutline.outline);
            }
        }
    }

    [Serializable]
    internal class OriginOutlineWrapper
    {
        public string assetGuid;
        public Vector2[] outline;

        public OriginOutlineWrapper(string assetGuid, Vector2[] outline)
        {
            this.assetGuid = assetGuid;
            this.outline = outline;
        }
    }

    internal enum SimplifiedOutlineToleranceErrorAppearance
    {
        Nothing,
        ReplacedByOOBB,
        NotValid
    }

    internal enum SpriteAnalyzedDataAddingChoice
    {
        NewSpriteData,
        CurrentlyLoaded
    }
}