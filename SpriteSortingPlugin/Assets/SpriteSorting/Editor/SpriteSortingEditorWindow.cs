using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSorting
{
    public class SpriteSortingEditorWindow : EditorWindow
    {
        private const float LineSpacing = 1.5f;

        private Vector2 scrollPosition = Vector2.zero;

        private bool ignoreAlphaOfSprites;
        private CameraProjectionType cameraProjectionType;
        private SortingType sortingType;
        private SpriteRenderer spriteRenderer;
        private SortingGroup sortingGroup;

        private int selectedSortingLayers;
        private string[] sortingLayerNames;
        private List<int> selectedLayers;

        private SpriteSortingAnalysisResult result;
        private bool analyzeButtonWasClicked;
        private ReorderableList reordableSpriteSortingList;
        private bool hasChangedLayer;
        private Color bg1 = new Color(0.83f, 0.83f, 0.83f);
        private Color bg2 = new Color(0.76f, 0.76f, 0.76f);
        private Color activeColor = new Color(0.1f, 0.69f, 1f, 0.7f);
        private Color focussingColor = new Color(0.45f, 0.77f, 0.95f, 0.91f);
        private int lastFocussedIndex;
        private bool isAnalyzingWithChangedLayerFirst;

        private ReorderableList reordableListForSortingGroup;
        private List<OverlappingItem> itemsForSortingGroup;
        private bool isCreatingNewSortingGroup;

        private SpriteSortingEditorPreview preview;

        [MenuItem("Window/Sprite Sorting %q")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSortingEditorWindow>();
            window.titleContent = new GUIContent("Sprite Sorting");
            window.Init();
            window.Show();
        }

        private void Init()
        {
            int i = 0;
        }

        private void OnEnable()
        {
            if (analyzeButtonWasClicked)
            {
                if (result.overlappingItems != null && result.overlappingItems.Count > 0)
                {
                    InitReordableList();
                }

                if (itemsForSortingGroup != null)
                {
                    InitReordableListForNewSortingGroup();
                }
            }

            preview?.EnableSceneVisualization(true);
            // int i = 0;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

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

                    break;
                case SortingType.Sprite:
                    spriteRenderer = EditorGUILayout.ObjectField("Sprite", spriteRenderer, typeof(SpriteRenderer), true,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight)) as SpriteRenderer;

                    //TODO: will not work for prefab scene
                    if (spriteRenderer != null && !spriteRenderer.gameObject.scene.isLoaded)
                    {
                        GUILayout.Label("Please choose a SpriteRenderer from an active Scene.");
                    }

                    break;
                case SortingType.SortingGroup:
                    sortingGroup = EditorGUILayout.ObjectField("Sorting Group", sortingGroup, typeof(SortingGroup),
                        true, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as SortingGroup;
                    if (sortingGroup != null && !sortingGroup.gameObject.scene.isLoaded)
                    {
                        GUILayout.Label("Please choose a SortingGroup from an active Scene.");
                    }

                    break;
            }

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

                EndScrollRect();
                return;
            }

            if (sortingType != SortingType.Layer)
            {
                UpdateSortingLayerNames();
            }

            reordableSpriteSortingList.DoLayoutList();

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

            if (hasChangedLayer)
            {
                isAnalyzingWithChangedLayerFirst = EditorGUILayout.ToggleLeft(
                    "Analyse Sprites / Sorting Groups with changed Layer first?", isAnalyzingWithChangedLayerFirst);
            }

            if (GUILayout.Button("Confirm and continue searching"))
            {
                Debug.Log("sort sprites");
                analyzeButtonWasClicked = false;
                result.overlappingItems = null;
                preview?.CleanUpPreview();

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
            if (reordableSpriteSortingList == null)
            {
                return;
            }

            reordableSpriteSortingList.drawHeaderCallback = null;
            reordableSpriteSortingList.drawElementCallback = null;
            reordableSpriteSortingList.onSelectCallback = null;
            reordableSpriteSortingList.elementHeightCallback = null;
            reordableSpriteSortingList.drawElementBackgroundCallback = null;
            reordableSpriteSortingList.onReorderCallbackWithDetails = null;
            reordableSpriteSortingList = null;

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
            UpdateSortingLayerNames();

            selectedSortingLayers =
                EditorGUILayout.MaskField("Sorting Layers", selectedSortingLayers, sortingLayerNames);
        }

        private void UpdateSortingLayerNames()
        {
            sortingLayerNames = new string[SortingLayer.layers.Length];
            for (var i = 0; i < SortingLayer.layers.Length; i++)
            {
                sortingLayerNames[i] = SortingLayer.layers[i].name;
            }

            if (selectedLayers != null)
            {
                return;
            }

            int defaultIndex = 0;
            for (var i = 0; i < sortingLayerNames.Length; i++)
            {
                if (sortingLayerNames[i].Equals("Default"))
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
                    AnalyzeLayer();
                    break;
                case SortingType.Sprite:
                    AnalyzeSprite();
                    break;
                case SortingType.SortingGroup:
                    AnalyzeSortingGroup();
                    break;
            }

            analyzeButtonWasClicked = true;
        }

        private void AnalyzeSprite()
        {
        }

        private void AnalyzeSortingGroup()
        {
        }

        private void UpdateSelectedLayers()
        {
            selectedLayers.Clear();

            for (int i = 0; i < sortingLayerNames.Length; i++)
            {
                //bitmask moving check if bit is set
                var layer = 1 << i;
                if ((selectedSortingLayers & layer) != 0)
                {
                    selectedLayers.Add(SortingLayer.NameToID(sortingLayerNames[i]));
                }
            }
        }

        private void AnalyzeLayer()
        {
            UpdateSelectedLayers();

            result = SpriteSortingUtility.AnalyzeSpriteSorting(new SpriteSortingData
                {selectedLayers = selectedLayers, cameraProjectionType = cameraProjectionType});

            if (result.overlappingItems == null || result.overlappingItems.Count <= 0)
            {
                return;
            }

            InitOverlappingItems(false);

            InitReordableList();

            if (preview == null)
            {
                preview = new SpriteSortingEditorPreview();
            }

            preview.UpdateOverlappingItems(result);

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
                    "Layer",
                    element.sortingLayerDropDownIndex, sortingLayerNames);

            if (EditorGUI.EndChangeCheck())
            {
                element.sortingLayerName = sortingLayerNames[element.sortingLayerDropDownIndex];
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
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                element.sortingOrder++;
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 15 + 90 + 10 + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                element.sortingOrder--;
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
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
                    reordableSpriteSortingList.index = index;
                }

                OnSelectCallback(reordableSpriteSortingList);

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

        private void InitReordableList()
        {
            reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
                typeof(OverlappingItem), true, true, false, false)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
                onSelectCallback = OnSelectCallback,
                elementHeightCallback = ElementHeightCallback,
                drawElementBackgroundCallback = DrawElementBackgroundCallback,
                onReorderCallbackWithDetails = OnReorderCallbackWithDetails
            };

            reordableSpriteSortingList.index = lastFocussedIndex;


            // reordableSpriteSortingList.onMouseUpCallback = (ReorderableList list) =>
            // {
            //     Debug.Log("mouse up reordable list");
            //     //s
            // };
            // //
            // reordableSpriteSortingList.onMouseDragCallback = (ReorderableList list) =>
            // {
            //     Debug.Log("mouse drag");
            //     //s
            // };
            //
            // reordableSpriteSortingList.onChangedCallback = (ReorderableList list) =>
            // {
            //     Debug.Log("changed reordable list");
            //     //d 
            // };
            //
            // reordableSpriteSortingList.onReorderCallback = (ReorderableList list) =>
            // {
            //     Debug.Log("reorder");
            //     //d
            // };
        }

        private void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            var itemWithNewIndex = result.overlappingItems[newIndex];

            if (oldIndex + 1 == newIndex || oldIndex - 1 == newIndex)
            {
                var itemWithOldIndex = result.overlappingItems[oldIndex];

                var tempSortingOrder = itemWithNewIndex.sortingOrder;
                var tempLayerId = itemWithNewIndex.sortingLayerDropDownIndex;

                itemWithNewIndex.sortingOrder = itemWithOldIndex.sortingOrder;
                itemWithNewIndex.sortingLayerDropDownIndex = itemWithOldIndex.sortingLayerDropDownIndex;

                itemWithOldIndex.sortingOrder = tempSortingOrder;
                itemWithOldIndex.sortingLayerDropDownIndex = tempLayerId;
                return;
            }

            var isAdjustingSortingOrderUpwards = newIndex <= result.overlappingItems.Count / 2;
            var lastItem = result.overlappingItems[newIndex + (isAdjustingSortingOrderUpwards ? 1 : -1)];

            if (isAdjustingSortingOrderUpwards)
            {
                if (itemWithNewIndex.sortingOrder <= lastItem.sortingOrder)
                {
                    itemWithNewIndex.sortingOrder = lastItem.sortingOrder + 1;
                }

                for (var i = newIndex - 1; i >= 0; i--)
                {
                    var previousItem = result.overlappingItems[i + 1];
                    var currentItem = result.overlappingItems[i];

                    if (previousItem.sortingOrder == currentItem.sortingOrder)
                    {
                        currentItem.sortingOrder++;
                    }
                }

                return;
            }

            if (itemWithNewIndex.sortingOrder >= lastItem.sortingOrder)
            {
                itemWithNewIndex.sortingOrder = lastItem.sortingOrder - 1;
            }

            for (var i = newIndex + 1; i < result.overlappingItems.Count; i++)
            {
                var previousItem = result.overlappingItems[i - 1];
                var currentItem = result.overlappingItems[i];

                if (previousItem.sortingOrder == currentItem.sortingOrder)
                {
                    currentItem.sortingOrder--;
                }
            }
        }

        //TODO: remember last focussed element before recompilation is active
        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            Color color;
            if (isActive)
            {
                color = activeColor;
            }
            else if (isFocused)
            {
                color = focussingColor;
            }
            else
            {
                color = index % 2 == 0 ? bg1 : bg2;
            }

            EditorGUI.DrawRect(rect, color);
        }

        private void InitOverlappingItems(bool isReset)
        {
            int sortingLayerIndex = GetLayerNameIndex(result.overlappingItems[0].originSortingLayer);

            for (var i = 0; i < result.overlappingItems.Count; i++)
            {
                var overlappingItem = result.overlappingItems[i];
                overlappingItem.sortingLayerDropDownIndex = sortingLayerIndex;

                if (!isReset)
                {
                    overlappingItem.OriginSortedIndex = i;
                }

                overlappingItem.sortingOrder = result.overlappingItems.Count - (i + 1);

                overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();
                overlappingItem.sortingLayerName = sortingLayerNames[overlappingItem.sortingLayerDropDownIndex];
                overlappingItem.UpdatePreviewSortingLayer();
            }

            if (!isReset)
            {
                lastFocussedIndex = -1;
            }
        }

        private void OnSelectCallback(ReorderableList list)
        {
            for (var i = 0; i < list.count; i++)
            {
                var item = (OverlappingItem) list.list[i];
                item.IsItemSelected = i == list.index;
            }

            lastFocussedIndex = list.index;
        }

        //TODO: adjust height when dragging elements
        private float ElementHeightCallback(int index)
        {
            var element = result.overlappingItems[index];
            if (element.originSortingGroup == null)
            {
                return EditorGUIUtility.singleLineHeight * 2 + LineSpacing + LineSpacing * 3;
            }

            return EditorGUIUtility.singleLineHeight * 3 + 2 * LineSpacing + LineSpacing * 3;
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = result.overlappingItems[index];
            bool isPreviewUpdating = false;
            bool isCurrentIndexUpdated = false;
            rect.y += 2;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 90, EditorGUIUtility.singleLineHeight),
                "\"" + element.originSpriteRenderer.name + "\"");

            if (GUI.Button(
                new Rect(rect.width - 28, rect.y, 55,
                    EditorGUIUtility.singleLineHeight), "Select"))
            {
                Selection.objects = new Object[] {element.originSpriteRenderer.gameObject};
                SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);
            }

            if (element.originSortingGroup != null)
            {
                rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 140, EditorGUIUtility.singleLineHeight),
                    "in outmost Sorting Group");

                EditorGUI.LabelField(
                    new Rect(rect.x + 140 + 2.5f, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "\"" + element.originSortingGroup.name + "\"");

                if (GUI.Button(
                    new Rect(rect.width - 56, rect.y, 83,
                        EditorGUIUtility.singleLineHeight), "Select Group"))
                {
                    Selection.objects = new Object[] {element.originSortingGroup.gameObject};
                    SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);
                }
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.BeginChangeCheck();
            element.sortingLayerDropDownIndex =
                EditorGUI.Popup(new Rect(rect.x, rect.y, 135, EditorGUIUtility.singleLineHeight), "Layer",
                    element.sortingLayerDropDownIndex, sortingLayerNames);

            if (EditorGUI.EndChangeCheck())
            {
                element.sortingLayerName = sortingLayerNames[element.sortingLayerDropDownIndex];
                element.UpdatePreviewSortingLayer();
                // Debug.Log("changed layer to " + element.tempSpriteRenderer.sortingLayerName);
                isPreviewUpdating = true;

                CheckChangedLayers();
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            EditorGUI.BeginChangeCheck();
            element.sortingOrder =
                EditorGUI.DelayedIntField(
                    new Rect(rect.x + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "Order " + element.originSortingOrder + " +", element.sortingOrder);

            if (EditorGUI.EndChangeCheck())
            {
                // Debug.Log("new order to " + element.tempSpriteRenderer.sortingOrder);
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                element.sortingOrder++;
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                element.sortingOrder--;
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (isPreviewUpdating)
            {
                if (!isCurrentIndexUpdated)
                {
                    reordableSpriteSortingList.index = index;
                }

                OnSelectCallback(reordableSpriteSortingList);

                preview.UpdatePreviewEditor();
            }
        }

        private void CheckChangedLayers()
        {
            foreach (var item in result.overlappingItems)
            {
                if (item.originSortingLayer == SortingLayer.NameToID(sortingLayerNames[item.sortingLayerDropDownIndex]))
                {
                    continue;
                }

                hasChangedLayer = true;
                return;
            }

            hasChangedLayer = false;
        }

        private bool UpdateSortingOrder(int currentIndex, OverlappingItem element)
        {
            if (currentIndex < 0 || element == null)
            {
                return false;
            }

            element.UpdatePreviewSortingOrderWithExistingOrder();

            //TODO: update other elements sorting order when one is updated, e.g. when the -1 button is pressed
            var index = currentIndex;
            var indexToSwitch = GetIndexToSwitch(currentIndex);
            if (indexToSwitch >= 0)
            {
                var tempItem = result.overlappingItems[currentIndex];
                result.overlappingItems.RemoveAt(currentIndex);
                result.overlappingItems.Insert(indexToSwitch, tempItem);
                Debug.Log("switch " + currentIndex + " with " + indexToSwitch);
                index = indexToSwitch;
            }

            reordableSpriteSortingList.index = index;
            var itemsHaveChanges = UpdateSurroundingItems(index);

            return indexToSwitch >= 0 || itemsHaveChanges;
        }

        private bool UpdateSurroundingItems(int currentIndex)
        {
            var itemsHaveChanges = false;

            //not called/used in current implementation
            for (var i = currentIndex - 1; i >= 0; i--)
            {
                var previousItem = result.overlappingItems[i + 1];
                var currentItem = result.overlappingItems[i];

                if (previousItem.sortingOrder == currentItem.sortingOrder)
                {
                    currentItem.sortingOrder++;
                    itemsHaveChanges = true;
                }
                else
                {
                    break;
                }
            }

            for (var i = currentIndex + 1; i < result.overlappingItems.Count; i++)
            {
                var previousItem = result.overlappingItems[i - 1];
                var currentItem = result.overlappingItems[i];

                if (previousItem.sortingOrder == currentItem.sortingOrder)
                {
                    currentItem.sortingOrder--;
                    itemsHaveChanges = true;
                }
                else
                {
                    break;
                }
            }

            return itemsHaveChanges;
        }

        private int GetIndexToSwitch(int currentIndex)
        {
            int newSortingOrder = result.overlappingItems[currentIndex].sortingOrder;
            int itemsCount = result.overlappingItems.Count;

            if (currentIndex > 0 && result.overlappingItems[currentIndex - 1].sortingOrder <= newSortingOrder)
            {
                int tempIndex = currentIndex;
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    int order = result.overlappingItems[i].sortingOrder;
                    if (newSortingOrder >= order)
                    {
                        tempIndex--;
                    }
                    else
                    {
                        break;
                    }
                }

                return tempIndex;
            }

            if (currentIndex + 1 < itemsCount &&
                result.overlappingItems[currentIndex + 1].sortingOrder > newSortingOrder)
            {
                int tempIndex = currentIndex;

                for (int i = currentIndex + 1; i < itemsCount; i++)
                {
                    int order = result.overlappingItems[i].sortingOrder;
                    if (newSortingOrder < order)
                    {
                        tempIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                return tempIndex;
            }

            return -1;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Overlapping Items");

            if (GUI.Button(new Rect(rect.width - 35, rect.y, 45, EditorGUIUtility.singleLineHeight), "Reset"))
            {
                result.overlappingItems.Sort((item1, item2) =>
                    item1.OriginSortedIndex.CompareTo(item2.OriginSortedIndex));

                InitOverlappingItems(true);

                preview.UpdatePreviewEditor();
                Repaint();
            }
        }

        private int GetLayerNameIndex(int layerId)
        {
            var layerNameToFind = SortingLayer.IDToName(layerId);
            for (var i = 0; i < sortingLayerNames.Length; i++)
            {
                if (sortingLayerNames[i].Equals(layerNameToFind))
                {
                    return i;
                }
            }

            return 0;
        }

        private void OnDisable()
        {
            preview?.EnableSceneVisualization(false);

            CleanUpReordableList();
        }

        private void OnDestroy()
        {
            preview?.CleanUpPreview();

// DestroyImmediate(reordableSO);
        }
    }
}