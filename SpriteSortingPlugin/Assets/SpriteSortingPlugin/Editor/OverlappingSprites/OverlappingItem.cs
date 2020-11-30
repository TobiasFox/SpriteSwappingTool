using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.OverlappingSprites
{
    [Serializable]
    public class OverlappingItem
    {
        public readonly SortingComponent sortingComponent;
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
        public int OriginAutoSortingOrder { get; set; }
        public bool IsBaseItem { get; private set; }
        public string SpriteAssetGuid { get; private set; }

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
            IsBaseItem = isBaseItem;

            SpriteAssetGuid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(sortingComponent.spriteRenderer.sprite.GetInstanceID()));
        }

        private void Init()
        {
            originSortingLayer = sortingComponent.OriginSortingLayer;
            originSortingOrder = sortingComponent.OriginSortingOrder;
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

            if (sortingComponent.sortingGroup != null)
            {
                Debug.LogFormat(
                    "Update Sorting options on Sorting Group {0} - Sorting Layer from {1} to {2}, Sorting Order from {3} to {4}",
                    sortingComponent.sortingGroup.name, sortingComponent.sortingGroup.sortingLayerName,
                    sortingLayerName,
                    sortingComponent.sortingGroup.sortingOrder, newSortingOrder);

                Undo.RecordObject(sortingComponent.sortingGroup, "apply sorting options");
                sortingComponent.sortingGroup.sortingLayerName = sortingLayerName;
                sortingComponent.sortingGroup.sortingOrder = newSortingOrder;
                EditorUtility.SetDirty(sortingComponent.sortingGroup);

                return;
            }

            Debug.LogFormat(
                "Update Sorting options on SpriteRenderer {0} - Sorting Layer from {1} to {2}, Sorting Order from {3} to {4}",
                sortingComponent.spriteRenderer.name, sortingComponent.spriteRenderer.sortingLayerName,
                sortingLayerName,
                sortingComponent.spriteRenderer.sortingOrder, newSortingOrder);

            Undo.RecordObject(sortingComponent.spriteRenderer, "apply sorting options");
            sortingComponent.spriteRenderer.sortingLayerName = sortingLayerName;
            sortingComponent.spriteRenderer.sortingOrder = newSortingOrder;
            EditorUtility.SetDirty(sortingComponent.spriteRenderer);
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