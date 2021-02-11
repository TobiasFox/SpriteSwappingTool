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
using SpriteSortingPlugin.SpriteSwappingDetector.Logging;
using SpriteSortingPlugin.SpriteSwappingDetector.UI.Preview;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSwappingDetector.UI.OverlappingSprites
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
        private bool isUsingRelativeSortingOrder;
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
            LogReorderChangeModification(oldIndex, newIndex);
        }

        //TODO: remember last focussed element before solution gets recompiled
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
                item.isItemSelected = i == list.index;
            }

            lastFocussedIndex = list.index;
        }

        //TODO: adjust height when dragging elements
        private float ElementHeightCallback(int index)
        {
            var element = (OverlappingItem) reordableSpriteSortingList.list[index];
            if (element.SortingComponent.SortingGroup == null)
            {
                return EditorGUIUtility.singleLineHeight * 2 + LineSpacing + LineSpacing * 3;
            }

            return EditorGUIUtility.singleLineHeight * 3 + 2 * LineSpacing + LineSpacing * 3;
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = (OverlappingItem) reordableSpriteSortingList.list[index];
            var sortingComponentSpriteRenderer = element.SortingComponent.SpriteRenderer;
            var sortingComponentOutmostSortingGroup = element.SortingComponent.SortingGroup;
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

                var sortingGroupLabelText =
                    "in outer most Sorting Group \"" + sortingComponentOutmostSortingGroup.name + "\"";
                var sortingGroupLabel = new GUIContent(sortingGroupLabelText, Styling.SortingGroupIcon)
                {
                    tooltip = element.IsBaseItem
                        ? UITooltipConstants.OverlappingItemListBaseItemSortingGroupTooltip
                        : UITooltipConstants.OverlappingItemListSortingGroupTooltip
                };

                GUI.Label(new Rect(rect.x, rect.y, 280, EditorGUIUtility.singleLineHeight),
                    sortingGroupLabel);

                if (GUI.Button(new Rect(rect.width - 56, rect.y, 83,
                    EditorGUIUtility.singleLineHeight), "Select Group"))
                {
                    Selection.objects = new Object[] {sortingComponentOutmostSortingGroup.gameObject};
                    SceneView.lastActiveSceneView.Frame(sortingComponentSpriteRenderer.bounds);
                    EditorGUIUtility.PingObject(sortingComponentOutmostSortingGroup);
                }
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            EditorGUIUtility.labelWidth = 35;
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                element.sortingLayerDropDownIndex =
                    EditorGUI.Popup(new Rect(rect.x, rect.y, 135, EditorGUIUtility.singleLineHeight),
                        new GUIContent("Layer", UITooltipConstants.OverlappingItemListSortingLayerTooltip),
                        element.sortingLayerDropDownIndex, SortingLayerUtility.SortingLayerGuiContents);

                if (changeScope.changed)
                {
                    var modifiedLayerName = SortingLayerUtility.SortingLayerNames[element.sortingLayerDropDownIndex];

                    LogSortingLayerChangeModification(index, element.sortingLayerName, modifiedLayerName);

                    element.sortingLayerName = modifiedLayerName;
                    overlappingItems.UpdateSortingLayer(index, out var newIndexInList);
                    reordableSpriteSortingList.index = newIndexInList;
                    isPreviewUpdating = true;

                    overlappingItems.CheckChangedLayers();
                }
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                var sortingOrderGUIContent = new GUIContent
                {
                    text = "Order " + (isUsingRelativeSortingOrder ? element.originSortingOrder + " +" : ""),
                    tooltip = isUsingRelativeSortingOrder
                        ? UITooltipConstants.OverlappingItemListRelativeSortingOrderTooltip
                        : UITooltipConstants.OverlappingItemListTotalSortingOrderTooltip
                };
                var modifiedSortingOrder =
                    EditorGUI.DelayedIntField(
                        new Rect(rect.x + 135 + 10, rect.y, 120, EditorGUIUtility.singleLineHeight),
                        sortingOrderGUIContent, element.sortingOrder);

                if (changeScope.changed)
                {
                    LogSortingOrderChangeModification(index, modifiedSortingOrder);
                    element.sortingOrder = modifiedSortingOrder;
                    isPreviewUpdating = true;
                    overlappingItems.UpdateSortingOrder(index);
                }
            }

            if (GUI.Button(
                new Rect(rect.x + 135 + 10 + 120 + 10 + 25 + 10, rect.y, 25,
                    EditorGUIUtility.singleLineHeight), "-1"))
            {
                LogSortingOrderChangeModification(index, element.sortingOrder - 1);
                element.sortingOrder--;
                isPreviewUpdating = true;
                overlappingItems.UpdateSortingOrder(index);
            }

            if (GUI.Button(
                new Rect(rect.x + 135 + 10 + 120 + 10, rect.y, 25, EditorGUIUtility.singleLineHeight),
                "+1"))
            {
                LogSortingOrderChangeModification(index, element.sortingOrder + 1);
                element.sortingOrder++;
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
                new GUIContent("Overlapping Items of visual glitch", UITooltipConstants.OverlappingItemListTooltip));

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


        private void LogSortingLayerChangeModification(int currentIndex, string previousLayerName,
            string modifiedLayerName)
        {
            if (!IsLoggingActive(out var loggingData))
            {
                return;
            }

            var modificationData = new SortingSuggestionModificationData()
            {
                type = ModificationType.ChangeSortingLayer,
                itemIndex = currentIndex,
                layerIndex = SortingLayerUtility.GetLayerNameIndex(previousLayerName),
                modifiedLayerIndex = SortingLayerUtility.GetLayerNameIndex(modifiedLayerName)
            };

            var currentSuggestionLoggingData = loggingData.GetCurrentSuggestionLoggingData();
            currentSuggestionLoggingData?.AddModification(modificationData);
        }

        private void LogSortingOrderChangeModification(int currentIndex, int modifiedSortingOrder)
        {
            if (!IsLoggingActive(out var loggingData))
            {
                return;
            }

            var item = overlappingItems.Items[currentIndex];

            var modificationData = new SortingSuggestionModificationData()
            {
                type = ModificationType.ChangeSortingOrder,
                itemIndex = currentIndex,
                order = item.sortingOrder,
                isRelative = item.IsUsingRelativeSortingOrder,
                modifiedOrder = modifiedSortingOrder,
            };

            var sortingOrderSuggestionLoggingData = loggingData.GetCurrentSuggestionLoggingData();
            sortingOrderSuggestionLoggingData?.AddModification(modificationData);
        }

        private void LogReorderChangeModification(int currentIndex, int modifiedIndex)
        {
            if (!IsLoggingActive(out var loggingData))
            {
                return;
            }

            var modificationData = new SortingSuggestionModificationData()
            {
                type = ModificationType.Reorder,
                itemIndex = currentIndex,
                modifiedItemIndex = modifiedIndex
            };

            var sortingOrderSuggestionLoggingData = loggingData.GetCurrentSuggestionLoggingData();
            sortingOrderSuggestionLoggingData?.AddModification(modificationData);
        }

        private bool IsLoggingActive(out LoggingData loggingData)
        {
            loggingData = null;
            if (!GeneralData.isSurveyActive || !GeneralData.isLoggingActive)
            {
                return false;
            }

            loggingData = LoggingManager.GetInstance().loggingData;

            return loggingData.IsCurrentLoggingDataActive;
        }
    }
}