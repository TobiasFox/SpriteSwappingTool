#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System;
using SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSorting.UI.Preview
{
    [Serializable]
    public class SpriteSortingEditorPreview
    {
        private const float PreviewHeight = 256;
        private static readonly Quaternion DefaultPreviewRotation = Quaternion.Euler(0, 120f, 0);

        private bool isPreviewExpanded = true;
        private bool isPreviewNeedsAnUpdate;
        private GameObject previewGameObject;
        private Editor previewEditor;
        private bool isVisualizingBoundsInScene;
        private bool isSceneVisualizingDelegateIsAdded;
        private bool isVisualizingSortingOrder;
        private bool isVisualizingSortingLayer;
        private bool isUpdatingSpriteRendererInScene;
        private OverlappingItems overlappingItems;
        private SpriteData spriteData;
        private OutlinePrecision outlinePrecision;
        private float lastPreviewControlElementsHeight;

        private GUIStyle sortingOrderStyle;

        public bool IsVisualizingBoundsInScene => isVisualizingBoundsInScene;
        public bool IsUpdatingSpriteRendererInScene => isUpdatingSpriteRendererInScene;

        public void UpdateOverlappingItems(OverlappingItems overlappingItems)
        {
            this.overlappingItems = overlappingItems;
        }

        public void UpdateSpriteData(SpriteData spriteData)
        {
            this.spriteData = spriteData;
        }

        public void UpdateOutlineType(OutlinePrecision outlinePrecision)
        {
            this.outlinePrecision = outlinePrecision;
        }

        public void DoPreview(bool isUpdatePreview)
        {
            var guiLayoutOptionArray = new[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.Height(isPreviewExpanded ? EditorGUIUtility.singleLineHeight : 25)
            };

            DrawScenePreview();

            UIUtil.DrawHorizontalLine();

            GUILayout.Label("SpriteRenderer Preview", Styling.CenteredStyle, guiLayoutOptionArray);
            var lastRect = GUILayoutUtility.GetLastRect();
            var foldoutRect = new Rect(0, lastRect.y, 12, lastRect.height);

            isPreviewExpanded = EditorGUI.Foldout(foldoutRect, isPreviewExpanded, GUIContent.none, true);

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && lastRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.button == 0)
                {
                    isPreviewExpanded = !isPreviewExpanded;
                }

                currentEvent.Use();
            }

            if (!isPreviewExpanded)
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

            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Rotate by click and drag");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset Rotation", GUILayout.Width(95)))
                {
                    previewGameObject.transform.rotation = DefaultPreviewRotation;
                    Object.DestroyImmediate(previewEditor);
                    previewEditor = Editor.CreateEditor(previewGameObject);
                }

                if (Event.current.type == EventType.Repaint)
                {
                    lastPreviewControlElementsHeight = horizontalScope.rect.height;
                }
            }

            EditorGUILayout.Space();

            var bgColor = new GUIStyle {normal = {background = Styling.SpriteSortingPreviewBackgroundTexture}};
            var previewRect = GUILayoutUtility.GetRect(1f, PreviewHeight + lastPreviewControlElementsHeight);

            //hack for not seeing the previewGameObject in the scene view 
            previewGameObject.SetActive(true);
            previewEditor.OnInteractivePreviewGUI(previewRect, bgColor);
            previewGameObject.SetActive(false);
        }

        private bool isScenePreviewExpanded = true;

        private void DrawScenePreview()
        {
            using (new GUILayout.VerticalScope())
            {
                var guiLayoutOptionArray = new[]
                {
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(isPreviewExpanded ? EditorGUIUtility.singleLineHeight : 25)
                };

                GUILayout.Label("Preview in Scene", Styling.CenteredStyle, guiLayoutOptionArray);

                var lastRect = GUILayoutUtility.GetLastRect();
                var foldoutRect = new Rect(0, lastRect.y, 12, lastRect.height);

                isScenePreviewExpanded = EditorGUI.Foldout(foldoutRect, isScenePreviewExpanded, GUIContent.none, true);

                var currentEvent = Event.current;
                if (currentEvent.type == EventType.MouseDown && lastRect.Contains(currentEvent.mousePosition))
                {
                    if (currentEvent.button == 0)
                    {
                        isScenePreviewExpanded = !isScenePreviewExpanded;
                    }

                    currentEvent.Use();
                }

                if (!isScenePreviewExpanded)
                {
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                isVisualizingBoundsInScene =
                                    EditorGUILayout.ToggleLeft(
                                        new GUIContent("Draw Sprite Outline",
                                            UITooltipConstants.SortingEditorScenePreviewSpriteOutlineTooltip),
                                        isVisualizingBoundsInScene);
                                if (changeScope.changed)
                                {
                                    EnableSceneVisualization(isVisualizingBoundsInScene);
                                }
                            }

                            isUpdatingSpriteRendererInScene = EditorGUILayout.ToggleLeft(new GUIContent(
                                    "Live Update of SpriteRenderers' sorting options",
                                    UITooltipConstants.SortingEditorScenePreviewReflectSortingOptionsInSceneTooltip),
                                isUpdatingSpriteRendererInScene);
                        }

                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                isVisualizingSortingOrder =
                                    EditorGUILayout.ToggleLeft(new GUIContent("Show Label: Sorting Order",
                                            UITooltipConstants.SortingEditorScenePreviewDisplaySortingOrderTooltip),
                                        isVisualizingSortingOrder);
                                if (changeScope.changed)
                                {
                                    EnableSceneVisualization(isVisualizingSortingOrder);
                                }
                            }

                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                isVisualizingSortingLayer =
                                    EditorGUILayout.ToggleLeft(new GUIContent("Show Label: Sorting Layer",
                                            UITooltipConstants.SortingEditorScenePreviewDisplaySortingLayerTooltip),
                                        isVisualizingSortingLayer);
                                if (changeScope.changed)
                                {
                                    EnableSceneVisualization(isVisualizingSortingLayer);
                                }
                            }
                        }
                    }
                }

                if (!isSceneVisualizingDelegateIsAdded &&
                    (isVisualizingBoundsInScene || isVisualizingSortingLayer || isVisualizingSortingOrder))
                {
                    EnableSceneVisualization(true);
                }
            }
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
            previewGameObject.transform.rotation = DefaultPreviewRotation;

            var previewRoot = new PreviewItem(previewGameObject.transform);

            foreach (var overlappingItem in overlappingItems.Items)
            {
                var sortingComponentOutmostSortingGroup = overlappingItem.SortingComponent.SortingGroup;
                var sortingComponentSpriteRenderer = overlappingItem.SortingComponent.SpriteRenderer;

                if (sortingComponentOutmostSortingGroup == null)
                {
                    var previewSpriteRenderer = previewRoot.AddSpriteRenderer(sortingComponentSpriteRenderer);
                    overlappingItem.previewSpriteRenderer = previewSpriteRenderer;
                    overlappingItem.UpdatePreviewSortingLayer();
                    overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();
                    continue;
                }

                var spritePreviewItem = GetAppropriatePreviewItem(sortingComponentSpriteRenderer, previewRoot);
                spritePreviewItem.AddSpriteRenderer(sortingComponentSpriteRenderer);

                previewRoot.TryGetSortingGroup(sortingComponentOutmostSortingGroup,
                    out overlappingItem.previewSortingGroup);
                overlappingItem.UpdatePreviewSortingLayer();
                overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();

                var childSpriteRenderer = sortingComponentOutmostSortingGroup
                    .GetComponentsInChildren<SpriteRenderer>();

                foreach (var spriteRenderer in childSpriteRenderer)
                {
                    if (spriteRenderer == sortingComponentSpriteRenderer)
                    {
                        continue;
                    }

                    if (!sortingComponentSpriteRenderer.bounds.Intersects(spriteRenderer.bounds))
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
                SortingGroupUtility.GetAllEnabledSortingGroups(
                    currentSpriteRenderer.GetComponentsInParent<SortingGroup>());

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
            if (!isPreviewExpanded)
            {
                return;
            }

            previewGameObject.SetActive(true);
            previewEditor.ReloadPreviewInstances();
            previewGameObject.SetActive(false);
        }

        public void EnableSceneVisualization(bool isEnabled)
        {
            if (isEnabled && (isVisualizingBoundsInScene || isVisualizingSortingLayer || isVisualizingSortingOrder))
            {
                if (!isSceneVisualizingDelegateIsAdded)
                {
                    if (sortingOrderStyle == null)
                    {
                        sortingOrderStyle = new GUIStyle
                            {normal = {background = Texture2D.whiteTexture}, fontStyle = FontStyle.Bold};
                    }

                    isSceneVisualizingDelegateIsAdded = true;
                    SceneView.duringSceneGui += OnSceneGUI;
                    SceneView.RepaintAll();
                }

                return;
            }

            if (isSceneVisualizingDelegateIsAdded)
            {
                isSceneVisualizingDelegateIsAdded = false;
                SceneView.duringSceneGui -= OnSceneGUI;
                SceneView.RepaintAll();
            }
        }

        public void DisableSceneVisualizations()
        {
            if (!isSceneVisualizingDelegateIsAdded)
            {
                return;
            }

            isSceneVisualizingDelegateIsAdded = false;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (overlappingItems == null)
            {
                return;
            }

            foreach (var item in overlappingItems.Items)
            {
                DrawBounds(item);
                DrawSortingOptions(item);
            }

            if (overlappingItems.Items.Count > 0)
            {
                sceneView.Repaint();
            }
        }

        private void DrawSortingOptions(OverlappingItem item)
        {
            if (!isVisualizingSortingOrder && !isVisualizingSortingLayer)
            {
                return;
            }

            Handles.BeginGUI();

            var text = "";
            if (isVisualizingSortingLayer)
            {
                text = item.sortingLayerName;
                if (item.HasSortingLayerChanged())
                {
                    text += " -> " + item.sortingLayerName;
                }
            }

            if (isVisualizingSortingOrder)
            {
                text += (isVisualizingSortingLayer ? "\n " : "") + item.originSortingOrder;
                var newSortingOrder = item.GetNewSortingOrder();
                if (item.originSortingOrder != newSortingOrder)
                {
                    text += " -> " + newSortingOrder;
                }
            }

            Handles.Label(item.SortingComponent.SpriteRenderer.transform.position, text, sortingOrderStyle);

            Handles.EndGUI();
        }

        private void DrawBounds(OverlappingItem item)
        {
            if (!isVisualizingBoundsInScene)
            {
                return;
            }

            Handles.color = item.isItemSelected ? Color.yellow : Color.red;

            var isDrawingSpriteRendererBounds = false;

            if (spriteData != null)
            {
                var hasSpriteDataItem =
                    spriteData.spriteDataDictionary.TryGetValue(item.SpriteAssetGuid, out var spriteDataItem);

                if (hasSpriteDataItem && CanDrawOutlineType(spriteDataItem))
                {
                    DrawOutline(spriteDataItem, item.SortingComponent.SpriteRenderer.transform);
                }
                else
                {
                    isDrawingSpriteRendererBounds = true;
                }
            }
            else
            {
                isDrawingSpriteRendererBounds = true;
            }

            if (isDrawingSpriteRendererBounds)
            {
                var bounds = item.SortingComponent.SpriteRenderer.bounds;
                Handles.DrawWireCube(bounds.center, new Vector3(bounds.size.x, bounds.size.y, 0));
            }
        }

        private void DrawOutline(SpriteDataItem spriteDataItem, Transform itemTransform)
        {
            switch (outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    spriteDataItem.objectOrientedBoundingBox.UpdateBox(itemTransform);
                    var oobbPoints = spriteDataItem.objectOrientedBoundingBox.Points;

                    Handles.DrawLine(oobbPoints[0], oobbPoints[1]);
                    Handles.DrawLine(oobbPoints[1], oobbPoints[2]);
                    Handles.DrawLine(oobbPoints[2], oobbPoints[3]);
                    Handles.DrawLine(oobbPoints[3], oobbPoints[0]);
                    break;
                case OutlinePrecision.PixelPerfect:
                    var lastPoint = itemTransform.TransformPoint(spriteDataItem.outlinePoints[0]);
                    for (var i = 1; i < spriteDataItem.outlinePoints.Length; i++)
                    {
                        var nextPoint = itemTransform.TransformPoint(spriteDataItem.outlinePoints[i]);
                        Handles.DrawLine(lastPoint, nextPoint);
                        lastPoint = nextPoint;
                    }

                    break;
            }
        }

        private bool CanDrawOutlineType(SpriteDataItem spriteDataItem)
        {
            if (spriteDataItem == null)
            {
                return false;
            }

            switch (outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    return spriteDataItem.IsValidOOBB();
                case OutlinePrecision.PixelPerfect:
                    return spriteDataItem.IsValidOutline();
                default:
                    return false;
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