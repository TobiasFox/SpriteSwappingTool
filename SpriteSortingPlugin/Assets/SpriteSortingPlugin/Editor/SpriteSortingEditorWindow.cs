using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
{
    public class SpriteSortingEditorWindow : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;

        private bool ignoreAlphaOfSprites;
        private CameraProjectionType cameraProjectionType;
        private SortingType sortingType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<Transform> gameObjectParents;
        private SerializedObject serializedObject;

        private int selectedSortingLayers;
        private List<int> selectedLayers;
        private bool isGameObjectParentsExpanded;
        private bool isUsingGameObjectParents;

        private SpriteSortingAnalysisResult result;
        private bool analyzeButtonWasClicked;
        private ReordableOverlappingItemList reordableOverlappingItemList;
        private bool isAnalyzingWithChangedLayerFirst;

        private ReorderableList reordableListForSortingGroup;
        private List<OverlappingItem> itemsForSortingGroup;
        private bool isCreatingNewSortingGroup;

        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;

        [MenuItem("Window/Sprite Sorting %q")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSortingEditorWindow>();
            window.titleContent = new GUIContent("Sprite Sorting");
            window.Show();
        }

        private void Awake()
        {
            preview = new SpriteSortingEditorPreview();
            reordableOverlappingItemList = new ReordableOverlappingItemList();
            // int i = 0;
        }

        private void OnEnable()
        {
            if (serializedObject == null)
            {
                serializedObject = new SerializedObject(this);
            }

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
            ignoreAlphaOfSprites = EditorGUILayout.Toggle("ignore Alpha Of Sprites", ignoreAlphaOfSprites);
            cameraProjectionType =
                (CameraProjectionType) EditorGUILayout.EnumPopup("Projection type of camera", cameraProjectionType);
            sortingType =
                (SortingType) EditorGUILayout.EnumPopup("Sorting Type", sortingType);

            switch (sortingType)
            {
                case SortingType.Layer:
                    ShowSortingLayers();

                    isUsingGameObjectParents =
                        EditorGUILayout.BeginToggleGroup("use specific GameObject parents?", isUsingGameObjectParents);

                    if (isUsingGameObjectParents)
                    {
                        var gameObjectParentsSerializedProp = serializedObject.FindProperty("gameObjectParents");
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
                    var serializedSpriteRenderer = serializedObject.FindProperty("spriteRenderer");
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

            if (sortingType != SortingType.Layer)
            {
                SortingLayerUtility.UpdateSortingLayerNames();
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
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            if (overlappingItems.HasChangedLayer)
            {
                isAnalyzingWithChangedLayerFirst = EditorGUILayout.ToggleLeft(
                    "Analyse Sprites / Sorting Groups with changed Layer first?", isAnalyzingWithChangedLayerFirst);
            }

            if (GUILayout.Button("Confirm and continue searching"))
            {
                Debug.Log("sort sprites");
                analyzeButtonWasClicked = false;
                result.overlappingItems = null;
                preview.CleanUpPreview();

                //TODO: check isAnalyzingWithChangedLayerFirst
                EndScrollRect();
                return;
            }

            preview.DoPreview(isAnalyzedButtonClickedThisFrame);

            EndScrollRect();
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
            SortingLayerUtility.UpdateSortingLayerNames();

            if (selectedLayers == null)
            {
                SelectDefaultLayer();
            }

            selectedSortingLayers =
                EditorGUILayout.MaskField("Sorting Layers", selectedSortingLayers,
                    SortingLayerUtility.SortingLayerNames);
        }

        private void SelectDefaultLayer()
        {
            int defaultIndex = 0;
            for (var i = 0; i < SortingLayerUtility.SortingLayerNames.Length; i++)
            {
                if (SortingLayerUtility.SortingLayerNames[i].Equals("Default"))
                {
                    defaultIndex = i;
                }
            }

            selectedSortingLayers = 1 << defaultIndex;
            selectedLayers = new List<int>();
        }

        private void Analyze()
        {
            switch (sortingType)
            {
                case SortingType.Layer:
                    UpdateSelectedLayers();
                    result = SpriteSortingUtility.AnalyzeSpriteSorting(cameraProjectionType, selectedLayers,
                        gameObjectParents);
                    break;
                case SortingType.Sprite:
                    result = SpriteSortingUtility.AnalyzeSpriteSorting(cameraProjectionType, spriteRenderer);
                    break;
            }

            if (result.overlappingItems == null || result.overlappingItems.Count <= 0)
            {
                return;
            }

            overlappingItems = new OverlappingItems(result.baseItem, result.overlappingItems);
            preview.UpdateOverlappingItems(overlappingItems);
            reordableOverlappingItemList.InitReordableList(overlappingItems, preview);

            if (result.overlappingItems.Count > 1)
            {
                InitReordableListForNewSortingGroup();
            }

            analyzeButtonWasClicked = true;
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
                    selectedLayers.Add(SortingLayer.NameToID(SortingLayerUtility.SortingLayerNames[i]));
                }
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
                // element.sortingLayerName = sortingLayerNames[element.sortingLayerDropDownIndex];
                element.UpdatePreviewSortingLayer();
                // Debug.Log("changed layer to " + element.tempSpriteRenderer.sortingLayerName);
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
                // Debug.Log("new order to " + element.tempSpriteRenderer.sortingOrder);
                isPreviewUpdating = true;
                // isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                element.sortingOrder++;
                isPreviewUpdating = true;
                // isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                element.sortingOrder--;
                isPreviewUpdating = true;
                // isCurrentIndexUpdated = UpdateSortingOrder(index, element);
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