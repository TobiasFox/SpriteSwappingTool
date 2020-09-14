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
        }

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
                reordableSpriteSortingList = null;
                // serializedResult = null;
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            foreach (var overlappingRenderer in result.overlappingItems)
            {
                if (overlappingRenderer.originSpriteRenderer != null)
                {
                    EditorGUILayout.ObjectField("Sprite", overlappingRenderer.originSpriteRenderer,
                        typeof(SpriteRenderer), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
                else if (overlappingRenderer.originSortingGroup != null)
                {
                    EditorGUILayout.ObjectField("Sorting Group", overlappingRenderer.originSortingGroup,
                        typeof(SortingGroup), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
            }

            EditorGUI.EndDisabledGroup();

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

            if (GUILayout.Button("reset rotation"))
            {
                previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);
                UpdatePreviewEditor();
            }

            //hack for not seeing the previewGameObject in the scene view 
            previewGameObject.SetActive(true);
            var bgColor = new GUIStyle {normal = {background = EditorGUIUtility.whiteTexture}};
            previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(position.width, 256), bgColor);
            previewGameObject.SetActive(false);
            // Debug.Log(previewGameObject.transform.rotation);
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

            int sortingLayerIndex = GetLayerNameIndex(result.overlappingItems[0].originSortingLayer);

            foreach (var overlappingItem in result.overlappingItems)
            {
                overlappingItem.sortingLayer = sortingLayerIndex;
            }

            reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
                typeof(ReordableSpriteSortingItem), true, true, false, false);
            // reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
            // typeof(SpriteSortingReordableList.ReordableSpriteSortingItem), true, true, false, false);

            reordableSpriteSortingList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Overlapping Items");
            };

            reordableSpriteSortingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = result.overlappingItems[index];
                bool isPreviewUpdating = false;
                rect.y += 2;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 90, EditorGUIUtility.singleLineHeight),
                    element.originSpriteRenderer.name);

                EditorGUIUtility.labelWidth = 35;
                EditorGUI.BeginChangeCheck();
                element.sortingLayer = EditorGUI.Popup(
                    new Rect(rect.x + 90 + 10, rect.y, 135, EditorGUIUtility.singleLineHeight),
                    "Layer", element.sortingLayer, sortingLayerNames);

                if (EditorGUI.EndChangeCheck())
                {
                    element.tempSpriteRenderer.sortingLayerName = sortingLayerNames[element.sortingLayer];
                    // Debug.Log("changed layer to " + element.tempSpriteRenderer.sortingLayerName);
                    isPreviewUpdating = true;
                }

                //TODO: dynamic spacing depending on number of digits of sorting order
                EditorGUIUtility.labelWidth = 70;

                EditorGUI.BeginChangeCheck();
                element.sortingOrder = EditorGUI.IntField(
                    new Rect(rect.x + 90 + 10 + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "Order " + element.originSortingOrder + " +", element.sortingOrder);

                if (EditorGUI.EndChangeCheck())
                {
                    element.tempSpriteRenderer.sortingOrder = element.originSortingOrder + element.sortingOrder;
                    // Debug.Log("new order to " + element.tempSpriteRenderer.sortingOrder);
                    isPreviewUpdating = true;
                }

                if (GUI.Button(
                    new Rect(rect.x + 90 + 10 + 135 + 10 + 120 + 10, rect.y, 55, EditorGUIUtility.singleLineHeight),
                    "Select"))
                {
                    Selection.objects = new Object[] {element.originSpriteRenderer.gameObject};
                    SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);
                }

                if (isPreviewUpdating)
                {
                    UpdatePreviewEditor();
                }
            };
            // reordableSO.reordableSpriteSortingItems = result.overlappingItems;
            // serializedResult = new SerializedObject(reordableSO);
            // Repaint();
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
        }

        private void OnDestroy()
        {
            // DestroyImmediate(reordableSO);
        }
    }
}