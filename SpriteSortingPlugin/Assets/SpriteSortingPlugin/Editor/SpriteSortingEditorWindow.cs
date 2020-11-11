using System;
using System.Collections.Generic;
using SpriteSortingPlugin.AutomaticSorting;
using SpriteSortingPlugin.OverlappingSpriteDetection;
using SpriteSortingPlugin.OverlappingSprites;
using SpriteSortingPlugin.Preview;
using SpriteSortingPlugin.SpriteAlphaAnalysis;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    public class SpriteSortingEditorWindow : EditorWindow
    {
        private const string SortingLayerNameDefault = "Default";

        private static Texture warnIcon;
        private static bool isIconInitialized;

        private Vector2 scrollPosition = Vector2.zero;

        private bool useSpriteAlphaOutline = true;
        [SerializeField] private SpriteData spriteData;
        [SerializeField] private OutlinePrecision outlinePrecision;
        private CameraProjectionType cameraProjectionType;
        private SortingType sortingType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Transform> gameObjectParents;
        [SerializeField] private Camera camera;
        private bool isApplyingAutoSorting;
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
        private GUIStyle centeredStyle;
        private GUIStyle helpBoxStyle;

        private OverlappingSpriteDetector overlappingSpriteDetector;
        private SpriteDetectionData spriteDetectionData;

        private OverlappingItemSortingOrderAnalyzer overlappingItemSortingOrderAnalyzer;
        private bool useSizeAsSortingCriteria;
        private bool isLargeSpritesInForeground;
        private List<string> autoSortingResultNames;
        private ReorderableList autoSortingResultList;
        private List<SortingCriterionData> sortingCriterionDataList;
        private List<Editor> sortingCriterionDataEditorList;

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
            centeredStyle = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
            helpBoxStyle = new GUIStyle("HelpBox");

            //TODO: remove
            // SelectDefaultSpriteAlphaData();

            if (!isIconInitialized)
            {
                warnIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                isIconInitialized = true;
            }

            overlappingSpriteDetector = new OverlappingSpriteDetector();
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
            GUILayout.Label("Sprite Sorting", centeredStyle, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space();
            DrawGeneralOptions();

            EditorGUILayout.Space();
            DrawSortingOptions();
            EditorGUILayout.Space();
            isApplyingAutoSorting = EditorGUILayout.BeginToggleGroup(
                "generate automatic order of overlapping renderer?",
                isApplyingAutoSorting);

            //TODO add auto sorting dependent options
            if (isApplyingAutoSorting)
            {
                DrawAutoSortingOptions();
            }

            EditorGUILayout.EndToggleGroup();

            serializedObject.ApplyModifiedProperties();
            var isAnalyzedButtonClickedThisFrame = false;

            EditorGUI.BeginDisabledGroup(isAnalyzedButtonDisabled);
            if (GUILayout.Button("Analyze"))
            {
                Analyze();
                isAnalyzedButtonClickedThisFrame = true;
            }

            EditorGUI.EndDisabledGroup();

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

            EditorGUILayout.BeginHorizontal();
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
            EditorGUILayout.EndHorizontal();

            autoSortingResultList.DoLayoutList();

            if (isConfirmButtonClicked)
            {
                EndScrollRect();
                return;
            }

            preview.DoPreview(isAnalyzedButtonClickedThisFrame);

            EndScrollRect();
        }

        private void InitializeSortingCriteriaDataAndEditors()
        {
            sortingCriterionDataList = new List<SortingCriterionData>();
            sortingCriterionDataEditorList = new List<Editor>();

            var sizeSortingCriterionData = CreateInstance<SizeSortingCriterionData>();
            sortingCriterionDataList.Add(sizeSortingCriterionData);
            var testEditor = Editor.CreateEditor(sizeSortingCriterionData);
            var specificEditor = (SizeCriterionDataEditor) testEditor;
            specificEditor.Initialize(sizeSortingCriterionData);
            sortingCriterionDataEditorList.Add(testEditor);

            var sizeSortingCriterionData2 = CreateInstance<SizeSortingCriterionData>();
            sortingCriterionDataList.Add(sizeSortingCriterionData2);
            testEditor = Editor.CreateEditor(sizeSortingCriterionData2);
            specificEditor = (SizeCriterionDataEditor) testEditor;
            specificEditor.Initialize(sizeSortingCriterionData2);
            sortingCriterionDataEditorList.Add(testEditor);
        }

        private void DrawAutoSortingOptions()
        {
            if (sortingCriterionDataList == null)
            {
                InitializeSortingCriteriaDataAndEditors();
            }

            foreach (var editor in sortingCriterionDataEditorList)
            {
                editor.OnInspectorGUI();
            }
        }

        private void DrawSortingOptions()
        {
            GUILayout.Label("Sorting Options");
            EditorGUILayout.BeginVertical("HelpBox");
            {
                sortingType = (SortingType) EditorGUILayout.EnumPopup("Sorting Type", sortingType);

                switch (sortingType)
                {
                    case SortingType.Layer:
                        ShowSortingLayers();

                        isUsingGameObjectParents =
                            EditorGUILayout.BeginToggleGroup("use specific GameObject parents?",
                                isUsingGameObjectParents);

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

                        EditorGUILayout.EndToggleGroup();

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
                                warnIcon));
                            isAnalyzedButtonDisabled = true;
                            EditorGUI.indentLevel--;
                        }

                        break;
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneralOptions()
        {
            GUILayout.Label("General Options");
            EditorGUILayout.BeginVertical(helpBoxStyle);
            {
                var projectTransparencySortMode = GraphicsSettings.transparencySortMode;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUIUtility.labelWidth = 350;
                EditorGUILayout.LabelField("Transparency Sort Mode (Change via Project Settings):",
                    projectTransparencySortMode.ToString());
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.EndDisabledGroup();

                switch (projectTransparencySortMode)
                {
                    case TransparencySortMode.Default:
                        EditorGUI.indentLevel++;
                        var cameraSerializedProp = serializedObject.FindProperty(nameof(camera));

                        EditorGUILayout.PropertyField(cameraSerializedProp, new GUIContent("Camera"));

                        if (cameraSerializedProp.objectReferenceValue == null)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField(new GUIContent("Please choose a camera", warnIcon));
                            isAnalyzedButtonDisabled = true;
                            EditorGUI.indentLevel--;
                        }
                        else
                        {
                            cameraProjectionType = ((Camera) cameraSerializedProp.objectReferenceValue).orthographic
                                ? CameraProjectionType.Orthographic
                                : CameraProjectionType.Perspective;
                        }

                        EditorGUI.indentLevel--;

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

                useSpriteAlphaOutline = EditorGUILayout.BeginToggleGroup(
                    "Use a more precise sprite outline than the SpriteRenderers Bounding Box?", useSpriteAlphaOutline);

                if (useSpriteAlphaOutline)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginChangeCheck();
                    spriteData = EditorGUILayout.ObjectField(new GUIContent("Sprite Data Asset"),
                        spriteData, typeof(SpriteData), false) as SpriteData;
                    if (EditorGUI.EndChangeCheck())
                    {
                        preview.UpdateSpriteData(spriteData);

                        //TODO select default value
                        // foreach (var spriteDataItem in spriteAlphaData.spriteDataDictionary.Values)
                        // {
                        //     if (spriteDataItem.outlinePoints != null)
                        //     {
                        //         alphaAnalysisType = AlphaAnalysisType.Outline;
                        //     }
                        // }
                    }

                    if (GUILayout.Button("Open Sprite Data editor window to create the data"))
                    {
                        var spriteAlphaEditorWindow = GetWindow<SpriteDataEditorWindow>();
                        spriteAlphaEditorWindow.Show();
                    }

                    EditorGUILayout.EndHorizontal();

                    if (spriteData == null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField(new GUIContent("Please choose a Sprite Data Asset", warnIcon));
                        isAnalyzedButtonDisabled = true;
                        EditorGUI.indentLevel--;
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

                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndVertical();
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

            spriteDetectionData.outlinePrecision = outlinePrecision;
            spriteDetectionData.spriteData = spriteData;
            spriteDetectionData.cameraProjectionType = cameraProjectionType;

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

            spriteDetectionData.outlinePrecision = outlinePrecision;
            spriteDetectionData.spriteData = spriteData;
            spriteDetectionData.cameraProjectionType = cameraProjectionType;

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

            if (isApplyingAutoSorting)
            {
                var sortingComponents =
                    new List<SortingComponent>(overlappingSpriteDetectionResult.overlappingSortingComponents);
                ApplyAutoSorting(overlappingSpriteDetectionResult.baseItem, sortingComponents);
            }

            overlappingSpriteDetectionResult.ConvertToOverlappingItems(out var overlappingItemList,
                out var overlappingBaseItem);

            overlappingItemList.Insert(0, overlappingBaseItem);

            overlappingItems = new OverlappingItems(overlappingBaseItem, overlappingItemList);
            preview.UpdateOverlappingItems(overlappingItems);
            preview.UpdateSpriteData(spriteData);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
        }

        private void ApplyAutoSorting(SortingComponent baseItem, List<SortingComponent> sortingComponents)
        {
            overlappingItemSortingOrderAnalyzer = new OverlappingItemSortingOrderAnalyzer();

            spriteDetectionData.outlinePrecision = outlinePrecision;
            spriteDetectionData.spriteData = spriteData;
            spriteDetectionData.cameraProjectionType = cameraProjectionType;

            var resultList =
                overlappingItemSortingOrderAnalyzer.GenerateAutomaticSortingOrder(baseItem, sortingComponents,
                    spriteDetectionData);

            //TODO: temp, replace this by overwriting directly the overlappingItemList
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

        private void OnDisable()
        {
            preview.DisableSceneVisualizations();

            CleanUpReordableList();
        }

        private void OnDestroy()
        {
            preview.CleanUpPreview();
            PolygonColliderCacher.CleanUp();
        }
    }
}