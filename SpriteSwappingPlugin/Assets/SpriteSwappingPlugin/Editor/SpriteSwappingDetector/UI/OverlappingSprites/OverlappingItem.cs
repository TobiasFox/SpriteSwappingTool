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
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSwappingPlugin.SpriteSwappingDetector.UI.OverlappingSprites
{
    [Serializable]
    public class OverlappingItem
    {
        public int originSortingOrder;
        public int originSortingLayer;
        public int originAutoSortingOrder;
        public int originSortedIndex;
        public bool isItemSelected;

        public SpriteRenderer previewSpriteRenderer;
        public SortingGroup previewSortingGroup;
        public int sortingOrder;
        public int sortingLayerDropDownIndex;
        public string sortingLayerName;
        private bool isUsingRelativeSortingOrder = true;
        private SortingComponent sortingComponent;
        private bool isBaseItem;
        private string spriteAssetGuid;

        public SortingComponent SortingComponent => sortingComponent;

        public bool IsBaseItem => isBaseItem;

        public string SpriteAssetGuid => spriteAssetGuid;

        public bool IsUsingRelativeSortingOrder => isUsingRelativeSortingOrder;

        public OverlappingItem(SpriteRenderer originSpriteRenderer)
        {
            var sortingGroup = originSpriteRenderer != null
                ? originSpriteRenderer.GetComponentInParent<SortingGroup>()
                : null;

            sortingComponent = new SortingComponent(originSpriteRenderer, sortingGroup);
            Init();
        }

        public OverlappingItem(SortingComponent sortingComponent, bool isBaseItem = false)
        {
            this.sortingComponent = sortingComponent;
            Init();
            this.isBaseItem = isBaseItem;

            spriteAssetGuid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(sortingComponent.SpriteRenderer.sprite.GetInstanceID()));
        }

        private void Init()
        {
            originSortingLayer = SortingComponent.OriginSortingLayer;
            originSortingOrder = SortingComponent.OriginSortingOrder;
        }

        public void UpdatePreviewSortingOrderWithExistingOrder()
        {
            var newPreviewSortingOrder = sortingOrder;
            if (isUsingRelativeSortingOrder)
            {
                newPreviewSortingOrder += originSortingOrder;
            }

            if (previewSortingGroup != null)
            {
                previewSortingGroup.sortingOrder = newPreviewSortingOrder;
            }
            else if (previewSpriteRenderer != null)
            {
                previewSpriteRenderer.sortingOrder = newPreviewSortingOrder;
            }
        }

        public void UpdatePreviewSortingLayer()
        {
            if (previewSortingGroup != null)
            {
                previewSortingGroup.sortingLayerName = sortingLayerName;
            }
            else if (previewSpriteRenderer != null)
            {
                previewSpriteRenderer.sortingLayerName = sortingLayerName;
            }
        }

        public void UpdatePreview()
        {
            UpdatePreviewSortingOrderWithExistingOrder();
            UpdatePreviewSortingLayer();
        }

        public void ConvertSortingOrder(bool isRelative)
        {
            isUsingRelativeSortingOrder = isRelative;
            sortingOrder += originSortingOrder * (isRelative ? -1 : 1);
        }

        public void CleanUpPreview()
        {
            if (previewSpriteRenderer != null)
            {
                Object.DestroyImmediate(previewSpriteRenderer.gameObject);
            }
        }

        public bool HasSortingLayerChanged()
        {
            return originSortingLayer !=
                   SortingLayer.NameToID(SortingLayerUtility.SortingLayerNames[sortingLayerDropDownIndex]);
        }

        public void ApplySortingOption(bool isContinuous = false)
        {
            var newSortingOrder = sortingOrder;
            if (isUsingRelativeSortingOrder)
            {
                newSortingOrder += originSortingOrder;
            }

            if (SortingComponent.SortingGroup != null)
            {
                ApplySortingOptionsToSortingGroup(newSortingOrder, isContinuous);

                return;
            }

            ApplySortingOptionsToSpriteRenderer(newSortingOrder, isContinuous);
        }

        private void ApplySortingOptionsToSpriteRenderer(int newSortingOrder, bool isContinuous = false)
        {
            var isSortingLayerIdentical = SortingComponent.SpriteRenderer.sortingLayerName.Equals(sortingLayerName);
            var isSortingOrderIdentical = SortingComponent.SpriteRenderer.sortingOrder == newSortingOrder;

            if (isSortingLayerIdentical && isSortingOrderIdentical)
            {
                return;
            }

            string message = null;

            if (!isContinuous)
            {
                message = "Update sorting options on SpriteRenderer " + sortingComponent.SpriteRenderer.name +
                          " - ";

                if (!isSortingLayerIdentical)
                {
                    message += "Sorting Layer: " + SortingComponent.SpriteRenderer.sortingLayerName + " -> " +
                               sortingLayerName + (!isSortingOrderIdentical ? ", " : "");
                }

                if (!isSortingOrderIdentical)
                {
                    message += "Sorting Order: " + SortingComponent.SpriteRenderer.sortingOrder + " -> " +
                               newSortingOrder;
                }

                Undo.RecordObject(SortingComponent.SpriteRenderer, "apply sorting options");
            }

            if (!isSortingLayerIdentical)
            {
                SortingComponent.SpriteRenderer.sortingLayerName = sortingLayerName;
            }

            if (!isSortingOrderIdentical)
            {
                SortingComponent.SpriteRenderer.sortingOrder = newSortingOrder;
            }

            if (!isContinuous)
            {
                EditorUtility.SetDirty(SortingComponent.SpriteRenderer);

                Debug.Log(message);
            }
        }

        private void ApplySortingOptionsToSortingGroup(int newSortingOrder, bool isContinuous)
        {
            if (sortingComponent.SortingGroup == null)
            {
                return;
            }

            var isSortingLayerIdentical = SortingComponent.SortingGroup.sortingLayerName.Equals(sortingLayerName);
            var isSortingOrderIdentical = SortingComponent.SortingGroup.sortingOrder == newSortingOrder;

            if (isSortingLayerIdentical && isSortingOrderIdentical)
            {
                return;
            }

            string message = null;

            if (!isContinuous)
            {
                message = "Update sorting options on SortingGroup " + sortingComponent.SortingGroup.name + " - ";

                if (!isSortingLayerIdentical)
                {
                    message += "Sorting Layer: " + SortingComponent.SortingGroup.sortingLayerName + " -> " +
                               sortingLayerName + (!isSortingOrderIdentical ? ", " : "");
                }

                if (!isSortingOrderIdentical)
                {
                    message += "Sorting Order: " + SortingComponent.SortingGroup.sortingOrder + " -> " +
                               newSortingOrder;
                }


                Undo.RecordObject(SortingComponent.SortingGroup, "apply sorting options");
            }

            if (!isSortingLayerIdentical)
            {
                SortingComponent.SortingGroup.sortingLayerName = sortingLayerName;
            }

            if (!isSortingOrderIdentical)
            {
                SortingComponent.SortingGroup.sortingOrder = newSortingOrder;
            }

            if (!isContinuous)
            {
                EditorUtility.SetDirty(SortingComponent.SortingGroup);

                Debug.Log(message);
            }
        }

        public int GetNewSortingOrder()
        {
            var newSortingOrder = sortingOrder;
            if (isUsingRelativeSortingOrder)
            {
                newSortingOrder += originSortingOrder;
            }

            return newSortingOrder;
        }

        public void RestoreSpriteRendererSortingOptions()
        {
            if (SortingComponent.SortingGroup != null)
            {
                SortingComponent.SortingGroup.sortingLayerID = originSortingLayer;
                SortingComponent.SortingGroup.sortingOrder = originSortingOrder;
                return;
            }

            SortingComponent.SpriteRenderer.sortingLayerID = originSortingLayer;
            SortingComponent.SpriteRenderer.sortingOrder = originSortingOrder;
        }

        public override string ToString()
        {
            return "OI[origin: " + SortingLayer.IDToName(originSortingLayer) + ", " + originSortingOrder +
                   " current: " + sortingLayerName + ", " + (originSortingOrder + sortingOrder) + "(" +
                   originSortingOrder + "+" + sortingOrder + "), baseItem: " + IsBaseItem +
                   ", originSortedIndex:" + originSortedIndex + "]";
        }
    }
}