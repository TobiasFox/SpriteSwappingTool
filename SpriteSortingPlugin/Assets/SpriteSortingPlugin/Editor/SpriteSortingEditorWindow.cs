using System;
using System.Collections.Generic;
using SpriteSortingPlugin.AutomaticSorting;
using SpriteSortingPlugin.AutomaticSorting.Data;
using SpriteSortingPlugin.AutomaticSorting.SortingPreset;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using SpriteSortingPlugin.OverlappingSprites;
using SpriteSortingPlugin.Preview;
using SpriteSortingPlugin.SpriteAnalysis;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    public class SpriteSortingEditorWindow : EditorWindow
    {
        private const string SortingLayerNameDefault = "Default";

        private Vector2 scrollPosition = Vector2.zero;

        [SerializeField] private SpriteData spriteData;
        [SerializeField] private OutlinePrecision outlinePrecision;
        private CameraProjectionType cameraProjectionType;
        private SortingType sortingType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Transform> gameObjectParents;
        [SerializeField] private Camera camera;
        [SerializeField] private bool isApplyingAutoSorting;
        private SerializedObject serializedObject;

        private int selectedSortingLayers;
        private List<string> selectedLayers;
        private bool isGameObjectParentsExpanded;
        private bool isUsingGameObjectParents;

        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;

        private bool analyzeButtonWasClicked;
        private bool isAnalyzedButtonDisabled;
        private ReordableOverlappingItemList reordableOverlappingItemList;

        private bool isAnalyzingWithChangedLayerFirst;

        private OverlappingSpriteDetector overlappingSpriteDetector;
        private SpriteDetectionData spriteDetectionData;

        private AutoSortingGenerator autoSortingGenerator;
        private List<string> autoSortingResultNames;
        private ReorderableList autoSortingResultList;
        private List<SortingCriteriaComponent> sortingCriteriaComponents;
        private AutoSortingCalculationData autoSortingCalculationData;
        private bool isReplacingOverlappingItemsWithAutoSortedResult = true;
        private SortingCriteriaPresetSelector sortingCriteriaPresetSelector;

        [MenuItem("Window/Sprite Sorting %q")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSortingEditorWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Sorting");
            preview = new SpriteSortingEditorPreview();
            reordableOverlappingItemList = new ReordableOverlappingItemList();
            SortingLayerUtility.UpdateSortingLayerNames();

            //TODO: remove
            // SelectDefaultSpriteAlphaData();

            overlappingSpriteDetector = new OverlappingSpriteDetector();

            sortingCriteriaPresetSelector = CreateInstance<SortingCriteriaPresetSelector>();
            sortingCriteriaPresetSelector.Init(this);
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
            if (!analyzeButtonWasClicked || !HasOverlappingItems())
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
            }

            SortingLayerUtility.UpdateSortingLayerNames();

            if (analyzeButtonWasClicked && HasOverlappingItems())
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
            GUILayout.Label("Sprite Sorting", Styling.CenteredStyle, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space();
            DrawCameraOptions();

            EditorGUILayout.Space();
            DrawSpriteDataAssetOptions();

            EditorGUILayout.Space();
            DrawSortingOptions();

            EditorGUILayout.Space();
            DrawAutoSortingOptions();

            serializedObject.ApplyModifiedProperties();
            var isAnalyzedButtonClickedThisFrame = false;

            using (new EditorGUI.DisabledScope(isAnalyzedButtonDisabled))
            {
                if (GUILayout.Button("Analyze"))
                {
                    Analyze();
                    isAnalyzedButtonClickedThisFrame = true;
                }
            }

            if (!analyzeButtonWasClicked)
            {
                EndScrollRect();
                return;
            }

            EditorGUILayout.Space();

            if (!HasOverlappingItems())
            {
                GUILayout.Label(
                    "No sorting order issues with overlapping sprites were found in the currently loaded scenes.",
                    EditorStyles.boldLabel);
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

            var isConfirmButtonClicked = false;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Confirm"))
                {
                    ApplySortingOptions();
                    isConfirmButtonClicked = true;
                }

                if (sortingType == SortingType.Layer && GUILayout.Button("Confirm and continue searching"))
                {
                    ApplySortingOptions();

                    //TODO: check isAnalyzingWithChangedLayerFirst
                    Analyze();

                    isConfirmButtonClicked = true;
                }
            }

            autoSortingResultList?.DoLayoutList();

            if (isConfirmButtonClicked)
            {
                EndScrollRect();
                return;
            }

            preview.DoPreview(isAnalyzedButtonClickedThisFrame);

            EndScrollRect();
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

            if (!isApplyingAutoSorting)
            {
                return isUsingSpriteData;
            }

            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList ||
                    !sortingCriteriaComponent.sortingCriterionData.isActive)
                {
                    continue;
                }

                if (sortingCriteriaComponent.sortingCriterion != null &&
                    sortingCriteriaComponent.sortingCriterion.IsUsingSpriteData())
                {
                    isUsingSpriteData = true;
                    if (usedBy.Length > 0)
                    {
                        usedBy += " and ";
                    }

                    usedBy += "Automatic Sorting";
                    break;
                }
            }

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
                        spriteData = EditorGUILayout.ObjectField(new GUIContent("Sprite Data Asset"),
                            spriteData, typeof(SpriteData), false) as SpriteData;
                        if (EditorGUI.EndChangeCheck())
                        {
                            preview.UpdateSpriteData(spriteData);
                        }

                        if (GUILayout.Button("Open Sprite Data editor window to create the data"))
                        {
                            var spriteAlphaEditorWindow = GetWindow<SpriteDataEditorWindow>();
                            spriteAlphaEditorWindow.Show();
                        }
                    }

                    if (!isUsingSpriteData)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField(
                            new GUIContent("Sprite Data Asset is not used by Sprite Sorting tool."));
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

        private void InitializeSortingCriteriaDataAndEditors()
        {
            sortingCriteriaComponents = new List<SortingCriteriaComponent>();

            foreach (SortingCriterionType sortingCriterionType in Enum.GetValues(typeof(SortingCriterionType)))
            {
                var sortingCriteriaComponent =
                    SortingCriteriaComponentFactory.CreateSortingCriteriaComponent(sortingCriterionType);
                sortingCriteriaComponents.Add(sortingCriteriaComponent);

                //default value
                if (sortingCriterionType == SortingCriterionType.Containment)
                {
                    sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;
                    sortingCriteriaComponent.sortingCriterionData.isActive = true;
                }
            }
        }

        private void DrawAutoSortingOptions()
        {
            GUILayout.Label("Automatic Sorting");
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                isApplyingAutoSorting = EditorGUILayout.Toggle("use automatic sorting?", isApplyingAutoSorting);

                if (!isApplyingAutoSorting)
                {
                    return;
                }

                EditorGUILayout.Space();

                if (sortingCriteriaComponents == null)
                {
                    InitializeSortingCriteriaDataAndEditors();
                }

                using (var headerScope = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.DrawRect(headerScope.rect, Styling.SortingCriteriaHeaderBackgroundLightColor);
                    GUILayout.Label("Sorting Criteria");

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Save or Load Preset", GUILayout.Width(135)))
                    {
                        sortingCriteriaPresetSelector.ShowPresetSelector();
                    }
                }

                DrawHorizontalLine(true);

                for (var i = 0; i < sortingCriteriaComponents.Count; i++)
                {
                    var sortingCriteriaComponent = sortingCriteriaComponents[i];
                    if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                    {
                        continue;
                    }

                    sortingCriteriaComponent.criterionDataBaseEditor.OnInspectorGUI();
                    if (sortingCriteriaComponents.Count > 0 && i < sortingCriteriaComponents.Count - 1)
                    {
                        DrawHorizontalLine();
                    }
                }

                DrawHorizontalLine(true);
                var isEverySortingCriteriaIsUsed = IsEverySortingCriteriaIsUsed();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(isEverySortingCriteriaIsUsed))
                    {
                        if (GUILayout.Button("Add Criterion", GUILayout.Width(103)))
                        {
                            DrawSortingCriteriaMenu();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(isEverySortingCriteriaIsUsed))
                    {
                        if (GUILayout.Button("All", GUILayout.Width(50)))
                        {
                            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
                            {
                                sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;
                                sortingCriteriaComponent.sortingCriterionData.isActive = true;
                            }
                        }
                    }

                    var isMinOneSortingCriteriaIsUsed = IsMinOneSortingCriteriaIsUsed();
                    using (new EditorGUI.DisabledScope(!isMinOneSortingCriteriaIsUsed))
                    {
                        if (GUILayout.Button("None", GUILayout.Width(50)))
                        {
                            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
                            {
                                sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = false;
                                sortingCriteriaComponent.sortingCriterionData.isActive = false;
                            }
                        }
                    }
                }
            }
        }

        private void DrawSortingCriteriaMenu()
        {
            var menu = new GenericMenu();

            for (var i = 0; i < sortingCriteriaComponents.Count; i++)
            {
                var sortingCriteriaComponent = sortingCriteriaComponents[i];
                if (sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    continue;
                }

                var content =
                    new GUIContent(sortingCriteriaComponent.criterionDataBaseEditor.GetTitleName());
                menu.AddItem(content, false, AddSortingCriteria, i);
            }

            menu.ShowAsContext();
        }

        private bool IsEverySortingCriteriaIsUsed()
        {
            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsMinOneSortingCriteriaIsUsed()
        {
            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddSortingCriteria(object userdata)
        {
            var index = (int) userdata;
            var sortingCriteriaComponent = sortingCriteriaComponents[index];
            sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;
            sortingCriteriaComponent.sortingCriterionData.isActive = true;
            sortingCriteriaComponents[index] = sortingCriteriaComponent;
        }

        private static void DrawHorizontalLine(bool isBig = false)
        {
            var rect = GUILayoutUtility.GetRect(1f, isBig ? 1.5f : 1f);

            // Splitter rect should be full-width
            // rect.xMin = 0f;
            // rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.DrawRect(rect, Styling.HorizontalLineColor);
        }

        private void DrawSortingOptions()
        {
            GUILayout.Label("Sorting Options");
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                sortingType = (SortingType) EditorGUILayout.EnumPopup("Sorting Type", sortingType);

                switch (sortingType)
                {
                    case SortingType.Layer:
                        ShowSortingLayers();

                        using (var gameObjectParentScope =
                            new EditorGUILayout.ToggleGroupScope("use specific GameObject parents?",
                                isUsingGameObjectParents))
                        {
                            isUsingGameObjectParents = gameObjectParentScope.enabled;

                            if (isUsingGameObjectParents)
                            {
                                var gameObjectParentsSerializedProp =
                                    serializedObject.FindProperty(nameof(gameObjectParents));
                                if (isGameObjectParentsExpanded)
                                {
                                    gameObjectParentsSerializedProp.isExpanded = true;
                                    isGameObjectParentsExpanded = false;
                                }

                                EditorGUILayout.PropertyField(gameObjectParentsSerializedProp, true);
                            }
                        }

                        break;
                    case SortingType.Sprite:
                        var serializedSpriteRenderer = serializedObject.FindProperty(nameof(spriteRenderer));
                        EditorGUILayout.PropertyField(serializedSpriteRenderer, true);

                        // //TODO: will not work for prefab scene
                        if (serializedSpriteRenderer.objectReferenceValue == null ||
                            !((SpriteRenderer) serializedSpriteRenderer.objectReferenceValue).gameObject.scene.isLoaded)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField(new GUIContent("Please choose a SpriteRenderer within the scene",
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
            GUILayout.Label("General Options");
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var projectTransparencySortMode = GraphicsSettings.transparencySortMode;

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUIUtility.labelWidth = 350;
                    EditorGUILayout.LabelField("Transparency Sort Mode (Change via Project Settings):",
                        projectTransparencySortMode.ToString());
                    EditorGUIUtility.labelWidth = 0;
                }

                switch (projectTransparencySortMode)
                {
                    case TransparencySortMode.Default:
                        EditorGUI.indentLevel++;
                        var cameraSerializedProp = serializedObject.FindProperty(nameof(camera));

                        EditorGUILayout.PropertyField(cameraSerializedProp, new GUIContent("Camera"));

                        if (cameraSerializedProp.objectReferenceValue == null)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField(new GUIContent("Please choose a camera", Styling.WarnIcon));
                            isAnalyzedButtonDisabled = true;
                            EditorGUI.indentLevel--;
                        }
                        else
                        {
                            cameraProjectionType = ((Camera) cameraSerializedProp.objectReferenceValue).orthographic
                                ? CameraProjectionType.Orthographic
                                : CameraProjectionType.Perspective;
                        }

                        break;
                    case TransparencySortMode.Perspective:
                        cameraProjectionType = CameraProjectionType.Perspective;
                        break;
                    case TransparencySortMode.Orthographic:
                        cameraProjectionType = CameraProjectionType.Orthographic;
                        break;
                    case TransparencySortMode.CustomAxis:
                        break;
                }

                EditorGUI.BeginChangeCheck();
                outlinePrecision =
                    (OutlinePrecision) EditorGUILayout.EnumPopup("Outline Precision", outlinePrecision);
                if (EditorGUI.EndChangeCheck())
                {
                    preview.UpdateOutlineType(outlinePrecision);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void ApplySortingOptions()
        {
            Debug.Log("apply sorting options");

            var itemCount = overlappingItems.Items.Count;
            for (var i = 0; i < itemCount; i++)
            {
                var overlappingItem = overlappingItems.Items[i];
                if (!overlappingItem.HasSortingLayerChanged())
                {
                    continue;
                }

                overlappingItems.Items.RemoveAt(i);
                overlappingItem.ApplySortingOption();
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
                    Debug.LogFormat("Update Sorting Order on Sorting Group {0} from {1} to {2}",
                        sortingGroupComponent.name, sortingGroupComponent.sortingOrder, sortingOption.Value);

                    Undo.RecordObject(sortingGroupComponent, "apply sorting options");
                    sortingGroupComponent.sortingOrder = sortingOption.Value;
                    EditorUtility.SetDirty(sortingGroupComponent);
                    continue;
                }

                var spriteRendererComponent = sortingComponent as SpriteRenderer;
                if (spriteRendererComponent != null)
                {
                    Debug.LogFormat("Update Sorting Order on SpriteRenderer {0} from {1} to {2}",
                        spriteRendererComponent.name, spriteRendererComponent.sortingOrder, sortingOption.Value);

                    Undo.RecordObject(spriteRendererComponent, "apply sorting options");
                    spriteRendererComponent.sortingOrder = sortingOption.Value;
                    EditorUtility.SetDirty(spriteRendererComponent);
                }
            }

            analyzeButtonWasClicked = false;
            overlappingItems = null;
            preview.CleanUpPreview();
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

            if (sortingCriteriaComponents != null)
            {
                foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
                {
                    DestroyImmediate(sortingCriteriaComponent.criterionDataBaseEditor);
                }
            }
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
            selectedSortingLayers = EditorGUILayout.MaskField("Sorting Layers", selectedSortingLayers,
                SortingLayerUtility.SortingLayerNames);
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
                if (!SortingLayerUtility.SortingLayerNames[i].Equals(SortingLayerNameDefault))
                {
                    continue;
                }

                defaultIndex = i;
                break;
            }

            selectedSortingLayers = 1 << defaultIndex;
            selectedLayers = new List<string> {SortingLayerNameDefault};
        }

        private void Analyze()
        {
            analyzeButtonWasClicked = true;

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

                    overlappingSpriteDetectionResult = overlappingSpriteDetector.DetectOverlappingSprites(
                        selectedLayerIds, gameObjectParents, spriteDetectionData);
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

            if (isApplyingAutoSorting)
            {
                var sortingComponents =
                    new List<SortingComponent>(overlappingSpriteDetectionResult.overlappingSortingComponents);
                CreateAndInitOverlappingItemSortingOrderAnalyzer();
                FillAutoSortingCalculationData();

                var resultList = autoSortingGenerator.GenerateAutomaticSortingOrder(
                    overlappingSpriteDetectionResult.baseItem, sortingComponents, autoSortingCalculationData);

                if (isReplacingOverlappingItemsWithAutoSortedResult)
                {
                    var sortingComponentList = new List<SortingComponent>(resultList);
                    overlappingSpriteDetectionResult.overlappingSortingComponents = sortingComponentList;

                    overlappingSpriteDetectionResult.ConvertToOverlappingItems(out overlappingItemList,
                        out overlappingBaseItem);

                    for (var i = 0; i < overlappingItemList.Count; i++)
                    {
                        var overlappingItem = overlappingItemList[i];
                        overlappingItem.AutoSortingOrder = resultList[i].sortingOrder;
                    }
                }
                else
                {
                    DrawAdditionalListOfAutoSortingResult(resultList);
                    overlappingSpriteDetectionResult.ConvertToOverlappingItems(out overlappingItemList,
                        out overlappingBaseItem);
                    overlappingItemList.Insert(0, overlappingBaseItem);
                }
            }
            else
            {
                overlappingSpriteDetectionResult.ConvertToOverlappingItems(out overlappingItemList,
                    out overlappingBaseItem);
                overlappingItemList.Insert(0, overlappingBaseItem);
            }

            overlappingItems = new OverlappingItems(overlappingBaseItem, overlappingItemList,
                isApplyingAutoSorting && isReplacingOverlappingItemsWithAutoSortedResult);
            preview.UpdateOverlappingItems(overlappingItems);
            preview.UpdateSpriteData(spriteData);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
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

            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    continue;
                }

                if (sortingCriteriaComponent.sortingCriterionData is ContainmentSortingCriterionData
                    containmentSortingCriterionData)
                {
                    autoSortingGenerator.SetContainmentCriteria(containmentSortingCriterionData,
                        sortingCriteriaComponent.sortingCriterion);
                    continue;
                }

                if (!sortingCriteriaComponent.sortingCriterionData.isActive)
                {
                    continue;
                }

                autoSortingGenerator.AddSortingCriteria(sortingCriteriaComponent.sortingCriterion);
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
            autoSortingCalculationData.cameraTransform = camera.transform;
        }

        private void OnDisable()
        {
            preview.DisableSceneVisualizations();

            CleanUpReordableList();
            DestroyImmediate(sortingCriteriaPresetSelector);
        }

        private void OnDestroy()
        {
            preview.CleanUpPreview();
            PolygonColliderCacher.GetInstance().CleanUp();
        }

        public SortingCriteriaPreset GenerateSortingCriteriaPreset()
        {
            var preset = CreateInstance<SortingCriteriaPreset>();
            preset.sortingCriterionData = new SortingCriterionData[sortingCriteriaComponents.Count];

            for (var i = 0; i < sortingCriteriaComponents.Count; i++)
            {
                var sortingCriteriaComponent = sortingCriteriaComponents[i];
                preset.sortingCriterionData[i] = sortingCriteriaComponent.sortingCriterionData.Copy();
            }

            return preset;
        }

        public void UpdateSortingCriteriaFromPreset(SortingCriteriaPreset preset)
        {
            for (var i = 0; i < preset.sortingCriterionData.Length; i++)
            {
                var sortingCriteriaComponent = sortingCriteriaComponents[i];
                sortingCriteriaComponent.sortingCriterionData = preset.sortingCriterionData[i];
                sortingCriteriaComponent.criterionDataBaseEditor.UpdateSortingCriterionData(sortingCriteriaComponent
                    .sortingCriterionData);
                sortingCriteriaComponents[i] = sortingCriteriaComponent;
            }
        }
    }
}