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
using SpriteSortingPlugin.SpriteAnalysis.UI;
using SpriteSortingPlugin.SpriteSorting.AutoSorting;
using SpriteSortingPlugin.SpriteSorting.AutoSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.AutoSorting.Data;
using SpriteSortingPlugin.SpriteSorting.Logging;
using SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection;
using SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites;
using SpriteSortingPlugin.SpriteSorting.UI.Preview;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin.SpriteSorting.UI
{
    public class SpriteSwappingDetector : EditorWindow
    {
        private static readonly float LargerButtonHeight = EditorGUIUtility.singleLineHeight * 1.25f;
        private static readonly Array OutlinePrecisionTypes = Enum.GetValues(typeof(OutlinePrecision));
        private static readonly Array SortingTypes = Enum.GetValues(typeof(SortingType));

        private static readonly string[] LogOutputPath = new string[]
        {
            "SurveyData"
        };

        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 gameObjectsParentScrollPosition = Vector2.zero;

        [SerializeField] private SpriteData spriteData;
        [SerializeField] private OutlinePrecision outlinePrecision;
        private TransparencySortMode lastConfiguredTransparencySortMode;
        private CameraProjectionType cameraProjectionType;
        private SortingType sortingType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Transform> gameObjectParents;
        [SerializeField] private Camera camera;
        private SerializedObject serializedObject;
        private SerializedProperty gameObjectParentsSerializedProperty;
        private float outlinePrecisionSliderValue;

        private bool isSortingOptionExpanded;
        private int selectedSortingLayers;
        private List<string> selectedLayers;
        private bool isUsingGameObjectParents;

        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;

        private bool wasAnalyzeButtonClicked;
        private bool isAnalyzedButtonDisabled;
        private ReordableOverlappingItemList reordableOverlappingItemList;

        private bool isAnalyzingWithChangedLayerFirst;
        private bool isContinueSearching = true;

        private bool isSearchingSurroundingSpriteRenderer = true;
        private OverlappingSpriteDetector overlappingSpriteDetector;
        private SpriteDetectionData spriteDetectionData;

        private AutoSortingGenerator autoSortingGenerator;

        private AutoSortingCalculationData autoSortingCalculationData;
        private AutoSortingOptionsUI autoSortingOptionsUI;

        private List<SortingCriterionType> skippedSortingCriteriaList;
        private bool isShowingNoGlitchResult;

        [MenuItem(GeneralData.Name + "/" + GeneralData.DetectorName + " " + GeneralData.DetectorShortcut, false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSwappingDetector>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Swapping Detector");
            preview = new SpriteSortingEditorPreview();
            reordableOverlappingItemList = new ReordableOverlappingItemList();
            SortingLayerUtility.UpdateSortingLayerNames();
            SelectDefaultLayer();

            overlappingSpriteDetector = new OverlappingSpriteDetector();
            autoSortingOptionsUI = new AutoSortingOptionsUI();
            autoSortingOptionsUI.Init();
        }

        public void ActivateAutoSorting()
        {
            autoSortingOptionsUI.ActivateAutoSorting();
        }

        private void OnInspectorUpdate()
        {
            if (sortingType == SortingType.Layer)
            {
                CheckSortingLayerOrder();
            }

            //redraws completely UI. If performance need to be improved, check each name and redraw only if they differ. 
            Repaint();
        }

        private void CheckSortingLayerOrder()
        {
            var isSortingLayerOrderChanged = SortingLayerUtility.UpdateSortingLayerNames();
            if (!isSortingLayerOrderChanged)
            {
                return;
            }

            ReInitializeSelectedLayers();
            UpdateChangedSortingLayerOrderInOverlappingItems();
        }

        private void UpdateChangedSortingLayerOrderInOverlappingItems()
        {
            if (!wasAnalyzeButtonClicked || !HasOverlappingItems())
            {
                return;
            }

            var currentIndex = reordableOverlappingItemList.GetIndex();
            var item = currentIndex < 0 ? null : overlappingItems.Items[currentIndex];

            overlappingItems.OnChangedSortingLayerOrder();

            reordableOverlappingItemList.SetIndex(currentIndex < 0 ? -1 : overlappingItems.Items.IndexOf(item));
        }

        private void ReInitializeSelectedLayers()
        {
            var oldSelectedLayerNames = new List<string>(selectedLayers);

            selectedLayers.Clear();
            selectedSortingLayers = 0;

            for (var i = 0; i < SortingLayerUtility.SortingLayerNames.Length; i++)
            {
                var layerName = SortingLayerUtility.SortingLayerNames[i];
                if (!oldSelectedLayerNames.Contains(layerName))
                {
                    continue;
                }

                selectedLayers.Add(layerName);
                selectedSortingLayers ^= 1 << i;
            }
        }

        private void OnEnable()
        {
            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(this);

                gameObjectParentsSerializedProperty =
                    serializedObject.FindProperty(nameof(gameObjectParents));
                gameObjectParentsSerializedProperty.isExpanded = true;
                gameObjectParentsSerializedProperty.arraySize = 2;
                serializedObject.ApplyModifiedProperties();
            }

            SortingLayerUtility.UpdateSortingLayerNames();

            if (wasAnalyzeButtonClicked && HasOverlappingItems())
            {
                reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
            }

            preview.EnableSceneVisualization(true);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            serializedObject.Update();

            isAnalyzedButtonDisabled = false;
            GUILayout.Label("Sprite Swapping Detector", Styling.CenteredStyleBold, GUILayout.ExpandWidth(true));
            var descriptionLabelStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleCenter, wordWrap = true
            };
            var descriptionLabelContent = new GUIContent(
                "This tool identifies and helps to sort overlapping and unsorted SpriteRenderers, since such renderers often lead to unwanted Sprite swaps (visual glitches).",
                UITooltipConstants.SortingEditorSpriteSwapDescriptionTooltip);

            GUILayout.Label(descriptionLabelContent, descriptionLabelStyle, GUILayout.ExpandWidth(true));

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(wasAnalyzeButtonClicked))
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        GUILayout.Label("General Options");
                        DrawCameraOptions();

                        EditorGUILayout.Space();
                        DrawSpriteDataAssetOptions();

                        EditorGUILayout.Space();
                        DrawSortingOptions();

                        EditorGUILayout.Space();
                        autoSortingOptionsUI.DrawAutoSortingOptions(wasAnalyzeButtonClicked);

                        if (isShowingNoGlitchResult && changeScope.changed)
                        {
                            isShowingNoGlitchResult = false;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            var isAnalyzedButtonClickedThisFrame = false;

            if (wasAnalyzeButtonClicked && autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                GUILayout.Label(
                    "To refine Sorting Criteria: clear findings, adjust the criteria and find visual glitch again.");
            }

            EditorGUILayout.Space();

            var isCameraRequired = IsCameraRequired(out _);
            if (isCameraRequired)
            {
                var cameraSerializedProp = serializedObject.FindProperty(nameof(camera));
                if (cameraSerializedProp.objectReferenceValue == null)
                {
                    EditorGUILayout.LabelField(new GUIContent("Camera is required.", Styling.WarnIcon,
                        UITooltipConstants.SortingEditorMissingCameraTooltip));
                }
            }

            var isUsingSpriteData = IsUsingSpriteData(out _);
            if (isUsingSpriteData && spriteData == null)
            {
                EditorGUILayout.LabelField(new GUIContent($"{nameof(SpriteData)} asset is required.",
                    Styling.WarnIcon));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isAnalyzedButtonDisabled))
                {
                    var analyzeButtonStyle = wasAnalyzeButtonClicked ? Styling.ButtonStyle : Styling.ButtonStyleBold;
                    var buttonText = "Find visual glitches" +
                                     (autoSortingOptionsUI.IsApplyingAutoSorting ? " with Sorting Criteria" : "");
                    if (GUILayout.Button(buttonText, analyzeButtonStyle, GUILayout.MinHeight(LargerButtonHeight)))
                    {
                        Analyze();
                        isAnalyzedButtonClickedThisFrame = true;
                    }

                    using (new EditorGUI.DisabledScope(!wasAnalyzeButtonClicked && !isShowingNoGlitchResult))
                    {
                        if (GUILayout.Button("Clear Findings", analyzeButtonStyle,
                            GUILayout.MinHeight(LargerButtonHeight), GUILayout.ExpandWidth(false)))
                        {
                            isShowingNoGlitchResult = false;
                            wasAnalyzeButtonClicked = false;
                            if (HasOverlappingItems() && preview.IsUpdatingSpriteRendererInScene)
                            {
                                overlappingItems.RestoreSpriteRendererSortingOptions();
                            }

                            CleanUpReordableList();
                            preview.DisableSceneVisualizations();
                            skippedSortingCriteriaList = null;

                            EndScrollRect();

                            IncrementClearedFoundItems();
                            ClearLastLog();
                            return;
                        }
                    }
                }
            }

            if (isShowingNoGlitchResult)
            {
                EditorGUILayout.Space();
                UIUtil.DrawHorizontalLine(3);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (!HasOverlappingItems())
                {
                    var centeredStyleBold = new GUIStyle(Styling.CenteredStyleBold)
                    {
                        normal = {background = Styling.SpriteSortingNoSortingOrderIssuesBackgroundTexture}
                    };

                    var message = "No visual glitches were found with the current glitch finding options.";

                    GUILayout.Label(new GUIContent(message, Styling.NoSortingOrderIssuesIcon), centeredStyleBold,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));

                    CleanUpReordableList();
                    preview.DisableSceneVisualizations();
                }
            }

            if (!wasAnalyzeButtonClicked)
            {
                EditorGUILayout.Space();
                EndScrollRect();
                return;
            }

            EditorGUILayout.Space();
            UIUtil.DrawHorizontalLine(3);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (!HasOverlappingItems())
            {
                var centeredStyleBold = new GUIStyle(Styling.CenteredStyleBold)
                {
                    normal = {background = Styling.SpriteSortingNoSortingOrderIssuesBackgroundTexture}
                };

                var message = "No visual glitches were found with the current glitch finding options.";
                GUILayout.Label(new GUIContent(message, Styling.NoSortingOrderIssuesIcon), centeredStyleBold,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));

                CleanUpReordableList();
                preview.DisableSceneVisualizations();

                EndScrollRect();
                return;
            }

            ShowSkippedSortingCriteriaMessage();
            reordableOverlappingItemList.DoLayoutList();

            EditorGUILayout.Space();

            //TODO: analyze renderers or sorting groups with changed sorting layers first?
            // if (overlappingItems.HasChangedLayer)
            // {
            //     using (new EditorGUI.DisabledScope(true))
            //     {
            //         isAnalyzingWithChangedLayerFirst = EditorGUILayout.ToggleLeft(
            //             new GUIContent("Analyse Sprites / Sorting Groups with changed Layer first?",
            //                 UITooltipConstants.SortingEditorAnalyzeSRorSGWithChangedLayerFirstTooltip),
            //             isAnalyzingWithChangedLayerFirst);
            //     }
            // }

            var isConfirmButtonPressed = false;

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var surroundingAnalysisToggleContent = new GUIContent(
                    "Is analyzing surrounding SpriteRenderers (recommended)",
                    UITooltipConstants.SortingEditorAnalyzeSurroundingSpriteRendererTooltip);

                isSearchingSurroundingSpriteRenderer = EditorGUILayout.ToggleLeft(surroundingAnalysisToggleContent,
                    isSearchingSurroundingSpriteRenderer);

                if (isSearchingSurroundingSpriteRenderer)
                {
                    var surroundingAnalysisDurationInfoContent = new GUIContent(
                        "This option might take some time and can affect other, not listed SpriteRenderers.",
                        Styling.InfoIcon,
                        UITooltipConstants.SortingEditorAnalyzeSurroundingSpriteRendererDurationTooltip);
                    GUILayout.Label(surroundingAnalysisDurationInfoContent,
                        new GUIStyle(EditorStyles.label) {wordWrap = true});
                }

                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope(Styling.HelpBoxStyle))
                {
                    if (GUILayout.Button("Confirm", Styling.ButtonStyleBold, GUILayout.Height(LargerButtonHeight),
                        GUILayout.ExpandWidth(true)))
                    {
                        ApplySortingOptions();
                        isConfirmButtonPressed = true;

                        IncrementConfirmedSortingOrder();
                        SaveLogFile();

                        if (isContinueSearching)
                        {
                            Analyze();
                        }
                    }

                    EditorGUIUtility.labelWidth = 200;
                    isContinueSearching =
                        EditorGUILayout.ToggleLeft("And continue searching for next glitch?", isContinueSearching,
                            GUILayout.ExpandWidth(false));
                    EditorGUIUtility.labelWidth = 0;
                }
            }

            if (isConfirmButtonPressed)
            {
                EndScrollRect();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            UIUtil.DrawHorizontalLine(3);
            preview.DoPreview(isAnalyzedButtonClickedThisFrame);

            if (preview.IsUpdatingSpriteRendererInScene)
            {
                overlappingItems.ApplySortingOption(true);
            }
            else
            {
                overlappingItems.RestoreSpriteRendererSortingOptions();
            }

            EndScrollRect();
        }

        private void EndScrollRect()
        {
            EditorGUILayout.EndScrollView();
        }

        private void ShowSkippedSortingCriteriaMessage()
        {
            if (skippedSortingCriteriaList == null || skippedSortingCriteriaList.Count <= 0)
            {
                return;
            }

            var skippedCriteria = string.Join(", ", skippedSortingCriteriaList);
            var skippedCriteriaMessage =
                $"Skipped Sorting criteria (missing entries in selected {nameof(SpriteData)} asset):\n" +
                $"{skippedCriteria}";
            var skippedTooltip =
                $"Due to missing entries in the selected {nameof(SpriteData)} some Sorting criteria are skipped.";
            GUILayout.Label(new GUIContent(skippedCriteriaMessage, Styling.WarnIcon, skippedTooltip),
                Styling.LabelWrapStyle);
        }

        private void DrawSpriteDataAssetOptions()
        {
            GUILayout.Label("Sprite Data");
            var isUsingSpriteData = IsUsingSpriteData(out var errorMessage);
            using (new EditorGUI.DisabledScope(!isUsingSpriteData))
            {
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (var changeScope = new EditorGUI.ChangeCheckScope())
                        {
                            spriteData = EditorGUILayout.ObjectField(new GUIContent("Sprite Data Asset",
                                    UITooltipConstants.SortingEditorSpriteDataAssetTooltip), spriteData,
                                typeof(SpriteData), false) as SpriteData;
                            if (changeScope.changed)
                            {
                                preview.UpdateSpriteData(spriteData);
                            }
                        }

                        if (GUILayout.Button("Open Sprite Analysis window", GUILayout.ExpandWidth(false)))
                        {
                            var spriteAlphaEditorWindow = GetWindow<SpriteSwappingDataAnalysis>();
                            spriteAlphaEditorWindow.Show();
                        }
                    }

                    if (!isUsingSpriteData)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(
                                new GUIContent($"Is not used by {GeneralData.FullDetectorName}."));
                        }
                    }
                    else if (spriteData == null)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(
                                new GUIContent("Please select a Sprite Data Asset. It is used by " + errorMessage + ".",
                                    Styling.InfoIcon));
                            isAnalyzedButtonDisabled = true;
                        }
                    }
                }
            }
        }

        private void DrawSortingOptions()
        {
            isSortingOptionExpanded = EditorGUILayout.Foldout(isSortingOptionExpanded, "Glitch Finding Options", true);
            if (!isSortingOptionExpanded)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(
                        new GUIContent("Finding Type", UITooltipConstants.SortingEditorSortingTypeTooltip),
                        GUILayout.ExpandWidth(false));

                    GUILayout.Space(72.5f);

                    foreach (SortingType currentSortingType in SortingTypes)
                    {
                        using (var changeScope = new EditorGUI.ChangeCheckScope())
                        {
                            GUILayout.Toggle(currentSortingType == sortingType,
                                ObjectNames.NicifyVariableName(currentSortingType.ToString()), Styling.ButtonStyle,
                                GUILayout.ExpandWidth(true));
                            if (changeScope.changed)
                            {
                                sortingType = currentSortingType;
                            }
                        }
                    }
                }

                switch (sortingType)
                {
                    case SortingType.Layer:
                        ShowSortingLayers();

                        var gameObjectsParentContent = new GUIContent("Use specific parents?",
                            UITooltipConstants.SortingEditorUsingGameObjectParentsTooltip);
                        isUsingGameObjectParents =
                            UIUtil.DrawFoldoutBoolContent(isUsingGameObjectParents, gameObjectsParentContent);

                        if (isUsingGameObjectParents)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                if (gameObjectParentsSerializedProperty.isExpanded)
                                {
                                    using (var scrollScope = new EditorGUILayout.ScrollViewScope(
                                        gameObjectsParentScrollPosition, false, true,
                                        GUILayout.MaxHeight(80)))
                                    {
                                        gameObjectsParentScrollPosition = scrollScope.scrollPosition;
                                        EditorGUILayout.PropertyField(gameObjectParentsSerializedProperty, true);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.PropertyField(gameObjectParentsSerializedProperty, true);
                                }
                            }
                        }

                        break;
                    case SortingType.Sprite:
                        var serializedSpriteRenderer = serializedObject.FindProperty(nameof(spriteRenderer));
                        EditorGUILayout.PropertyField(serializedSpriteRenderer,
                            new GUIContent(serializedSpriteRenderer.displayName,
                                UITooltipConstants.SortingEditorSingleSpriteRendererTooltip), true);

                        // will not work for prefab scene
                        var spriteRendererRef = (SpriteRenderer) serializedSpriteRenderer.objectReferenceValue;

                        string errorMessage = null;
                        if (spriteRendererRef == null)
                        {
                            isAnalyzedButtonDisabled = true;
                        }
                        else if (!spriteRendererRef.gameObject.scene.isLoaded)
                        {
                            errorMessage = "Please select a SpriteRenderer of a currently loaded scene.";
                        }
                        else if (!spriteRendererRef.gameObject.activeInHierarchy || !spriteRendererRef.enabled ||
                                 spriteRendererRef.sprite == null)
                        {
                            errorMessage =
                                "The SpriteRenderer is not active in the scene or the sprite is null. Please select another sprite.";
                        }

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.LabelField(new GUIContent(errorMessage, Styling.WarnIcon));
                                isAnalyzedButtonDisabled = true;
                            }
                        }

                        break;
                }
            }
        }

        private void DrawCameraOptions()
        {
            using (var verticalScope = new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                if (!wasAnalyzeButtonClicked)
                {
                    lastConfiguredTransparencySortMode = GraphicsSettings.transparencySortMode;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Transparency Sort Mode:",
                                UITooltipConstants.SortingEditorTransparencySortModeTooltip),
                            new GUIContent(lastConfiguredTransparencySortMode.ToString()));
                    }

                    if (GUILayout.Button("Open Project Settings to change", GUILayout.ExpandWidth(false)))
                    {
                        SettingsService.OpenProjectSettings("Project/Graphics");
                    }
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    switch (lastConfiguredTransparencySortMode)
                    {
                        case TransparencySortMode.Perspective:
                            cameraProjectionType = CameraProjectionType.Perspective;
                            break;
                        case TransparencySortMode.Orthographic:
                            cameraProjectionType = CameraProjectionType.Orthographic;
                            break;
                        case TransparencySortMode.CustomAxis:
                            //TODO: add functionality for custom axis
                            break;
                    }

                    var isCameraNeeded = IsCameraRequired(out var errorMessage);
                    if (isCameraNeeded && !wasAnalyzeButtonClicked)
                    {
                        var cameraSerializedProp = serializedObject.FindProperty(nameof(camera));
                        EditorGUILayout.PropertyField(cameraSerializedProp,
                            new GUIContent("Camera", UITooltipConstants.SortingEditorCameraTooltip));

                        if (cameraSerializedProp.objectReferenceValue == null)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.LabelField(new GUIContent(
                                    "Please select a Camera. It is used to " + errorMessage + ".", Styling.InfoIcon,
                                    UITooltipConstants.SortingEditorMissingCameraTooltip));
                                isAnalyzedButtonDisabled = true;
                            }
                        }
                        else
                        {
                            cameraProjectionType = ((Camera) cameraSerializedProp.objectReferenceValue).orthographic
                                ? CameraProjectionType.Orthographic
                                : CameraProjectionType.Perspective;
                        }
                    }

                    DrawOutlinePrecisionSlider(verticalScope.rect.width);
                }
            }
        }

        private void DrawOutlinePrecisionSlider(float width)
        {
            var sliderContentRect = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight);
            sliderContentRect = EditorGUI.IndentedRect(sliderContentRect);

            var labelRect = sliderContentRect;
            labelRect.width = EditorGUIUtility.labelWidth - sliderContentRect.x / 2;

            var fromLabel = sliderContentRect;
            fromLabel.xMin = labelRect.xMax;
            fromLabel.width = 27.5f;

            var toLabel = sliderContentRect;
            toLabel.xMin = toLabel.xMax - 70;

            var sliderRect = sliderContentRect;
            sliderRect.xMin = fromLabel.xMax + 10;
            sliderRect.xMax = toLabel.xMin - 10;

            GUI.Label(labelRect,
                new GUIContent("Outline Precision", UITooltipConstants.SortingEditorOutlinePrecisionTooltip));
            GUI.Label(fromLabel,
                new GUIContent("Fast", UITooltipConstants.SortingEditorOutlinePrecisionFast));
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                outlinePrecisionSliderValue = GUI.HorizontalSlider(sliderRect, outlinePrecisionSliderValue, 0,
                    OutlinePrecisionTypes.Length - 1);
                if (changeScope.changed)
                {
                    outlinePrecisionSliderValue = (int) Math.Round(outlinePrecisionSliderValue);
                    outlinePrecision = (OutlinePrecision) outlinePrecisionSliderValue;
                    preview?.UpdateOutlineType(outlinePrecision);
                }
            }

            GUI.Label(toLabel,
                new GUIContent("Accurate", UITooltipConstants.SortingEditorOutlinePrecisionAccurate));

            var selectedLabel =
                new GUIContent("Selected: " + ObjectNames.NicifyVariableName(outlinePrecision.ToString()));

            switch (outlinePrecision)
            {
                case OutlinePrecision.AxisAlignedBoundingBox:
                    selectedLabel.tooltip = UITooltipConstants.SortingEditorOutlinePrecisionAABBTooltip;
                    break;
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    selectedLabel.tooltip = UITooltipConstants.SortingEditorOutlinePrecisionOOBBTooltip;
                    break;
                case OutlinePrecision.PixelPerfect:
                    selectedLabel.tooltip = UITooltipConstants.SortingEditorOutlinePrecisionPixelPerfectTooltip;
                    break;
            }

            EditorGUILayout.LabelField(new GUIContent(" "), selectedLabel);
        }

        private void ApplySortingOptions()
        {
            if (preview.IsUpdatingSpriteRendererInScene)
            {
                overlappingItems.RestoreSpriteRendererSortingOptions();
            }

            if (isSearchingSurroundingSpriteRenderer)
            {
                ApplySortingOptionsIterative();
            }
            else
            {
                overlappingItems.ApplySortingOption();
            }

            wasAnalyzeButtonClicked = false;
            overlappingItems = null;
            preview.EnableSceneVisualization(false);
            preview.CleanUpPreview();
            skippedSortingCriteriaList = null;
        }

        private void ApplySortingOptionsIterative()
        {
            if (overlappingItems.Items.Count <= 0)
            {
                return;
            }

            var layerOverlappingItemDictionary = new Dictionary<string, List<OverlappingItem>>();

            foreach (var overlappingItem in overlappingItems.Items)
            {
                var currentLayer = overlappingItem.sortingLayerName;

                if (!layerOverlappingItemDictionary.TryGetValue(currentLayer, out var overlappingItemList))
                {
                    overlappingItemList = new List<OverlappingItem>();
                    layerOverlappingItemDictionary.Add(currentLayer, overlappingItemList);
                }

                overlappingItemList.Add(overlappingItem);
            }

            FillSpriteDetectionData();

            if (overlappingSpriteDetector == null)
            {
                overlappingSpriteDetector = new OverlappingSpriteDetector();
            }

            foreach (var overlappingItemLayerPair in layerOverlappingItemDictionary)
            {
                var sortingOptions = overlappingSpriteDetector.AnalyzeSurroundingSpritesAndGetAdjustedSortingOptions(
                    overlappingItemLayerPair.Value, spriteDetectionData);

                foreach (var sortingOption in sortingOptions)
                {
                    var sortingComponent = EditorUtility.InstanceIDToObject(sortingOption.Key);
                    var sortingGroupComponent = sortingComponent as SortingGroup;
                    if (sortingGroupComponent != null)
                    {
                        var currentSortingLayerName = sortingGroupComponent.sortingLayerName;

                        if (sortingGroupComponent.sortingOrder == sortingOption.Value &&
                            currentSortingLayerName.Equals(overlappingItemLayerPair.Key))
                        {
                            continue;
                        }

                        var isChangedOverlappingItem =
                            overlappingItems.ContainsSortingGroupsOrSpriteRenderersInstanceId(
                                sortingComponent.GetInstanceID());

                        var changedLayerLog = currentSortingLayerName.Equals(overlappingItemLayerPair
                            .Key)
                            ? ""
                            : $", Sorting Layer {currentSortingLayerName} -> {overlappingItemLayerPair.Key}";

                        Debug.LogFormat("Changed sorting options on " +
                                        (isChangedOverlappingItem ? "previously" : "iterative ") +
                                        " found Sorting Group {0} Sorting Order: {1} -> {2}{3}",
                            sortingGroupComponent.name, sortingGroupComponent.sortingOrder, sortingOption.Value,
                            changedLayerLog);

                        Undo.RecordObject(sortingGroupComponent, "apply sorting options");
                        sortingGroupComponent.sortingOrder = sortingOption.Value;
                        sortingGroupComponent.sortingLayerName = overlappingItemLayerPair.Key;
                        EditorUtility.SetDirty(sortingGroupComponent);
                        continue;
                    }

                    var spriteRendererComponent = sortingComponent as SpriteRenderer;
                    if (spriteRendererComponent != null)
                    {
                        var currentSortingLayerName = spriteRendererComponent.sortingLayerName;
                        if (spriteRendererComponent.sortingOrder == sortingOption.Value &&
                            currentSortingLayerName.Equals(overlappingItemLayerPair.Key))
                        {
                            continue;
                        }

                        var isChangedOverlappingItem =
                            overlappingItems.ContainsSortingGroupsOrSpriteRenderersInstanceId(
                                sortingComponent.GetInstanceID());

                        var changedLayerLog = currentSortingLayerName.Equals(overlappingItemLayerPair
                            .Key)
                            ? ""
                            : $", Sorting Layer {currentSortingLayerName} -> {overlappingItemLayerPair.Key}";

                        Debug.LogFormat("Changed sorting options on " +
                                        (isChangedOverlappingItem ? "previously" : "iterative ") +
                                        " found SpriteRenderer {0}: Sorting Order: {1} -> {2}{3}",
                            spriteRendererComponent.name, spriteRendererComponent.sortingOrder, sortingOption.Value,
                            changedLayerLog);

                        Undo.RecordObject(spriteRendererComponent, "apply sorting options");
                        spriteRendererComponent.sortingOrder = sortingOption.Value;
                        spriteRendererComponent.sortingLayerName = overlappingItemLayerPair.Key;
                        EditorUtility.SetDirty(spriteRendererComponent);
                    }
                }
            }
        }

        private void FillSpriteDetectionData()
        {
            if (spriteDetectionData == null)
            {
                spriteDetectionData = new SpriteDetectionData();
            }

            spriteDetectionData.outlinePrecision = outlinePrecision;
            spriteDetectionData.spriteData = spriteData;
            spriteDetectionData.cameraProjectionType = cameraProjectionType;
        }

        private void CleanUpReordableList()
        {
            reordableOverlappingItemList?.CleanUp();
        }

        private bool HasOverlappingItems()
        {
            return overlappingItems != null && overlappingItems.Items.Count > 0;
        }

        private void ShowSortingLayers()
        {
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                selectedSortingLayers = EditorGUILayout.MaskField(
                    new GUIContent("Sorting Layers", UITooltipConstants.SortingEditorSortingLayerTooltip),
                    selectedSortingLayers, SortingLayerUtility.SortingLayerNames);
                if (changeScope.changed)
                {
                    UpdateSelectedLayers();
                }
            }
        }

        private void UpdateSelectedLayers()
        {
            selectedLayers.Clear();

            for (var i = 0; i < SortingLayerUtility.SortingLayerNames.Length; i++)
            {
                //bitmask moving check if bit is set
                var layer = 1 << i;
                if ((selectedSortingLayers & layer) != 0)
                {
                    selectedLayers.Add(SortingLayerUtility.SortingLayerNames[i]);
                }
            }
        }

        private void SelectDefaultLayer()
        {
            var defaultIndex = 0;
            for (var i = 0; i < SortingLayerUtility.SortingLayerNames.Length; i++)
            {
                if (!SortingLayerUtility.SortingLayerNames[i].Equals(SortingLayerUtility.SortingLayerNameDefault))
                {
                    continue;
                }

                defaultIndex = i;
                break;
            }

            selectedSortingLayers = 1 << defaultIndex;
            selectedLayers = new List<string> {SortingLayerUtility.SortingLayerNameDefault};
        }

        private void Analyze()
        {
            isShowingNoGlitchResult = false;
            wasAnalyzeButtonClicked = true;
            skippedSortingCriteriaList = null;

            var isVisualizingBoundsInScene = preview.IsVisualizingBoundsInScene;

            if (isVisualizingBoundsInScene)
            {
                preview.EnableSceneVisualization(false);
            }

            if (HasOverlappingItems() && preview.IsUpdatingSpriteRendererInScene)
            {
                overlappingItems.RestoreSpriteRendererSortingOptions();
            }

            FillSpriteDetectionData();

            var overlappingSpriteDetectionResult = new OverlappingSpriteDetectionResult();
            if (overlappingSpriteDetector == null)
            {
                overlappingSpriteDetector = new OverlappingSpriteDetector();
            }

            switch (sortingType)
            {
                case SortingType.Layer:

                    var selectedLayerIds = new List<int>();
                    foreach (var selectedLayer in selectedLayers)
                    {
                        selectedLayerIds.Add(SortingLayer.NameToID(selectedLayer));
                    }

                    var parentTransforms = isUsingGameObjectParents && gameObjectParentsSerializedProperty.arraySize > 0
                        ? gameObjectParents
                        : null;

                    overlappingSpriteDetectionResult = overlappingSpriteDetector.DetectOverlappingSprites(
                        selectedLayerIds, parentTransforms, spriteDetectionData);
                    break;
                case SortingType.Sprite:

                    overlappingSpriteDetectionResult =
                        overlappingSpriteDetector.DetectOverlappingSprites(spriteRenderer, spriteDetectionData);
                    break;
            }

            if (overlappingSpriteDetectionResult.overlappingSortingComponents == null ||
                overlappingSpriteDetectionResult.overlappingSortingComponents.Count <= 0 ||
                overlappingSpriteDetectionResult.baseItem == null)
            {
                overlappingItems = null;
                isShowingNoGlitchResult = true;
                wasAnalyzeButtonClicked = false;

                return;
            }

            if (isVisualizingBoundsInScene)
            {
                preview.EnableSceneVisualization(true);
            }

            List<OverlappingItem> overlappingItemList;
            OverlappingItem overlappingBaseItem;

            if (autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                overlappingItemList = ApplyAutoSorting(overlappingSpriteDetectionResult, out overlappingBaseItem);
            }
            else
            {
                ConvertToOverlappingItems(overlappingSpriteDetectionResult, out overlappingItemList,
                    out overlappingBaseItem);
                overlappingItemList.Insert(0, overlappingBaseItem);
            }

            overlappingItems = new OverlappingItems(overlappingBaseItem, overlappingItemList,
                autoSortingOptionsUI.IsApplyingAutoSorting);
            preview.UpdateOverlappingItems(overlappingItems);
            preview.UpdateSpriteData(spriteData);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);

            AddSortingSuggestionLoggingData();
            IncrementFoundGlitch();
        }

        private List<OverlappingItem> ApplyAutoSorting(
            OverlappingSpriteDetectionResult overlappingSpriteDetectionResult, out OverlappingItem overlappingBaseItem)
        {
            List<OverlappingItem> overlappingItemList;
            var sortingComponents =
                new List<SortingComponent>(overlappingSpriteDetectionResult.overlappingSortingComponents);
            CreateAndInitOverlappingItemSortingOrderAnalyzer();
            FillAutoSortingCalculationData();

            var resultList = autoSortingGenerator.GenerateAutomaticSortingOrder(
                overlappingSpriteDetectionResult.baseItem, sortingComponents, autoSortingCalculationData,
                out var skippedSortingCriteria);

            skippedSortingCriteriaList = skippedSortingCriteria;

            var sortingComponentList = new List<SortingComponent>();
            foreach (var autoSortingComponent in resultList)
            {
                sortingComponentList.Add(autoSortingComponent.sortingComponent);
            }

            overlappingSpriteDetectionResult.overlappingSortingComponents = sortingComponentList;

            ConvertToOverlappingItems(overlappingSpriteDetectionResult, out overlappingItemList,
                out overlappingBaseItem);

            for (var i = 0; i < overlappingItemList.Count; i++)
            {
                var overlappingItem = overlappingItemList[i];
                overlappingItem.originAutoSortingOrder =
                    Math.Abs(overlappingItem.originSortingOrder - resultList[i].sortingOrder);
            }

            return overlappingItemList;
        }

        private static void ConvertToOverlappingItems(OverlappingSpriteDetectionResult overlappingSpriteDetectionResult,
            out List<OverlappingItem> overlappingItems,
            out OverlappingItem overlappingBaseItem)
        {
            overlappingItems = null;
            overlappingBaseItem = null;

            if (overlappingSpriteDetectionResult.overlappingSortingComponents == null ||
                overlappingSpriteDetectionResult.baseItem == null)
            {
                return;
            }

            overlappingItems = new List<OverlappingItem>();
            overlappingBaseItem = new OverlappingItem(overlappingSpriteDetectionResult.baseItem, true);

            foreach (var overlappingSortingComponent in overlappingSpriteDetectionResult.overlappingSortingComponents)
            {
                if (overlappingSortingComponent.Equals(overlappingSpriteDetectionResult.baseItem))
                {
                    overlappingItems.Add(overlappingBaseItem);
                }
                else
                {
                    overlappingItems.Add(new OverlappingItem(overlappingSortingComponent));
                }
            }
        }

        private void CreateAndInitOverlappingItemSortingOrderAnalyzer()
        {
            autoSortingGenerator = new AutoSortingGenerator();

            foreach (var sortingCriterion in autoSortingOptionsUI.GetActiveSortingCriteria())
            {
                if (sortingCriterion is ContainmentSortingCriterion containmentSortingCriterionData)
                {
                    autoSortingGenerator.SetContainmentCriterion(containmentSortingCriterionData);
                    continue;
                }

                autoSortingGenerator.AddSortingCriterion(sortingCriterion);
            }
        }

        private void FillAutoSortingCalculationData()
        {
            if (autoSortingCalculationData == null)
            {
                autoSortingCalculationData = new AutoSortingCalculationData();
            }

            autoSortingCalculationData.outlinePrecision = outlinePrecision;
            autoSortingCalculationData.spriteData = spriteData;
            autoSortingCalculationData.cameraProjectionType = cameraProjectionType;
            autoSortingCalculationData.cameraTransform = camera == null ? null : camera.transform;
        }

        private void OnDisable()
        {
            preview.DisableSceneVisualizations();

            CleanUpReordableList();
        }

        private void OnDestroy()
        {
            if (HasOverlappingItems() && overlappingItems.IsContinuouslyReflectingSortingOptionsInScene)
            {
                overlappingItems.RestoreSpriteRendererSortingOptions();
            }

            preview.CleanUpPreview();
            PolygonColliderCacher.GetInstance().CleanUp();
            autoSortingOptionsUI.Cleanup();
            ClearLastLog();
        }

        #region validation

        private bool IsCameraRequired(out string usedBy)
        {
            usedBy = "";
            var isCameraRequired = false;

            if (GraphicsSettings.transparencySortMode == TransparencySortMode.Default)
            {
                usedBy = "identify visual glitches";
                isCameraRequired = true;
            }

            var isCameraRequiredForAutoSortingOptions =
                autoSortingOptionsUI.IsCameraRequired(out var usedByExplanation);

            if (isCameraRequiredForAutoSortingOptions)
            {
                if (usedBy.Length > 0)
                {
                    usedBy += " and ";
                }

                usedBy += usedByExplanation;
            }

            isCameraRequired |= isCameraRequiredForAutoSortingOptions;

            return isCameraRequired;
        }

        private bool IsUsingSpriteData(out string usedBy)
        {
            usedBy = "";
            var isUsingSpriteData = false;

            switch (outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                case OutlinePrecision.PixelPerfect:
                    isUsingSpriteData = true;
                    usedBy = "Outline Precision";
                    break;
            }

            var isSpriteDataRequiredForAutoSorting =
                autoSortingOptionsUI.IsSpriteDataRequired(out var usedByExplanation);

            if (isSpriteDataRequiredForAutoSorting)
            {
                if (usedBy.Length > 0)
                {
                    usedBy += " and ";
                }

                usedBy += usedByExplanation;
            }

            isUsingSpriteData |= isSpriteDataRequiredForAutoSorting;

            return isUsingSpriteData;
        }

        #endregion

        #region logging

        private void AddSortingSuggestionLoggingData()
        {
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return;
            }

            if (!autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                return;
            }

            var sortingOrderSuggestionLoggingData = new SortingSuggestionLoggingData();
            var validSortingCriteria = autoSortingOptionsUI.GenerateSortingCriteriaDataArray();
            sortingOrderSuggestionLoggingData.Init(overlappingItems.Items, validSortingCriteria);

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.AddSortingOrderSuggestionLoggingData(sortingOrderSuggestionLoggingData);
        }

        private void ClearLastLog()
        {
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return;
            }

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.ClearLastLoggingData();
        }

        private void SaveLogFile()
        {
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return;
            }

            if (!autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                return;
            }

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.ConfirmSortingOrder();
            var loggingDataJson = JsonUtility.ToJson(loggingData);

            var tempCachePath = Path.Combine(Application.temporaryCachePath, Path.Combine(LogOutputPath));
            var logDirectory = Path.Combine(tempCachePath, GeneralData.currentSurveyId, "LogFiles");
            Directory.CreateDirectory(logDirectory);
            var pathAndName = Path.Combine(logDirectory, loggingData.UniqueFileName);

            File.WriteAllText(pathAndName, loggingDataJson);
        }

        private void IncrementConfirmedSortingOrder()
        {
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return;
            }

            if (!autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                return;
            }

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.CurrentFoundGlitchStatistic.totalConfirmedGlitches++;
        }

        private void IncrementClearedFoundItems()
        {
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return;
            }

            if (!autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                return;
            }

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.CurrentFoundGlitchStatistic.totalClearedGlitches++;
        }

        private void IncrementFoundGlitch()
        {
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return;
            }

            if (!autoSortingOptionsUI.IsApplyingAutoSorting)
            {
                return;
            }

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.CurrentFoundGlitchStatistic.totalFoundGlitches++;
        }

        #endregion

        // private void ResetAndResortOverlappingItems()
        // {
        //     var wrapResult = new OverlappingSpriteDetectionResult
        //     {
        //         overlappingSortingComponents = new List<SortingComponent>()
        //     };
        //
        //     foreach (var overlappingItem in overlappingItems.Items)
        //     {
        //         if (!overlappingItem.IsBaseItem)
        //         {
        //             wrapResult.overlappingSortingComponents.Add(overlappingItem.SortingComponent);
        //         }
        //         else
        //         {
        //             wrapResult.baseItem = overlappingItem.SortingComponent;
        //         }
        //     }
        //
        //     var overlappingItemList = ApplyAutoSorting(wrapResult, out var overlappingBaseItem);
        //
        //     CleanUpReordableList();
        //     skippedSortingCriteriaList = null;
        //     preview.CleanUpPreview();
        //     
        //     overlappingItems = new OverlappingItems(overlappingBaseItem, overlappingItemList,
        //         autoSortingOptionsUI.IsApplyingAutoSorting && isReplacingOverlappingItemsWithAutoSortedResult);
        //     preview.UpdateOverlappingItems(overlappingItems);
        //     preview.UpdateSpriteData(spriteData);
        //     reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
        // }
    }
}