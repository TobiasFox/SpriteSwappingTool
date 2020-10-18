using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class OverlappingItem
    {
        public SpriteRenderer originSpriteRenderer;
        public SortingGroup originSortingGroup;
        public int originSortingOrder;
        public int originSortingLayer;

        public SpriteRenderer previewSpriteRenderer;
        public SortingGroup previewSortingGroup;
        public int sortingOrder;
        public int sortingLayerDropDownIndex;
        public string sortingLayerName;
        private bool isUsingRelativeSortingOrder = true;

        public bool IsItemSelected { get; set; }
        public int OriginSortedIndex { get; set; }
        public bool IsBaseItem { get; private set; }

        public string SpriteAssetGuid { get; private set; }

        public OverlappingItem(SpriteRenderer originSpriteRenderer)
        {
            this.originSpriteRenderer = originSpriteRenderer;
            originSortingGroup = originSpriteRenderer.GetComponentInParent<SortingGroup>();

            if (originSortingGroup != null)
            {
                originSortingLayer = originSortingGroup.sortingLayerID;
                originSortingOrder = originSortingGroup.sortingOrder;
            }
            else
            {
                originSortingLayer = originSpriteRenderer.sortingLayerID;
                originSortingOrder = originSpriteRenderer.sortingOrder;
            }
        }

        public OverlappingItem(SortingComponent sortingComponent, bool isBaseItem = false)
        {
            originSpriteRenderer = sortingComponent.spriteRenderer;
            originSortingGroup = sortingComponent.outmostSortingGroup;
            IsBaseItem = isBaseItem;

            if (originSortingGroup != null)
            {
                originSortingLayer = originSortingGroup.sortingLayerID;
                originSortingOrder = originSortingGroup.sortingOrder;
            }
            else
            {
                originSortingLayer = originSpriteRenderer.sortingLayerID;
                originSortingOrder = originSpriteRenderer.sortingOrder;
            }

            SpriteAssetGuid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(originSpriteRenderer.sprite.GetInstanceID()));
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

        public void ApplySortingOption()
        {
            var newSortingOrder = sortingOrder;
            if (isUsingRelativeSortingOrder)
            {
                newSortingOrder += originSortingOrder;
            }

            if (originSortingGroup != null)
            {
                Debug.LogFormat(
                    "Update Sorting options on Sorting Group {0} - Sorting Layer from {1} to {2}, Sorting Order from {3} to {4}",
                    originSortingGroup.name, originSortingGroup.sortingLayerName, sortingLayerName,
                    originSortingGroup.sortingOrder, newSortingOrder);

                Undo.RecordObject(originSortingGroup, "apply sorting options");
                originSortingGroup.sortingLayerName = sortingLayerName;
                originSortingGroup.sortingOrder = newSortingOrder;
                EditorUtility.SetDirty(originSortingGroup);

                return;
            }

            Debug.LogFormat(
                "Update Sorting options on SpriteRenderer {0} - Sorting Layer from {1} to {2}, Sorting Order from {3} to {4}",
                originSpriteRenderer.name, originSpriteRenderer.sortingLayerName, sortingLayerName,
                originSpriteRenderer.sortingOrder, newSortingOrder);

            Undo.RecordObject(originSpriteRenderer, "apply sorting options");
            originSpriteRenderer.sortingLayerName = sortingLayerName;
            originSpriteRenderer.sortingOrder = newSortingOrder;
            EditorUtility.SetDirty(originSpriteRenderer);
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

        public override string ToString()
        {
            return "OI[origin: " + SortingLayer.IDToName(originSortingLayer) + ", " + originSortingOrder +
                   " current: " + sortingLayerName + ", " + (originSortingOrder + sortingOrder) + "(" +
                   originSortingOrder + "+" + sortingOrder + "), baseItem: " + IsBaseItem +
                   ", originSortedIndex:" + OriginSortedIndex + "]";
        }
    }
}