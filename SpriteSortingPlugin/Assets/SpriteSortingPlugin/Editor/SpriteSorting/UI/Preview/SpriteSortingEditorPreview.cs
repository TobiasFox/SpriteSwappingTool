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

        private bool isPreviewVisible = true;
        private bool isPreviewNeedsAnUpdate;
        private GameObject previewGameObject;
        private Editor previewEditor;
        private bool isVisualizingBoundsInScene;
        private bool isSceneVisualizingDelegateIsAdded;
        private bool isVisualizingSortingOrder;
        private bool isVisualizingSortingLayer;
        private OverlappingItems overlappingItems;
        private SpriteData spriteData;
        private OutlinePrecision outlinePrecision;
        private float lastPreviewControlElementsHeight;

        private GUIStyle sortingOrderStyle;

        public bool IsVisualizingBoundsInScene => isVisualizingBoundsInScene;

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

            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    DrawSpriteDataSceneVisualizationToggles();
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

        private void DrawSpriteDataSceneVisualizationToggles()
        {
            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label("Visualization in the scene", Styling.CenteredStyle, GUILayout.ExpandWidth(true));
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    isVisualizingBoundsInScene = GUILayout.Toggle(isVisualizingBoundsInScene,
                        new GUIContent("Outline", UITooltipConstants.SortingEditorScenePreviewSpriteOutlineTooltip),
                        Styling.ButtonStyle, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        EnableSceneVisualization(isVisualizingBoundsInScene);
                    }

                    EditorGUI.BeginChangeCheck();
                    isVisualizingSortingOrder = GUILayout.Toggle(isVisualizingSortingOrder, new GUIContent(
                            "Sorting Order", UITooltipConstants.SortingEditorScenePreviewDisplaySortingOrderTooltip),
                        Styling.ButtonStyle, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        EnableSceneVisualization(isVisualizingSortingOrder);
                    }

                    EditorGUI.BeginChangeCheck();
                    isVisualizingSortingLayer = GUILayout.Toggle(isVisualizingSortingLayer, new GUIContent(
                            "Sorting Layer", UITooltipConstants.SortingEditorScenePreviewDisplaySortingLayerTooltip),
                        Styling.ButtonStyle, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        EnableSceneVisualization(isVisualizingSortingLayer);
                    }
                }

                if (!isSceneVisualizingDelegateIsAdded &&
                    (isVisualizingBoundsInScene || isVisualizingSortingLayer || isVisualizingSortingOrder))
                {
                    EnableSceneVisualization(true);
                }
            }

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(22.5f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Reset Rotation", GUILayout.Width(95)))
                    {
                        previewGameObject.transform.rotation = DefaultPreviewRotation;
                        Object.DestroyImmediate(previewEditor);
                        previewEditor = Editor.CreateEditor(previewGameObject);
                    }
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
                var sortingComponentOutmostSortingGroup = overlappingItem.sortingComponent.sortingGroup;
                var sortingComponentSpriteRenderer = overlappingItem.sortingComponent.spriteRenderer;

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

            Handles.Label(item.sortingComponent.spriteRenderer.transform.position, text, sortingOrderStyle);

            Handles.EndGUI();
        }

        private void DrawBounds(OverlappingItem item)
        {
            if (!isVisualizingBoundsInScene)
            {
                return;
            }

            Handles.color = item.IsItemSelected ? Color.yellow : Color.red;

            var isDrawingSpriteRendererBounds = false;

            if (spriteData != null)
            {
                var hasSpriteDataItem =
                    spriteData.spriteDataDictionary.TryGetValue(item.SpriteAssetGuid, out var spriteDataItem);

                if (hasSpriteDataItem && CanDrawOutlineType(spriteDataItem))
                {
                    DrawOutline(spriteDataItem, item.sortingComponent.spriteRenderer.transform);
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
                var bounds = item.sortingComponent.spriteRenderer.bounds;
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