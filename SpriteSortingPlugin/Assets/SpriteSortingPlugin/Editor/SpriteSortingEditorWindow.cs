using System;
using System.Collections.Generic;
using SpriteSortingPlugin.Preview;
using SpriteSortingPlugin.SpriteAlphaAnalysis;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
{
    public class SpriteSortingEditorWindow : EditorWindow
    {
        private const string SortingLayerNameDefault = "Default";

        private Vector2 scrollPosition = Vector2.zero;

        private bool useSpriteAlphaOutline = true;
        [SerializeField] private SpriteData spriteData;
        [SerializeField] private OutlinePrecision outlinePrecision;
        private CameraProjectionType cameraProjectionType;
        private SortingType sortingType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Transform> gameObjectParents;
        private SerializedObject serializedObject;

        private int selectedSortingLayers;
        private List<string> selectedLayers;
        private bool isGameObjectParentsExpanded;
        private bool isUsingGameObjectParents;

        private SpriteSortingAnalysisResult result;
        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;

        private bool analyzeButtonWasClicked;
        private ReordableOverlappingItemList reordableOverlappingItemList;

        private bool isAnalyzingWithChangedLayerFirst;

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
            SelectDefaultSpriteAlphaData();
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
            if (!analyzeButtonWasClicked || result.overlappingItems == null ||
                result.overlappingItems.Count <= 0)
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
                if (oldSelectedLayerNames.Contains(layerName))
                {
                    selectedLayers.Add(layerName);
                    selectedSortingLayers ^= 1 << i;
                }
            }
        }

        private void OnEnable()
        {
            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(this);
            }

            SortingLayerUtility.UpdateSortingLayerNames();

            if (analyzeButtonWasClicked)
            {
                if (result.overlappingItems != null && result.overlappingItems.Count > 0)
                {
                    reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
                }
            }

            preview.EnableSceneVisualization(true);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            serializedObject.Update();

            var style = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
            GUILayout.Label("Sprite Sorting", style, GUILayout.ExpandWidth(true));

            useSpriteAlphaOutline = EditorGUILayout.BeginToggleGroup(
                "use more precise sprite outline than the SpriteRenderers Bounding Box?", useSpriteAlphaOutline);

            if (useSpriteAlphaOutline)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                spriteData = EditorGUILayout.ObjectField(new GUIContent("Sprite Data Asset"),
                    spriteData, typeof(SpriteData), false) as SpriteData;
                if (EditorGUI.EndChangeCheck())
                {
                    preview.UpdateSpriteAlphaData(spriteData);

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

                EditorGUI.BeginChangeCheck();
                outlinePrecision = (OutlinePrecision) EditorGUILayout.EnumPopup("Outline Precision", outlinePrecision);
                if (EditorGUI.EndChangeCheck())
                {
                    preview.UpdateOutlineType(outlinePrecision);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndToggleGroup();

            cameraProjectionType =
                (CameraProjectionType) EditorGUILayout.EnumPopup("Projection type of camera", cameraProjectionType);
            sortingType = (SortingType) EditorGUILayout.EnumPopup("Sorting Type", sortingType);

            switch (sortingType)
            {
                case SortingType.Layer:
                    ShowSortingLayers();

                    isUsingGameObjectParents =
                        EditorGUILayout.BeginToggleGroup("use specific GameObject parents?", isUsingGameObjectParents);

                    if (isUsingGameObjectParents)
                    {
                        var gameObjectParentsSerializedProp = serializedObject.FindProperty(nameof(gameObjectParents));
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

                    //TODO: will not work for prefab scene
                    if (spriteRenderer != null && !spriteRenderer.gameObject.scene.isLoaded)
                    {
                        GUILayout.Label("Please choose a SpriteRenderer from an active Scene.");
                    }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            bool isAnalyzedButtonClickedThisFrame = false;

            if (GUILayout.Button("Analyze"))
            {
                Analyze();
                isAnalyzedButtonClickedThisFrame = true;
            }

            if (!analyzeButtonWasClicked)
            {
                EndScrollRect();
                return;
            }

            if (result.overlappingItems == null || (result.overlappingItems.Count <= 0))
                // if (result.overlappingItems == null || result.overlappingItems.Count <= 0)
            {
                GUILayout.Label(
                    "No sorting order issues with overlapping sprites were found in the currently loaded scenes.",
                    EditorStyles.boldLabel);
                CleanUpReordableList();
                preview.EnableSceneVisualization(false);

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

            var buttonLabel = sortingType == SortingType.Layer ? "Confirm and continue searching" : "confirm";
            if (GUILayout.Button(buttonLabel))
            {
                Debug.Log("sort sprites");

                ApplySortingOptions();

                analyzeButtonWasClicked = false;
                result.overlappingItems = null;
                preview.CleanUpPreview();

                EndScrollRect();

                if (sortingType == SortingType.Layer)
                {
                    //TODO: check isAnalyzingWithChangedLayerFirst
                    Analyze();
                }

                return;
            }

            preview.DoPreview(isAnalyzedButtonClickedThisFrame);

            EndScrollRect();
        }

        private void ApplySortingOptions()
        {
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

            var sortingOptions = SpriteSortingUtility.AnalyzeSurroundingSprites(cameraProjectionType,
                overlappingItems.Items, spriteData, outlinePrecision);

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
        }

        private void EndScrollRect()
        {
            EditorGUILayout.EndScrollView();
        }

        private void CleanUpReordableList()
        {
            reordableOverlappingItemList?.CleanUp();
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
                if (SortingLayerUtility.SortingLayerNames[i].Equals(SortingLayerNameDefault))
                {
                    defaultIndex = i;
                    break;
                }
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

            switch (sortingType)
            {
                case SortingType.Layer:

                    var selectedLayerIds = new List<int>();
                    foreach (var selectedLayer in selectedLayers)
                    {
                        selectedLayerIds.Add(SortingLayer.NameToID(selectedLayer));
                    }

                    result = SpriteSortingUtility.AnalyzeSpriteSorting(cameraProjectionType, selectedLayerIds,
                        gameObjectParents, spriteData, outlinePrecision);
                    break;
                case SortingType.Sprite:
                    result = SpriteSortingUtility.AnalyzeSpriteSorting(cameraProjectionType, spriteRenderer,
                        spriteData, outlinePrecision);
                    break;
            }

            if (isVisualizingBoundsInScene)
            {
                preview.EnableSceneVisualization(true);
            }

            if (result.overlappingItems == null || result.overlappingItems.Count <= 0)
            {
                return;
            }

            overlappingItems = new OverlappingItems(result.baseItem, result.overlappingItems);
            preview.UpdateOverlappingItems(overlappingItems);
            preview.UpdateSpriteAlphaData(spriteData);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);
        }

        private void OnDisable()
        {
            preview.EnableSceneVisualization(false);

            CleanUpReordableList();
        }

        private void OnDestroy()
        {
            preview.CleanUpPreview();
            SpriteSortingUtility.CleanUp();
        }
    }
}