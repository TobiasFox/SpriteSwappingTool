using System;
using SpriteSortingPlugin.SpriteSorting.UI.Preview;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites
{
    [Serializable]
    public class ReordableOverlappingItemList
    {
        private const float LineSpacing = 1.5f;
        private const int SelectButtonWidth = 55;

        private ReorderableList reordableSpriteSortingList;
        private int lastFocussedIndex = -1;
        private OverlappingItems overlappingItems;
        private SpriteSortingEditorPreview preview;
        private bool isUsingRelativeSortingOrder = true;
        private float lastElementRectWidth;
        
        public void InitReordableList(OverlappingItems overlappingItems, SpriteSortingEditorPreview preview)
        {
            this.overlappingItems = overlappingItems;
            this.preview = preview;

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

            if (!isUsingRelativeSortingOrder)
            {
                overlappingItems.ConvertSortingOrder(isUsingRelativeSortingOrder);
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
            overlappingItems.ReOrderItem(newIndex);
            preview.UpdatePreviewEditor();
        }

        //TODO: remember last focussed element before recompilation is active
        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            Color color;
            if (isActive)
            {
                color = Styling.ListElementActiveColor;
            }
            else if (isFocused)
            {
                color = Styling.ListElementFocussingColor;
            }
            else
            {
                color = index % 2 == 0
                    ? Styling.ListElementBackground1
                    : Styling.ListElementBackground2;
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
            if (element.sortingComponent.sortingGroup == null)
            {
                return EditorGUIUtility.singleLineHeight * 2 + LineSpacing + LineSpacing * 3;
            }

            return EditorGUIUtility.singleLineHeight * 3 + 2 * LineSpacing + LineSpacing * 3;
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = (OverlappingItem) reordableSpriteSortingList.list[index];
            var sortingComponentSpriteRenderer = element.sortingComponent.spriteRenderer;
            var sortingComponentOutmostSortingGroup = element.sortingComponent.sortingGroup;
            var isPreviewUpdating = false;
            var startX = rect.x;

            rect.y += 2;

            if (element.IsBaseItem)
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Base Item", Styling.BaseItemIcon,
                        UITooltipConstants.OverlappingItemListBaseItemTooltip));
                startX += 80 + 5;
            }

            if (Event.current.type == EventType.Repaint)
            {
                lastElementRectWidth = rect.width;
            }

            var guiContent = new GUIContent("\"" + sortingComponentSpriteRenderer.name + "\"",
                Styling.SpriteIcon)
            {
                tooltip = element.IsBaseItem
                    ? UITooltipConstants.OverlappingItemListBaseItemSpriteRendererTooltip
                    : UITooltipConstants.OverlappingItemListSpriteRendererTooltip
            };

            EditorGUI.LabelField(new Rect(startX, rect.y, lastElementRectWidth - SelectButtonWidth - 90,
                EditorGUIUtility.singleLineHeight), guiContent);

            if (GUI.Button(new Rect(rect.width - 28, rect.y, SelectButtonWidth, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                Selection.objects = new Object[] {sortingComponentSpriteRenderer.gameObject};
                SceneView.lastActiveSceneView.Frame(sortingComponentSpriteRenderer.bounds);
                EditorGUIUtility.PingObject(sortingComponentSpriteRenderer);
            }

            if (sortingComponentOutmostSortingGroup != null)
            {
                rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

                var sortingGroupLabel = new GUIContent("in outmost Sorting Group", Styling.SortingGroupIcon)
                {
                    tooltip = element.IsBaseItem
                        ? UITooltipConstants.OverlappingItemListBaseItemSortingGroupTooltip
                        : UITooltipConstants.OverlappingItemListSortingGroupTooltip
                };

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 160, EditorGUIUtility.singleLineHeight),
                    sortingGroupLabel);

                EditorGUI.LabelField(
                    new Rect(rect.x + 160 + 2.5f, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    "\"" + sortingComponentOutmostSortingGroup.name + "\"");

                if (GUI.Button(new Rect(rect.width - 56, rect.y, 83,
                    EditorGUIUtility.singleLineHeight), "Select Group"))
                {
                    Selection.objects = new Object[] {sortingComponentOutmostSortingGroup.gameObject};
                    SceneView.lastActiveSceneView.Frame(element.sortingComponent.spriteRenderer.bounds);
                    EditorGUIUtility.PingObject(sortingComponentOutmostSortingGroup);
                }
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.BeginChangeCheck();

            element.sortingLayerDropDownIndex =
                EditorGUI.Popup(new Rect(rect.x, rect.y, 135, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Layer", UITooltipConstants.OverlappingItemListSortingLayerTooltip),
                    element.sortingLayerDropDownIndex, SortingLayerUtility.SortingLayerGuiContents);

            if (EditorGUI.EndChangeCheck())
            {
                element.sortingLayerName = SortingLayerUtility.SortingLayerNames[element.sortingLayerDropDownIndex];
                overlappingItems.UpdateSortingLayer(index, out var newIndexInList);
                reordableSpriteSortingList.index = newIndexInList;
                isPreviewUpdating = true;

                overlappingItems.CheckChangedLayers();
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            EditorGUI.BeginChangeCheck();
            var sortingOrderGUIContent = new GUIContent
            {
                text = "Order " + (isUsingRelativeSortingOrder ? element.originSortingOrder + " +" : ""),
                tooltip = isUsingRelativeSortingOrder
                    ? UITooltipConstants.OverlappingItemListRelativeSortingOrderTooltip
                    : UITooltipConstants.OverlappingItemListTotalSortingOrderTooltip
            };
            element.sortingOrder =
                EditorGUI.DelayedIntField(new Rect(rect.x + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                    sortingOrderGUIContent, element.sortingOrder);

            if (EditorGUI.EndChangeCheck())
            {
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
            var labelRect = rect;
            labelRect.xMax -= 135 + 45;
            EditorGUI.LabelField(labelRect,
                new GUIContent("Overlapping Items", UITooltipConstants.OverlappingItemListTooltip));

            if (GUI.Button(new Rect(rect.width - 172.5f, rect.y, 135, EditorGUIUtility.singleLineHeight),
                new GUIContent((isUsingRelativeSortingOrder ? "Total" : "Relative") + " Sorting Order",
                    UITooltipConstants.OverlappingItemListUsingRelativeSortingOrderTooltip))
            )
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
                reordableSpriteSortingList.index = lastFocussedIndex;
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