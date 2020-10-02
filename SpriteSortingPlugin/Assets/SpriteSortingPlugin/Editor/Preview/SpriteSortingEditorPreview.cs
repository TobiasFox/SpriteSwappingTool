﻿using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.Preview
{
    [Serializable]
    public class SpriteSortingEditorPreview
    {
        private bool isPreviewVisible = true;
        private bool isPreviewNeedsAnUpdate;
        private GameObject previewGameObject;
        private Editor previewEditor;
        private bool isVisualizingBoundsInScene;
        private bool isSceneVisualizingDelegateIsAdded;
        private OverlappingItems overlappingItems;

        public void UpdateOverlappingItems(OverlappingItems overlappingItems)
        {
            this.overlappingItems = overlappingItems;
        }

        public void DoPreview(bool isUpdatePreview)
        {
            isPreviewVisible = EditorGUILayout.Foldout(isPreviewVisible, "Preview", true);

            if (!isPreviewVisible)
            {
                isPreviewNeedsAnUpdate = true;
                return;
            }

            if (isUpdatePreview || isPreviewNeedsAnUpdate)
            {
                isPreviewNeedsAnUpdate = false;
                CleanUpPreview();
            }

            GeneratePreview();

            var horizontalRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            isVisualizingBoundsInScene =
                EditorGUILayout.ToggleLeft("Visualize Bounds in Scene ", isVisualizingBoundsInScene);
            if (EditorGUI.EndChangeCheck())
            {
                EnableSceneVisualization(isVisualizingBoundsInScene);
            }

            if (GUILayout.Button("Reset rotation"))
            {
                previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);
                Object.DestroyImmediate(previewEditor);
                previewEditor = Editor.CreateEditor(previewGameObject);
            }

            EditorGUILayout.EndHorizontal();

            var bgColor = new GUIStyle {normal = {background = EditorGUIUtility.whiteTexture}};
            var previewRect = EditorGUILayout.GetControlRect(false, 256 + horizontalRect.height);

            //hack for not seeing the previewGameObject in the scene view 
            previewGameObject.SetActive(true);
            previewEditor.OnInteractivePreviewGUI(previewRect, bgColor);
            previewGameObject.SetActive(false);
        }

        private void GeneratePreview()
        {
            if (previewGameObject == null)
            {
                GeneratePreviewGameObject();
            }

            if (previewEditor == null)
            {
                previewEditor = Editor.CreateEditor(previewGameObject);
            }
        }

        private void GeneratePreviewGameObject()
        {
            previewGameObject = PreviewUtility.CreateGameObject(null, "Preview", true);
            previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);

            var previewRoot = new PreviewItem(previewGameObject.transform);

            foreach (var overlappingItem in overlappingItems.Items)
            {
                if (overlappingItem.originSortingGroup == null)
                {
                    var previewSpriteRenderer = previewRoot.AddSpriteRenderer(overlappingItem.originSpriteRenderer);
                    overlappingItem.previewSpriteRenderer = previewSpriteRenderer;
                    overlappingItem.UpdatePreviewSortingLayer();
                    overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();
                    continue;
                }

                var spritePreviewItem = GetAppropriatePreviewItem(overlappingItem.originSpriteRenderer, previewRoot);
                spritePreviewItem.AddSpriteRenderer(overlappingItem.originSpriteRenderer);

                previewRoot.TryGetSortingGroup(overlappingItem.originSortingGroup,
                    out overlappingItem.previewSortingGroup);
                overlappingItem.UpdatePreviewSortingLayer();
                overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();

                var childSpriteRenderer = overlappingItem.originSortingGroup
                    .GetComponentsInChildren<SpriteRenderer>();

                foreach (var spriteRenderer in childSpriteRenderer)
                {
                    if (spriteRenderer == overlappingItem.originSpriteRenderer)
                    {
                        continue;
                    }

                    if (!overlappingItem.originSpriteRenderer.bounds.Intersects(spriteRenderer.bounds))
                    {
                        continue;
                    }

                    var previewItem = GetAppropriatePreviewItem(spriteRenderer, previewRoot);
                    previewItem.AddSpriteRenderer(spriteRenderer);
                }
            }

            PreviewUtility.HideAndDontSaveGameObject(previewGameObject);
        }

        private static PreviewItem GetAppropriatePreviewItem(SpriteRenderer currentSpriteRenderer,
            PreviewItem lastPreviewGroup)
        {
            var activeSortingGroups =
                SpriteSortingUtility.FilterSortingGroups(currentSpriteRenderer.GetComponentsInParent<SortingGroup>());

            for (int i = activeSortingGroups.Count - 1; i >= 0; i--)
            {
                var currentSortingGroup = activeSortingGroups[i];

                var hasSortingGroup =
                    lastPreviewGroup.TryGetPreviewItem(currentSortingGroup, out var previewSortingGroup);

                if (!hasSortingGroup)
                {
                    lastPreviewGroup.AddSortingGroup(currentSortingGroup);
                    lastPreviewGroup.TryGetPreviewItem(currentSortingGroup, out lastPreviewGroup);
                }
                else
                {
                    lastPreviewGroup = previewSortingGroup;
                }
            }

            return lastPreviewGroup;
        }

        public void UpdatePreviewEditor()
        {
            if (!isPreviewVisible)
            {
                return;
            }

            previewGameObject.SetActive(true);
            previewEditor.ReloadPreviewInstances();
            previewGameObject.SetActive(false);
        }

        public void EnableSceneVisualization(bool isEnabled)
        {
            if (isEnabled && isVisualizingBoundsInScene)
            {
                if (!isSceneVisualizingDelegateIsAdded)
                {
                    isSceneVisualizingDelegateIsAdded = true;
                    SceneView.duringSceneGui += OnSceneGUI;
                }

                return;
            }

            if (isSceneVisualizingDelegateIsAdded)
            {
                isSceneVisualizingDelegateIsAdded = false;
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (overlappingItems == null)
            {
                return;
            }

            foreach (var item in overlappingItems.Items)
            {
                Handles.color = item.IsItemSelected ? Color.yellow : Color.red;
                var bounds = item.originSpriteRenderer.bounds;
                //TODO: consider rotated bounds
                Handles.DrawWireCube(bounds.center, new Vector3(bounds.size.x, bounds.size.y, 0));
            }

            if (overlappingItems.Items.Count > 0)
            {
                sceneView.Repaint();
            }
        }

        public void CleanUpPreview()
        {
            if (previewGameObject != null)
            {
                Object.DestroyImmediate(previewGameObject);
                previewGameObject = null;
            }

            if (previewEditor != null)
            {
                Object.DestroyImmediate(previewEditor);
            }
        }
    }
}