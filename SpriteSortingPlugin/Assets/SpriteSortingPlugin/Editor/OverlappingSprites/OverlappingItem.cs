using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.OverlappingSprites
{
    [Serializable]
    public class OverlappingItem : SortingComponent
    {
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
        public int AutoSortingOrder { get; set; }
        public bool IsBaseItem { get; private set; }
        public string SpriteAssetGuid { get; private set; }

        public OverlappingItem(SpriteRenderer originSpriteRenderer) : base(originSpriteRenderer,
            originSpriteRenderer.GetComponentInParent<SortingGroup>())
        {
            Init();
        }

        public OverlappingItem(SortingComponent sortingComponent, bool isBaseItem = false) : base(
            sortingComponent.OriginSpriteRenderer, sortingComponent.OutmostSortingGroup)
        {
            Init();
            IsBaseItem = isBaseItem;

            SpriteAssetGuid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(OriginSpriteRenderer.sprite.GetInstanceID()));
        }

        private void Init()
        {
            originSortingLayer = OriginSortingLayer;
            originSortingOrder = OriginSortingOrder;
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

            if (OutmostSortingGroup != null)
            {
                Debug.LogFormat(
                    "Update Sorting options on Sorting Group {0} - Sorting Layer from {1} to {2}, Sorting Order from {3} to {4}",
                    OutmostSortingGroup.name, OutmostSortingGroup.sortingLayerName, sortingLayerName,
                    OutmostSortingGroup.sortingOrder, newSortingOrder);

                Undo.RecordObject(OutmostSortingGroup, "apply sorting options");
                OutmostSortingGroup.sortingLayerName = sortingLayerName;
                OutmostSortingGroup.sortingOrder = newSortingOrder;
                EditorUtility.SetDirty(OutmostSortingGroup);

                return;
            }

            Debug.LogFormat(
                "Update Sorting options on SpriteRenderer {0} - Sorting Layer from {1} to {2}, Sorting Order from {3} to {4}",
                OriginSpriteRenderer.name, OriginSpriteRenderer.sortingLayerName, sortingLayerName,
                OriginSpriteRenderer.sortingOrder, newSortingOrder);

            Undo.RecordObject(OriginSpriteRenderer, "apply sorting options");
            OriginSpriteRenderer.sortingLayerName = sortingLayerName;
            OriginSpriteRenderer.sortingOrder = newSortingOrder;
            EditorUtility.SetDirty(OriginSpriteRenderer);
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