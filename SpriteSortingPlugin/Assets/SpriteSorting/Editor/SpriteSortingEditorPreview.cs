using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSorting
{
    [Serializable]
    public class SpriteSortingEditorPreview
    {
        private bool isPreviewVisible = true;
        private GameObject previewGameObject;
        private Editor previewEditor;
        private bool isVisualizingBoundsInScene;
        private bool isSceneVisualizingDelegateIsAdded;
        private List<ReordableSpriteSortingItem> overlappingItems;

        public SpriteSortingEditorPreview(List<ReordableSpriteSortingItem> overlappingItems)
        {
            this.overlappingItems = overlappingItems;
        }

        public void UpdateOverlappingItems(List<ReordableSpriteSortingItem> overlappingItems)
        {
            this.overlappingItems = overlappingItems;
        }

        public void DoPreview(bool isUpdatePreview, float currentEditorWidth)
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
                previewEditor = Editor.CreateEditor(previewGameObject);
            }

            var horizontalRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = 180;
            EditorGUI.BeginChangeCheck();
            isVisualizingBoundsInScene =
                EditorGUILayout.Toggle("Visualize Bounds in Scene ", isVisualizingBoundsInScene);
            if (EditorGUI.EndChangeCheck())
            {
                EnableSceneVisualization(isVisualizingBoundsInScene);
            }

            EditorGUIUtility.labelWidth = -1;

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

        private void GeneratePreviewGameObject()
        {
            previewGameObject = new GameObject
            {
                hideFlags = HideFlags.DontSave
            };
            previewGameObject.transform.rotation = Quaternion.Euler(0, 120f, 0);

            foreach (var overlappingItem in overlappingItems)
            {
                var spriteGameObject = new GameObject(overlappingItem.originSpriteRenderer.name)
                {
                    hideFlags = HideFlags.DontSave
                };
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

            foreach (var item in overlappingItems)
            {
                Handles.color = item.IsItemSelected ? Color.yellow : Color.red;
                var bounds = item.originSpriteRenderer.bounds;
                //TODO: consider rotated bounds
                Handles.DrawWireCube(bounds.center, new Vector3(bounds.size.x, bounds.size.y, 0));
            }

            if (overlappingItems.Count > 0)
            {
                sceneView.Repaint();
            }
        }

        public void CleanUpPreview()
        {
            if (previewGameObject != null)
            {
                var transformChildCount = previewGameObject.transform.childCount;
                for (int i = 0; i < transformChildCount; i++)
                {
                    var childTransform = previewGameObject.transform.GetChild(0);
                    if (childTransform != null)
                    {
                        Object.DestroyImmediate(childTransform.gameObject);
                    }
                }

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