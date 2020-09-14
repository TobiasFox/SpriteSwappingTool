﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSorting
{
    public class SpriteSortingEditorWindow : EditorWindow
    {
        [SerializeField] private bool ignoreAlphaOfSprites;
        [SerializeField] private CameraProjectionType cameraProjectionType;
        [SerializeField] private SortingType sortingType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SortingGroup sortingGroup;

        private int selectedSortingLayers;
        private string[] sortingLayerNames;
        private List<int> selectedLayers;

        private SpriteSortingAnalysisResult result;
        private bool analyzeButtonWasClicked;
        private ReorderableList reordableSpriteSortingList;

        private SerializedObject serializedResult;

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

            if (GUILayout.Button("Analyze"))
            {
                Analyze();
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
                serializedResult = null;
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            foreach (var overlappingRenderer in result.overlappingItems)
            {
                if (overlappingRenderer.spriteRenderer != null)
                {
                    EditorGUILayout.ObjectField("Sprite", overlappingRenderer.spriteRenderer, typeof(SpriteRenderer),
                        true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
                else if (overlappingRenderer.sortingGroup != null)
                {
                    EditorGUILayout.ObjectField("Sorting Group", overlappingRenderer.sortingGroup, typeof(SortingGroup),
                        true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
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

            CreatePreview();
        }

        private void CreatePreview()
        {
            isPreviewVisible = EditorGUILayout.Foldout(isPreviewVisible, "Preview", true);

            if (isPreviewVisible)
            {
                if (previewGameObject == null)
                {
                    GeneratePreviewGameObject();
                }

                if (previewEditor == null)
                {
                    previewEditor = Editor.CreateEditor(previewGameObject);
                }

                if (GUILayout.Button("reset rotation"))
                {
                    DestroyImmediate(previewEditor);
                    previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);
                    previewEditor = Editor.CreateEditor(previewGameObject);
                }

                var bgColor = new GUIStyle {normal = {background = EditorGUIUtility.whiteTexture}};
                previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
            }
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
            }

            previewEditor = null;
        }

        private void GeneratePreviewGameObject()
        {
            previewGameObject = new GameObject();
            previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);

            foreach (var overlappingItem in result.overlappingItems)
            {
                var spriteGameObject = new GameObject(overlappingItem.spriteRenderer.name);
                ComponentUtility.CopyComponent(overlappingItem.spriteRenderer.transform);
                ComponentUtility.PasteComponentValues(spriteGameObject.transform);
                ComponentUtility.CopyComponent(overlappingItem.spriteRenderer);
                ComponentUtility.PasteComponentAsNew(spriteGameObject);
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
                selectedSortingLayers = 1 << 0;
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
            // foreach (var sortingLayer in SortingLayer.layers)
            // {
            //     Debug.Log(sortingLayer.name + " " + sortingLayer.id);
            // }
            //
            // Debug.Log(SortingLayer.NameToID("Test2"));

            result = SpriteSortingUtility.AnalyzeSpriteSorting(new SpriteSortingData
                {selectedLayers = selectedLayers, cameraProjectionType = cameraProjectionType});

            reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
                typeof(SpriteSortingReordableList.ReordableSpriteSortingItem), true, true, false, false);
            // reordableSpriteSortingList = new ReorderableList(result.overlappingItems,
            // typeof(SpriteSortingReordableList.ReordableSpriteSortingItem), true, true, false, false);

            reordableSpriteSortingList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Overlapping Items");
            };

            reordableSpriteSortingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = result.overlappingItems[index];
                rect.y += 2;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 90, EditorGUIUtility.singleLineHeight),
                    element.spriteRenderer.name);

                EditorGUIUtility.labelWidth = 50;
                element.sortingLayer = EditorGUI.Popup(
                    new Rect(rect.x + 90 + 10, rect.y, 150, EditorGUIUtility.singleLineHeight),
                    "Layer", element.sortingLayer, sortingLayerNames);

                //TODO: dynamic spacing depending on number of digits of sorting order
                EditorGUIUtility.labelWidth = 110;
                element.sortingOrder = EditorGUI.IntField(
                    new Rect(rect.x + 90 + 10 + 150 + 10, rect.y, 150, EditorGUIUtility.singleLineHeight),
                    "current order " + element.spriteRenderer.sortingOrder + " +", element.sortingOrder);

                if (GUI.Button(
                    new Rect(rect.x + 90 + 10 + 150 + 10 + 150 + 10, rect.y, 60, EditorGUIUtility.singleLineHeight),
                    "Select"))
                {
                    Debug.Log("select " + element.spriteRenderer.name);
                    Selection.objects = new Object[] {element.spriteRenderer.gameObject};
                    SceneView.lastActiveSceneView.Frame(element.spriteRenderer.bounds);
                }
            };
            // reordableSO.reordableSpriteSortingItems = result.overlappingItems;
            // serializedResult = new SerializedObject(reordableSO);
            Repaint();
        }

        private int GetCurrentIndexFromLayerNames(int currentLayerId)
        {
            var layerName = SortingLayer.IDToName(currentLayerId);

            for (var i = 0; i < SortingLayer.layers.Length; i++)
            {
                if (layerName.Equals(SortingLayer.layers[i].name))
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