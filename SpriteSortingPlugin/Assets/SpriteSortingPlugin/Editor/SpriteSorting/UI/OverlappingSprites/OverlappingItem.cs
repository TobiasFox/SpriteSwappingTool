using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites
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

        public void ApplySortingOption()
        {
            var newSortingOrder = sortingOrder;
            if (isUsingRelativeSortingOrder)
            {
                newSortingOrder += originSortingOrder;
            }

            if (SortingComponent.SortingGroup != null)
            {
                ApplySortingOptionsToSortingGroup(newSortingOrder);

                return;
            }

            ApplySortingOptionsToSpriteRenderer(newSortingOrder);
        }

        private void ApplySortingOptionsToSpriteRenderer(int newSortingOrder)
        {
            var isSortingLayerIdentical = SortingComponent.SpriteRenderer.sortingLayerName.Equals(sortingLayerName);
            var isSortingOrderIdentical = SortingComponent.SpriteRenderer.sortingOrder == newSortingOrder;

            if (isSortingLayerIdentical && isSortingOrderIdentical)
            {
                return;
            }

            var message = "Update sorting options on SpriteRenderer " + sortingComponent.SpriteRenderer.name + " - ";

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
            if (!isSortingLayerIdentical)
            {
                SortingComponent.SpriteRenderer.sortingLayerName = sortingLayerName;
            }

            if (!isSortingOrderIdentical)
            {
                SortingComponent.SpriteRenderer.sortingOrder = newSortingOrder;
            }

            EditorUtility.SetDirty(SortingComponent.SpriteRenderer);

            Debug.Log(message);
        }

        private void ApplySortingOptionsToSortingGroup(int newSortingOrder)
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

            var message = "Update sorting options on SortingGroup " + sortingComponent.SortingGroup.name + " - ";

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
            if (!isSortingLayerIdentical)
            {
                SortingComponent.SortingGroup.sortingLayerName = sortingLayerName;
            }

            if (!isSortingOrderIdentical)
            {
                SortingComponent.SortingGroup.sortingOrder = newSortingOrder;
            }

            EditorUtility.SetDirty(SortingComponent.SortingGroup);

            Debug.Log(message);
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
                   ", originSortedIndex:" + originSortedIndex + "]";
        }
    }
}