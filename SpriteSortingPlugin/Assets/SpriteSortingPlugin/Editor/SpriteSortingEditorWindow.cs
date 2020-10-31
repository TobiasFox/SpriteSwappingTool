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
        [SerializeField] private SpriteAlphaData spriteAlphaData;
        [SerializeField] private OutlineType outlineType;
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

        private ReorderableList reordableListForSortingGroup;
        private List<OverlappingItem> itemsForSortingGroup;
        private bool isCreatingNewSortingGroup;

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
                spriteAlphaData =
                    AssetDatabase.LoadAssetAtPath<SpriteAlphaData>(AssetDatabase.GUIDToAssetPath(guids[0]));
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

                if (itemsForSortingGroup != null)
                {
                    InitReordableListForNewSortingGroup();
                }
            }

            preview.EnableSceneVisualization(true);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            serializedObject.Update();

            GUILayout.Label("Sprite Sorting", EditorStyles.boldLabel);
            useSpriteAlphaOutline = EditorGUILayout.BeginToggleGroup("use Sprite Alpha Outline", useSpriteAlphaOutline);

            if (useSpriteAlphaOutline)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                spriteAlphaData = EditorGUILayout.ObjectField(new GUIContent("Sprite Alpha Data Asset"),
                    spriteAlphaData, typeof(SpriteAlphaData), false) as SpriteAlphaData;
                if (EditorGUI.EndChangeCheck())
                {
                    preview.UpdateSpriteAlphaData(spriteAlphaData);

                    //TODO select default value
                    // foreach (var spriteDataItem in spriteAlphaData.spriteDataDictionary.Values)
                    // {
                    //     if (spriteDataItem.outlinePoints != null)
                    //     {
                    //         alphaAnalysisType = AlphaAnalysisType.Outline;
                    //     }
                    // }
                }

                if (GUILayout.Button("Open Sprite Alpha Editor Window to create the Data"))
                {
                    var spriteAlphaEditorWindow = GetWindow<SpriteAlphaEditorWindow>();
                    spriteAlphaEditorWindow.Show();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                outlineType = (OutlineType) EditorGUILayout.EnumPopup("Outline Type", outlineType);
                if (EditorGUI.EndChangeCheck())
                {
                    preview.UpdateOutlineType(outlineType);
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

            if (result.overlappingItems == null || (result.overlappingItems.Count <= 0 && itemsForSortingGroup == null))
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

            isCreatingNewSortingGroup =
                EditorGUILayout.Foldout(isCreatingNewSortingGroup, "Create new Sorting Group?", true);

            if (isCreatingNewSortingGroup)
            {
                EditorGUI.indentLevel++;
                var rectForReordableList =
                    EditorGUILayout.GetControlRect(false, reordableListForSortingGroup.GetHeight());
                reordableListForSortingGroup.DoList(new Rect(rectForReordableList.x + 12.5f, rectForReordableList.y,
                    rectForReordableList.width - 12.5f, rectForReordableList.height));

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.Space();
            }

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
                overlappingItems.Items, spriteAlphaData, outlineType);

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

            if (reordableListForSortingGroup == null)
            {
                return;
            }

            reordableListForSortingGroup.drawHeaderCallback = null;
            reordableListForSortingGroup.drawElementCallback = null;

            reordableListForSortingGroup = null;
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
                        gameObjectParents, spriteAlphaData, outlineType);
                    break;
                case SortingType.Sprite:
                    result = SpriteSortingUtility.AnalyzeSpriteSorting(cameraProjectionType, spriteRenderer,
                        spriteAlphaData, outlineType);
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
            preview.UpdateSpriteAlphaData(spriteAlphaData);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);

            if (result.overlappingItems.Count > 1)
            {
                InitReordableListForNewSortingGroup();
            }
        }

        private void InitReordableListForNewSortingGroup()
        {
            itemsForSortingGroup = new List<OverlappingItem>();
            reordableListForSortingGroup = new ReorderableList(itemsForSortingGroup,
                typeof(OverlappingItem), true, true, false, true)
            {
                drawHeaderCallback = DrawHeaderForNewSortingGroupCallback,
                drawElementCallback = DrawElementForNewSortingGroupCallback
            };
        }

        private void DrawElementForNewSortingGroupCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = itemsForSortingGroup[index];
            bool isPreviewUpdating = false;
            bool isCurrentIndexUpdated = false;
            rect.y += 2;

            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y, 90, EditorGUIUtility.singleLineHeight),
                element.originSpriteRenderer.name);

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.BeginChangeCheck();
            element.sortingLayerDropDownIndex =
                EditorGUI.Popup(new Rect(rect.x + 15 + 90 + 10, rect.y, 135, EditorGUIUtility.singleLineHeight),
                    "Layer", element.sortingLayerDropDownIndex, SortingLayerUtility.SortingLayerNames);

            if (EditorGUI.EndChangeCheck())
            {
                element.UpdatePreviewSortingLayer();
                isPreviewUpdating = true;
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            EditorGUI.BeginChangeCheck();
            element.sortingOrder =
                EditorGUI.DelayedIntField(
                    new Rect(rect.x + 15 + 90 + 10 + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "Order " + element.originSortingOrder + " +", element.sortingOrder);

            if (EditorGUI.EndChangeCheck())
            {
                isPreviewUpdating = true;
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                element.sortingOrder++;
                isPreviewUpdating = true;
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                element.sortingOrder--;
                isPreviewUpdating = true;
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10 + 25 + 10 + 25 + 10, rect.y, 55,
                    EditorGUIUtility.singleLineHeight), "Select"))
            {
                Selection.objects = new Object[] {element.originSpriteRenderer.gameObject};
                SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);
            }

            if (isPreviewUpdating)
            {
                if (!isCurrentIndexUpdated)
                {
                    reordableListForSortingGroup.index = index;
                }

                // OnSelectCallback(reordableSpriteSortingList);

                preview.UpdatePreviewEditor();
            }
        }

        private void DrawHeaderForNewSortingGroupCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Items For new Sorting Group");

            var hasElements = itemsForSortingGroup != null && itemsForSortingGroup.Count > 0;

            if (!hasElements)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            if (GUI.Button(new Rect(rect.width - 53, rect.y, 80, EditorGUIUtility.singleLineHeight), "Remove All"))
            {
            }

            if (!hasElements)
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        private void OnDisable()
        {
            preview.EnableSceneVisualization(false);

            CleanUpReordableList();
        }

        private void OnDestroy()
        {
            preview.CleanUpPreview();
        }
    }
}