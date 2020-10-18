using System;
using SpriteSortingPlugin.Preview;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class ReordableOverlappingItemList
    {
        private const float LineSpacing = 1.5f;
        private const int SelectButtonWidth = 55;
        private static Texture spriteIcon;
        private static Texture baseItemIcon;
        private static Texture sortingGroupIcon;
        private static bool isInitializedIcons;

        private ReorderableList reordableSpriteSortingList;
        private int lastFocussedIndex = -1;
        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;
        private bool isUsingRelativeSortingOrder = true;

        public void InitReordableList(OverlappingItems overlappingItems, SpriteSortingEditorPreview preview)
        {
            this.overlappingItems = overlappingItems;
            this.preview = preview;

            if (!isInitializedIcons)
            {
                spriteIcon = EditorGUIUtility.IconContent("Sprite Icon").image;
                baseItemIcon = EditorGUIUtility.IconContent("PreMatCylinder@2x").image;
                sortingGroupIcon = EditorGUIUtility.IconContent("BlendTree Icon").image;
                isInitializedIcons = true;
            }

            reordableSpriteSortingList = new ReorderableList(overlappingItems.Items,
                typeof(OverlappingItem), true, true, false, false)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
                onSelectCallback = OnSelectCallback,
                elementHeightCallback = ElementHeightCallback,
                drawElementBackgroundCallback = DrawElementBackgroundCallback,
                onReorderCallbackWithDetails = OnReorderCallbackWithDetails,
                index = lastFocussedIndex
            };

            {
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
        }

        public void DoLayoutList()
        {
            reordableSpriteSortingList.DoLayoutList();
        }

        public int GetIndex()
        {
            return reordableSpriteSortingList.index;
        }

        public void SetIndex(int newIndex)
        {
            reordableSpriteSortingList.index = newIndex;
            OnSelectCallback(reordableSpriteSortingList);
        }

        private void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            overlappingItems.ReOrderItem(oldIndex, newIndex);
            preview.UpdatePreviewEditor();
        }

        //TODO: remember last focussed element before recompilation is active
        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            Color color;
            if (isActive)
            {
                color = ReordableBackgroundColors.ActiveColor;
            }
            else if (isFocused)
            {
                color = ReordableBackgroundColors.FocussingColor;
            }
            else
            {
                color = index % 2 == 0
                    ? ReordableBackgroundColors.BackgroundColor1
                    : ReordableBackgroundColors.BackgroundColor2;
            }

            EditorGUI.DrawRect(rect, color);
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
            var element = (OverlappingItem) reordableSpriteSortingList.list[index];
            if (element.originSortingGroup == null)
            {
                return EditorGUIUtility.singleLineHeight * 2 + LineSpacing + LineSpacing * 3;
            }

            return EditorGUIUtility.singleLineHeight * 3 + 2 * LineSpacing + LineSpacing * 3;
        }

        private float lastElementRectWidth;

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = (OverlappingItem) reordableSpriteSortingList.list[index];
            var isPreviewUpdating = false;
            var startX = rect.x;

            rect.y += 2;

            if (element.IsBaseItem)
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Base Item", baseItemIcon));
                startX += 80 + 5;
            }

            if (Event.current.type == EventType.Repaint)
            {
                lastElementRectWidth = rect.width;
            }

            EditorGUI.LabelField(
                new Rect(startX, rect.y, lastElementRectWidth - SelectButtonWidth - 90, EditorGUIUtility.singleLineHeight),
                new GUIContent("\"" + element.originSpriteRenderer.name + "\"", spriteIcon));

            if (GUI.Button(new Rect(rect.width - 28, rect.y, SelectButtonWidth, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                Selection.objects = new Object[] {element.originSpriteRenderer.gameObject};
                SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);
                EditorGUIUtility.PingObject(element.originSpriteRenderer);
            }

            if (element.originSortingGroup != null)
            {
                rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 160, EditorGUIUtility.singleLineHeight),
                    new GUIContent("in outmost Sorting Group", sortingGroupIcon));

                EditorGUI.LabelField(
                    new Rect(rect.x + 160 + 2.5f, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "\"" + element.originSortingGroup.name + "\"");

                if (GUI.Button(
                    new Rect(rect.width - 56, rect.y, 83,
                        EditorGUIUtility.singleLineHeight), "Select Group"))
                {
                    Selection.objects = new Object[] {element.originSortingGroup.gameObject};
                    SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);
                    EditorGUIUtility.PingObject(element.originSortingGroup);
                }
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.BeginChangeCheck();
            element.sortingLayerDropDownIndex =
                EditorGUI.Popup(new Rect(rect.x, rect.y, 135, EditorGUIUtility.singleLineHeight), "Layer",
                    element.sortingLayerDropDownIndex, SortingLayerUtility.SortingLayerNames);

            if (EditorGUI.EndChangeCheck())
            {
                element.sortingLayerName = SortingLayerUtility.SortingLayerNames[element.sortingLayerDropDownIndex];
                overlappingItems.UpdateSortingLayer(index, out var newIndexInList);
                reordableSpriteSortingList.index = newIndexInList;
                // Debug.Log("changed layer to " + element.tempSpriteRenderer.sortingLayerName);
                isPreviewUpdating = true;

                overlappingItems.CheckChangedLayers();
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            EditorGUI.BeginChangeCheck();
            var sortingOrderLabel = "Order " + (isUsingRelativeSortingOrder ? element.originSortingOrder + " +" : "");
            element.sortingOrder =
                EditorGUI.DelayedIntField(new Rect(rect.x + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    sortingOrderLabel, element.sortingOrder);

            if (EditorGUI.EndChangeCheck())
            {
                // Debug.Log("new order to " + element.tempSpriteRenderer.sortingOrder);
                isPreviewUpdating = true;
                overlappingItems.UpdateSortingOrder(index);
            }

            if (GUI.Button(
                new Rect(rect.x + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                element.sortingOrder++;
                isPreviewUpdating = true;
                overlappingItems.UpdateSortingOrder(index);
            }

            if (GUI.Button(
                new Rect(rect.x + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                element.sortingOrder--;
                isPreviewUpdating = true;
                overlappingItems.UpdateSortingOrder(index);
            }

            if (isPreviewUpdating)
            {
                OnSelectCallback(reordableSpriteSortingList);

                preview.UpdatePreviewEditor();
            }
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Overlapping Items");

            if (GUI.Button(new Rect(rect.width - 172.5f, rect.y, 135, EditorGUIUtility.singleLineHeight),
                (isUsingRelativeSortingOrder ? "Total" : "Relative") + " Sorting Order"))
            {
                isUsingRelativeSortingOrder = !isUsingRelativeSortingOrder;
                overlappingItems.ConvertSortingOrder(isUsingRelativeSortingOrder);
                preview.UpdatePreviewEditor();
            }

            if (GUI.Button(new Rect(rect.width - 35, rect.y, 45, EditorGUIUtility.singleLineHeight), "Reset"))
            {
                overlappingItems.Reset();
                preview.UpdatePreviewEditor();

                lastFocussedIndex = -1;
            }
        }

        public void CleanUp()
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
        }
    }
}