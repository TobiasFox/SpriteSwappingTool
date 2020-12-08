using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SpriteAnalysis.UI;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection;
using SpriteSortingPlugin.SpriteSorting.UI.AutoSorting;
using SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites;
using SpriteSortingPlugin.SpriteSorting.UI.Preview;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin.SpriteSorting.UI
{
    public class SpriteRendererSwappingDetectorEditorWindow : EditorWindow
    {
        private static readonly float LargerButtonHeight = EditorGUIUtility.singleLineHeight * 1.25f;
        private static readonly Array OutlinePrecisionTypes = Enum.GetValues(typeof(OutlinePrecision));
        private static readonly Array SortingTypes = Enum.GetValues(typeof(SortingType));

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

        private int selectedSortingLayers;
        private List<string> selectedLayers;
        private bool isUsingGameObjectParents;

        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;

        private bool wasAnalyzeButtonClicked;
        private bool isAnalyzedButtonDisabled;
        private ReordableOverlappingItemList reordableOverlappingItemList;

        private bool isAnalyzingWithChangedLayerFirst;

        private bool isSearchingSurroundingSpriteRenderer = true;
        private OverlappingSpriteDetector overlappingSpriteDetector;
        private SpriteDetectionData spriteDetectionData;

        private AutoSortingGenerator autoSortingGenerator;

        private AutoSortingCalculationData autoSortingCalculationData;
        private AutoSortingOptionsUI autoSortingOptionsUI;

        //TODO remove bool, is only for debugging
        private bool isReplacingOverlappingItemsWithAutoSortedResult = true;
        private List<string> autoSortingResultNames;

        private ReorderableList autoSortingResultList;
        //end remove debug variables

        [MenuItem("Window/Sprite Swapping Detector %q")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteRendererSwappingDetectorEditorWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Swapping Detector");
            preview = new SpriteSortingEditorPreview();
            reordableOverlappingItemList = new ReordableOverlappingItemList();
            SortingLayerUtility.UpdateSortingLayerNames();

            //TODO: remove
            // SelectDefaultSpriteAlphaData();

            overlappingSpriteDetector = new OverlappingSpriteDetector();
            autoSortingOptionsUI = new AutoSortingOptionsUI();
            autoSortingOptionsUI.Init();
        }


        private void SelectDefaultSpriteAlphaData()
        {
            try
            {
                var guids = AssetDatabase.FindAssets("DefaultSpriteAlphaData");
                spriteData =
                    AssetDatabase.LoadAssetAtPath<SpriteData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            catch (Exception e)
            {
                Debug.Log("auto selection of SpriteAlphaData went wrong");
            }
        }

        private void OnInspectorUpdate()
        {
            if (sortingType == SortingType.Layer)
            {
                CheckSortingLayerOrder();
            }

            //TODO: could be more performant by comparing the name each frame instead of redrawing everything
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
                "This tool identifies and helps to sort overlapping and unsorted SpriteRenderers, since such renderers often lead to an unwanted swap.",
                UITooltipConstants.SortingEditorSpriteSwapDescriptionTooltip);

            GUILayout.Label(descriptionLabelContent, descriptionLabelStyle, GUILayout.ExpandWidth(true));

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(wasAnalyzeButtonClicked))
                {
                    GUILayout.Label("General Options");
                    DrawCameraOptions();

                    EditorGUILayout.Space();
                    DrawSpriteDataAssetOptions();

                    EditorGUILayout.Space();
                    DrawSortingOptions();

                    EditorGUILayout.Space();
                    autoSortingOptionsUI.DrawAutoSortingOptions(wasAnalyzeButtonClicked);
                }
            }

            serializedObject.ApplyModifiedProperties();
            var isAnalyzedButtonClickedThisFrame = false;

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isAnalyzedButtonDisabled))
                {
                    var analyzeButtonStyle = wasAnalyzeButtonClicked ? Styling.ButtonStyle : Styling.ButtonStyleBold;

                    if (GUILayout.Button("Find overlapping and unsorted SpriteRenderers", analyzeButtonStyle,
                        GUILayout.MinHeight(LargerButtonHeight)))
                    {
                        Analyze();
                        isAnalyzedButtonClickedThisFrame = true;
                    }

                    using (new EditorGUI.DisabledScope(!wasAnalyzeButtonClicked))
                    {
                        if (GUILayout.Button("Clear Findings", analyzeButtonStyle,
                            GUILayout.MinHeight(LargerButtonHeight), GUILayout.ExpandWidth(false)))
                        {
                            wasAnalyzeButtonClicked = false;
                            CleanUpReordableList();
                            preview.DisableSceneVisualizations();

                            EndScrollRect();
                            return;
                        }
                    }
                }
            }

            if (!wasAnalyzeButtonClicked)
            {
                EndScrollRect();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (!HasOverlappingItems())
            {
                var centeredStyleBold = new GUIStyle(Styling.CenteredStyleBold)
                {
                    normal = {background = Styling.SpriteSortingNoSortingOrderIssuesBackgroundTexture}
                };

                GUILayout.Label(
                    new GUIContent(
                        "No sorting order issues with overlapping SpriteRenderers were found in all opened scenes.",
                        Styling.NoSortingOrderIssuesIcon),
                    centeredStyleBold, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));

                CleanUpReordableList();
                preview.DisableSceneVisualizations();

                EndScrollRect();
                return;
            }

            reordableOverlappingItemList.DoLayoutList();

            EditorGUILayout.Space();

            if (overlappingItems.HasChangedLayer)
            {
                isAnalyzingWithChangedLayerFirst = EditorGUILayout.ToggleLeft(
                    "Analyse Sprites / Sorting Groups with changed Layer first?", isAnalyzingWithChangedLayerFirst);
            }

            var surroundingAnalysisToggleContent = new GUIContent(
                "Is analyzing surrounding SpriteRenderers (recommended)",
                UITooltipConstants.SortingEditorAnalyzeSurroundingSpriteRendererTooltip);

            isSearchingSurroundingSpriteRenderer = EditorGUILayout.ToggleLeft(surroundingAnalysisToggleContent,
                isSearchingSurroundingSpriteRenderer);

            if (isSearchingSurroundingSpriteRenderer)
            {
                var surroundingAnalysisDurationInfoContent = new GUIContent(
                    "The following option might take some time and can affect other surrounding SpriteRenderers of the ones listed above.",
                    Styling.InfoIcon, UITooltipConstants.SortingEditorAnalyzeSurroundingSpriteRendererDurationTooltip);
                GUILayout.Label(surroundingAnalysisDurationInfoContent,
                    new GUIStyle(EditorStyles.label) {wordWrap = true});
            }

            ShowConfirmButton(out var isConfirmButtonPressed);

            autoSortingResultList?.DoLayoutList();

            if (isConfirmButtonPressed)
            {
                EndScrollRect();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            preview.DoPreview(isAnalyzedButtonClickedThisFrame);

            EndScrollRect();
        }

        private void ShowConfirmButton(out bool isConfirmButtonPressed)
        {
            isConfirmButtonPressed = false;
            var confirmButtonLabels = sortingType == SortingType.Layer
                ? new[] {"Confirm", "Confirm and continue searching"}
                : new[] {"Confirm"};

            var selectedConfirmButtonIndex =
                GUILayout.SelectionGrid(-1, confirmButtonLabels, confirmButtonLabels.Length, Styling.ButtonStyleBold,
                    GUILayout.Height(LargerButtonHeight));
            if (selectedConfirmButtonIndex >= 0)
            {
                ApplySortingOptions();
                isConfirmButtonPressed = true;

                if (selectedConfirmButtonIndex == 1)
                {
                    //TODO: check isAnalyzingWithChangedLayerFirst
                    Analyze();
                }
            }
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
                        EditorGUI.BeginChangeCheck();
                        spriteData = EditorGUILayout.ObjectField(
                            new GUIContent("Sprite Data Asset", UITooltipConstants.SortingEditorSpriteDataAssetTooltip),
                            spriteData, typeof(SpriteData), false) as SpriteData;
                        if (EditorGUI.EndChangeCheck())
                        {
                            preview.UpdateSpriteData(spriteData);
                        }

                        if (GUILayout.Button("Open Sprite Analysis window", GUILayout.ExpandWidth(false)))
                        {
                            var spriteAlphaEditorWindow = GetWindow<SpriteDataEditorWindow>();
                            spriteAlphaEditorWindow.Show();
                        }
                    }

                    if (!isUsingSpriteData)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField(
                            new GUIContent("Sprite Data Asset is not used by Sprite Swapping Detector."));
                        EditorGUI.indentLevel--;
                    }
                    else if (spriteData == null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField(
                            new GUIContent("Please choose a Sprite Data Asset. It is used by " + errorMessage + ".",
                                Styling.WarnIcon));
                        isAnalyzedButtonDisabled = true;
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawSortingOptions()
        {
            GUILayout.Label("Sorting Options");
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(
                        new GUIContent("Sorting Type", UITooltipConstants.SortingEditorSortingTypeTooltip),
                        GUILayout.ExpandWidth(false));

                    GUILayout.Space(72.5f);

                    foreach (SortingType currentSortingType in SortingTypes)
                    {
                        EditorGUI.BeginChangeCheck();
                        GUILayout.Toggle(currentSortingType == sortingType,
                            ObjectNames.NicifyVariableName(currentSortingType.ToString()), Styling.ButtonStyle,
                            GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck())
                        {
                            sortingType = currentSortingType;
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
                            EditorGUI.indentLevel++;

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

                            EditorGUI.indentLevel--;
                        }

                        break;
                    case SortingType.Sprite:
                        var serializedSpriteRenderer = serializedObject.FindProperty(nameof(spriteRenderer));
                        EditorGUILayout.PropertyField(serializedSpriteRenderer,
                            new GUIContent(serializedSpriteRenderer.displayName,
                                UITooltipConstants.SortingEditorSingleSpriteRendererTooltip), true);

                        //TODO: will not work for prefab scene
                        if (serializedSpriteRenderer.objectReferenceValue == null ||
                            !((SpriteRenderer) serializedSpriteRenderer.objectReferenceValue).gameObject.scene.isLoaded)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField(new GUIContent(
                                "Please choose a SpriteRenderer within an opened scene.",
                                Styling.WarnIcon));
                            isAnalyzedButtonDisabled = true;
                            EditorGUI.indentLevel--;
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

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUIUtility.labelWidth = 350;
                    EditorGUILayout.LabelField(
                        new GUIContent("Transparency Sort Mode (Change via Project Settings):",
                            UITooltipConstants.SortingEditorTransparencySortModeTooltip),
                        new GUIContent(lastConfiguredTransparencySortMode.ToString()));
                    EditorGUIUtility.labelWidth = 0;
                }

                EditorGUI.indentLevel++;
                switch (lastConfiguredTransparencySortMode)
                {
                    case TransparencySortMode.Perspective:
                        cameraProjectionType = CameraProjectionType.Perspective;
                        break;
                    case TransparencySortMode.Orthographic:
                        cameraProjectionType = CameraProjectionType.Orthographic;
                        break;
                    case TransparencySortMode.CustomAxis:
                        // TODO add functionality for custom axis
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
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField(new GUIContent(
                            "Please choose a Camera. It is used to " + errorMessage + ".", Styling.WarnIcon,
                            UITooltipConstants.SortingEditorMissingCameraTooltip));
                        isAnalyzedButtonDisabled = true;
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        cameraProjectionType = ((Camera) cameraSerializedProp.objectReferenceValue).orthographic
                            ? CameraProjectionType.Orthographic
                            : CameraProjectionType.Perspective;
                    }
                }

                DrawOutlinePrecisionSlider(verticalScope.rect.width);

                EditorGUI.indentLevel--;
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
            EditorGUI.BeginChangeCheck();
            outlinePrecisionSliderValue = GUI.HorizontalSlider(sliderRect, outlinePrecisionSliderValue, 0,
                OutlinePrecisionTypes.Length - 1);
            if (EditorGUI.EndChangeCheck())
            {
                outlinePrecisionSliderValue = (int) Math.Round(outlinePrecisionSliderValue);
                outlinePrecision = (OutlinePrecision) outlinePrecisionSliderValue;
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

        private bool IsCameraRequired(out string usedBy)
        {
            usedBy = "";
            var isCameraRequired = false;

            if (GraphicsSettings.transparencySortMode == TransparencySortMode.Default)
            {
                usedBy = "identify unsorted SpriteRenderers";
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

        private void ApplySortingOptions()
        {
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
            preview.CleanUpPreview();
        }

        //TODO overlappingItems with changed but same layer are not considered
        private void ApplySortingOptionsIterative()
        {
            // Debug.Log("apply sorting options");

            var counter = 0;
            while (counter < overlappingItems.Items.Count)
            {
                var overlappingItem = overlappingItems.Items[counter];
                if (!overlappingItem.HasSortingLayerChanged())
                {
                    counter++;
                    continue;
                }

                overlappingItems.Items.RemoveAt(counter);
                overlappingItem.ApplySortingOption();
            }

            if (overlappingItems.Items.Count <= 0)
            {
                return;
            }

            FillSpriteDetectionData();

            if (overlappingSpriteDetector == null)
            {
                overlappingSpriteDetector = new OverlappingSpriteDetector();
            }

            var sortingOptions = overlappingSpriteDetector.AnalyzeSurroundingSpritesAndGetAdjustedSortingOptions(
                overlappingItems.Items, spriteDetectionData);

            foreach (var sortingOption in sortingOptions)
            {
                var sortingComponent = EditorUtility.InstanceIDToObject(sortingOption.Key);
                var sortingGroupComponent = sortingComponent as SortingGroup;
                if (sortingGroupComponent != null)
                {
                    if (sortingGroupComponent.sortingOrder == sortingOption.Value)
                    {
                        continue;
                    }

                    Debug.LogFormat("Update on Sorting Group {0} Sorting Order: {1} -> {2}",
                        sortingGroupComponent.name, sortingGroupComponent.sortingOrder, sortingOption.Value);

                    Undo.RecordObject(sortingGroupComponent, "apply sorting options");
                    sortingGroupComponent.sortingOrder = sortingOption.Value;
                    EditorUtility.SetDirty(sortingGroupComponent);
                    continue;
                }

                var spriteRendererComponent = sortingComponent as SpriteRenderer;
                if (spriteRendererComponent != null)
                {
                    if (spriteRendererComponent.sortingOrder == sortingOption.Value)
                    {
                        continue;
                    }

                    Debug.LogFormat("Update on SpriteRenderer {0} Sorting Order: {1} -> {2}",
                        spriteRendererComponent.name, spriteRendererComponent.sortingOrder, sortingOption.Value);

                    Undo.RecordObject(spriteRendererComponent, "apply sorting options");
                    spriteRendererComponent.sortingOrder = sortingOption.Value;
                    EditorUtility.SetDirty(spriteRendererComponent);
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

        private void EndScrollRect()
        {
            EditorGUILayout.EndScrollView();
        }

        private void CleanUpReordableList()
        {
            reordableOverlappingItemList?.CleanUp();
            autoSortingResultList = null;
        }

        private bool HasOverlappingItems()
        {
            return overlappingItems != null && overlappingItems.Items.Count > 0;
        }

        private void ShowSortingLayers()
        {
            if (selectedLayers == null)
            {
                SelectDefaultLayer();
            }

            EditorGUI.BeginChangeCheck();
            selectedSortingLayers = EditorGUILayout.MaskField(
                new GUIContent("Sorting Layers", UITooltipConstants.SortingEditorSortingLayerTooltip),
                selectedSortingLayers, SortingLayerUtility.SortingLayerNames);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSelectedLayers();
            }
        }

        private void UpdateSelectedLayers()
        {
            selectedLayers.Clear();

            for (int i = 0; i < SortingLayerUtility.SortingLayerNames.Length; i++)
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
            wasAnalyzeButtonClicked = true;

            var isVisualizingBoundsInScene = preview.IsVisualizingBoundsInScene;

            if (isVisualizingBoundsInScene)
            {
                preview.EnableSceneVisualization(false);
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
                return;
            }

            if (isVisualizingBoundsInScene)
            {
                preview.EnableSceneVisualization(true);
            }

            List<OverlappingItem> overlappingItemList;
            OverlappingItem overlappingBaseItem;

            var isApplyingAutoSortingForAnalysis = autoSortingOptionsUI.IsApplyingAutoSorting &&
                                                   autoSortingOptionsUI.HasActiveAutoSortingCriteria();
            if (isApplyingAutoSortingForAnalysis)
            {
                var sortingComponents =
                    new List<SortingComponent>(overlappingSpriteDetectionResult.overlappingSortingComponents);
                CreateAndInitOverlappingItemSortingOrderAnalyzer();
                FillAutoSortingCalculationData();

                var resultList = autoSortingGenerator.GenerateAutomaticSortingOrder(
                    overlappingSpriteDetectionResult.baseItem, sortingComponents, autoSortingCalculationData);

                if (isReplacingOverlappingItemsWithAutoSortedResult)
                {
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
                        overlappingItem.originAutoSortingOrder = resultList[i].sortingOrder;
                    }
                }
                else
                {
                    DrawAdditionalListOfAutoSortingResult(resultList);
                    ConvertToOverlappingItems(overlappingSpriteDetectionResult, out overlappingItemList,
                        out overlappingBaseItem);
                    overlappingItemList.Insert(0, overlappingBaseItem);
                }
            }
            else
            {
                ConvertToOverlappingItems(overlappingSpriteDetectionResult, out overlappingItemList,
                    out overlappingBaseItem);
                overlappingItemList.Insert(0, overlappingBaseItem);
            }

            overlappingItems = new OverlappingItems(overlappingBaseItem, overlappingItemList,
                autoSortingOptionsUI.IsApplyingAutoSorting && isReplacingOverlappingItemsWithAutoSortedResult);
            preview.UpdateOverlappingItems(overlappingItems);
            preview.UpdateSpriteData(spriteData);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
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
                overlappingItems.Add(new OverlappingItem(overlappingSortingComponent));
            }
        }

        private void DrawAdditionalListOfAutoSortingResult(List<AutoSortingComponent> resultList)
        {
            autoSortingResultNames = new List<string>();
            for (var i = 0; i < resultList.Count; i++)
            {
                var autoSortingComponent = resultList[i];
                autoSortingResultNames.Add((i + 1) + ". new sortingOrder: " + autoSortingComponent.sortingOrder + ", " +
                                           autoSortingComponent);
            }

            autoSortingResultList =
                new ReorderableList(autoSortingResultNames, typeof(string), false, false, false, false);
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


            // foreach (var sortingCriteriaComponent in automaticSortingOptionsUI.sortingCriteriaComponents)
            // {
            //     if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
            //     {
            //         continue;
            //     }
            //
            //     if (sortingCriteriaComponent.sortingCriterionData is ContainmentSortingCriterionData
            //         containmentSortingCriterionData)
            //     {
            //         autoSortingGenerator.SetContainmentCriterion(containmentSortingCriterionData,
            //             sortingCriteriaComponent.sortingCriterion);
            //         continue;
            //     }
            //
            //     if (!sortingCriteriaComponent.sortingCriterionData.isActive)
            //     {
            //         continue;
            //     }
            //
            //     autoSortingGenerator.AddSortingCriterion(sortingCriteriaComponent.sortingCriterion);
            // }
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
            preview.CleanUpPreview();
            PolygonColliderCacher.GetInstance().CleanUp();
            autoSortingOptionsUI.Cleanup();
        }
    }
}