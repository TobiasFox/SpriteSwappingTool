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

        // private SerializedObject serializedResult;
        // private SpriteSortingReordableList reordableSO;

        private bool isPreviewVisible = true;
        private GameObject previewGameObject;
        private Editor previewEditor;

        [MenuItem("Window/Sprite Sorting")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteSortingEditorWindow>();
            window.Init();
            window.Show();
        }

        private void Init()
        {
            // reordableSO = CreateInstance<SpriteSortingReordableList>();
            // serializedResult = new SerializedObject(reordableSO);
        }

        private void OnEnable()
        {
            // SceneView.duringSceneGui += OnSceneGUI;
        }

        //TODO: add with OnFocus and OnFocusLeft
        // private void OnSceneGUI(SceneView sceneView)
        // {
        //     if (result.overlappingItems == null)
        //     {
        //         return;
        //     }
        //
        //     foreach (ReordableSpriteSortingItem item in reordableSpriteSortingList.list)
        //     {
        //         Handles.color = item.IsItemSelected ? Color.yellow : Color.red;
        //         var bounds = item.originSpriteRenderer.bounds;
        //         //TODO: consider rotated bounds
        //         Handles.DrawWireCube(bounds.center, new Vector3(bounds.size.x, bounds.size.y, 0));
        //     }
        //
        //     if (reordableSpriteSortingList.count > 0)
        //     {
        //         sceneView.Repaint();
        //     }
        // }

        private int test;

        private void OnGUI()
        {
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
                return;
            }

            if (result.overlappingItems == null || result.overlappingItems.Count <= 0)
            {
                GUILayout.Label(
                    "No sorting order issues with overlapping sprites were found in the currently loaded scenes.",
                    EditorStyles.boldLabel);
                CleanUpReordableList();

                // serializedResult = null;
                return;
            }

            // serializedResult.Update();
            reordableSpriteSortingList.DoLayoutList();
            // serializedResult.ApplyModifiedProperties();

            if (GUILayout.Button("Confirm and continue searching"))
            {
                Debug.Log("sort sprites");
                analyzeButtonWasClicked = false;
                result.overlappingItems = null;
                CleanUpPreview();
                return;
            }

            CreatePreview(isAnalyzedButtonClickedThisFrame);
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
            reordableSpriteSortingList = null;
        }

        private void CreatePreview(bool isUpdatePreview)
        {
            isPreviewVisible = EditorGUILayout.Foldout(isPreviewVisible, "Preview", true);

            if (!isPreviewVisible)
            {
                return;
            }

            if (isUpdatePreview)
            {
                CleanUpPreview();
            }

            if (previewGameObject == null)
            {
                GeneratePreviewGameObject();
            }

            if (previewEditor == null)
            {
                UpdatePreviewEditor();
            }

            if (GUILayout.Button("Reset rotation"))
            {
                previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);
                UpdatePreviewEditor();
            }

            //hack for not seeing the previewGameObject in the scene view 
            previewGameObject.SetActive(true);
            var bgColor = new GUIStyle {normal = {background = EditorGUIUtility.whiteTexture}};
            previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(position.width, 256), bgColor);
            previewGameObject.SetActive(false);
        }

        private void UpdatePreviewEditor()
        {
            if (!isPreviewVisible)
            {
                return;
            }

            DestroyImmediate(previewEditor);
            previewEditor = Editor.CreateEditor(previewGameObject);
        }

        private void CleanUpPreview()
        {
            if (previewGameObject != null)
            {
                var transformChildCount = previewGameObject.transform.childCount;
                for (int i = 0; i < transformChildCount; i++)
                {
                    var transform = previewGameObject.transform.GetChild(0);
                    if (transform != null)
                    {
                        DestroyImmediate(transform.gameObject);
                    }
                }

                DestroyImmediate(previewGameObject);
                previewGameObject = null;
            }

            previewEditor = null;
        }

        private void GeneratePreviewGameObject()
        {
            previewGameObject = new GameObject();
            previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);

            foreach (var overlappingItem in result.overlappingItems)
            {
                var spriteGameObject = new GameObject(overlappingItem.originSpriteRenderer.name);
                ComponentUtility.CopyComponent(overlappingItem.originSpriteRenderer.transform);
                ComponentUtility.PasteComponentValues(spriteGameObject.transform);

                //TODO: conside SortingOrder and SpriteRenderer components

                if (overlappingItem.originSpriteRenderer != null)
                {
                    ComponentUtility.CopyComponent(overlappingItem.originSpriteRenderer);
                    ComponentUtility.PasteComponentAsNew(spriteGameObject);
                    overlappingItem.tempSpriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();
                    overlappingItem.tempSpriteRenderer.sortingOrder = overlappingItem.sortingOrder;
                }

                if (overlappingItem.originSortingGroup != null)
                {
                    ComponentUtility.CopyComponent(overlappingItem.originSortingGroup);
                    ComponentUtility.PasteComponentAsNew(spriteGameObject);
                    overlappingItem.tempSortingGroup = spriteGameObject.GetComponent<SortingGroup>();
                }

                spriteGameObject.transform.SetParent(previewGameObject.transform);
                spriteGameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            previewGameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        private void ShowSortingLayers()
        {
            sortingLayerNames = new string[SortingLayer.layers.Length];
            for (var i = 0; i < SortingLayer.layers.Length; i++)
            {
                sortingLayerNames[i] = SortingLayer.layers[i].name;
            }

            if (selectedLayers == null)
            {
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

            selectedSortingLayers =
                EditorGUILayout.MaskField("Sorting Layers", selectedSortingLayers, sortingLayerNames);
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

            reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
                typeof(ReordableSpriteSortingItem), true, true, false, false);
            // reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
            // typeof(SpriteSortingReordableList.ReordableSpriteSortingItem), true, true, false, false);

            reordableSpriteSortingList.drawHeaderCallback = DrawHeaderCallback;
            reordableSpriteSortingList.drawElementCallback = DrawElementCallback;
            reordableSpriteSortingList.onSelectCallback = OnSelectCallback;

            // reordableSO.reordableSpriteSortingItems = result.overlappingItems;
            // serializedResult = new SerializedObject(reordableSO);
            // Repaint();

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
            //
            // reordableSpriteSortingList.onReorderCallbackWithDetails =
            //     (ReorderableList list, int previousIndex, int newIndex) =>
            //     {
            //         Debug.Log("reorder from "+previousIndex +" to "+newIndex);
            //         //d
            //     };
        }

        private void InitOverlappingItems(bool isReset)
        {
            int sortingLayerIndex = GetLayerNameIndex(result.overlappingItems[0].originSortingLayer);

            for (var i = 0; i < result.overlappingItems.Count; i++)
            {
                var overlappingItem = result.overlappingItems[i];
                overlappingItem.sortingLayer = sortingLayerIndex;

                if (!isReset)
                {
                    overlappingItem.OriginSortedIndex = i;
                }

                overlappingItem.sortingOrder = result.overlappingItems.Count - (i + 1);

                if (overlappingItem.tempSpriteRenderer != null)
                {
                    overlappingItem.tempSpriteRenderer.sortingOrder = overlappingItem.sortingOrder;
                    overlappingItem.tempSpriteRenderer.sortingLayerID =
                        overlappingItem.originSpriteRenderer.sortingLayerID;
                }

                if (overlappingItem.tempSortingGroup != null)
                {
                    overlappingItem.tempSortingGroup.sortingOrder = overlappingItem.sortingOrder;
                    overlappingItem.tempSpriteRenderer.sortingLayerID =
                        overlappingItem.originSortingGroup.sortingLayerID;
                }
            }
        }

        private void OnSelectCallback(ReorderableList list)
        {
            // Debug.Log("on select");
            for (var i = 0; i < list.count; i++)
            {
                var item = (ReordableSpriteSortingItem) list.list[i];
                item.IsItemSelected = i == list.index;
            }
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = result.overlappingItems[index];
            bool isPreviewUpdating = false;
            bool isCurrentIndexUpdated = false;
            rect.y += 2;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 90, EditorGUIUtility.singleLineHeight),
                element.originSpriteRenderer.name);

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.BeginChangeCheck();
            element.sortingLayer =
                EditorGUI.Popup(new Rect(rect.x + 90 + 10, rect.y, 135, EditorGUIUtility.singleLineHeight), "Layer",
                    element.sortingLayer, sortingLayerNames);

            if (EditorGUI.EndChangeCheck())
            {
                element.tempSpriteRenderer.sortingLayerName = sortingLayerNames[element.sortingLayer];
                // Debug.Log("changed layer to " + element.tempSpriteRenderer.sortingLayerName);
                isPreviewUpdating = true;
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            EditorGUI.BeginChangeCheck();
            element.sortingOrder =
                EditorGUI.DelayedIntField(
                    new Rect(rect.x + 90 + 10 + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "Order " + element.originSortingOrder + " +", element.sortingOrder);

            if (EditorGUI.EndChangeCheck())
            {
                // Debug.Log("new order to " + element.tempSpriteRenderer.sortingOrder);
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 90 + 10 + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                element.sortingOrder++;
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 90 + 10 + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                element.sortingOrder--;
                isPreviewUpdating = true;
                isCurrentIndexUpdated = UpdateSortingOrder(index, element);
            }

            if (GUI.Button(
                new Rect(rect.x + 90 + 10 + 135 + 10 + 120 + 10 + 25 + 10 + 25 + 10, rect.y, 55,
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

                UpdatePreviewEditor();
            }
        }

        private bool UpdateSortingOrder(int currentIndex, ReordableSpriteSortingItem element)
        {
            if (currentIndex < 0 || element == null)
            {
                return false;
            }

            element.tempSpriteRenderer.sortingOrder = element.originSortingOrder + element.sortingOrder;

            var indexToSwitch = GetIndexToSwitch(currentIndex);
            if (indexToSwitch < 0)
            {
                return false;
            }

            var tempItem = result.overlappingItems[currentIndex];
            result.overlappingItems.RemoveAt(currentIndex);
            result.overlappingItems.Insert(indexToSwitch, tempItem);
            reordableSpriteSortingList.list = result.overlappingItems;
            Debug.Log("switch " + currentIndex + " with " + indexToSwitch);
            reordableSpriteSortingList.index = indexToSwitch;

            return true;
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

                reordableSpriteSortingList.list = result.overlappingItems;
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
            CleanUpPreview();
            CleanUpReordableList();
            // SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnDestroy()
        {
// DestroyImmediate(reordableSO);
        }
    }
}