using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SpriteSorting
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
        // public GameObject sortingGroupChildren;
        public int sortingOrder;
        public int sortingLayer;

        public bool IsItemSelected { get; set; }
        public int OriginSortedIndex { get; set; }

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

        public void UpdatePreviewSortingOrderWithExistingOrder()
        {
            if (previewSortingGroup != null)
            {
                previewSortingGroup.sortingOrder = originSortingOrder + sortingOrder;
            }
            else if (previewSpriteRenderer != null)
            {
                previewSpriteRenderer.sortingOrder = originSortingOrder + sortingOrder;
            }
        }

        public void UpdatePreviewSortingLayer(string sortingLayerName)
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

        public void GeneratePreview(Transform parent)
        {
            var spriteGameObject = new GameObject(originSpriteRenderer.name)
            {
                hideFlags = HideFlags.DontSave
            };
            ComponentUtility.CopyComponent(originSpriteRenderer.transform);
            ComponentUtility.PasteComponentValues(spriteGameObject.transform);

            if (originSpriteRenderer != null)
            {
                ComponentUtility.CopyComponent(originSpriteRenderer);
                ComponentUtility.PasteComponentAsNew(spriteGameObject);
                previewSpriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();
                previewSpriteRenderer.sortingOrder = sortingOrder;
            }

            if (originSortingGroup != null)
            {
                ComponentUtility.CopyComponent(originSortingGroup);
                ComponentUtility.PasteComponentAsNew(spriteGameObject);
                previewSortingGroup = spriteGameObject.GetComponent<SortingGroup>();

                //TODO: get all children of sorting group
                // sortingGroupChildren = new GameObject("sortingGroupChildren")
                // {
                //     hideFlags = HideFlags.DontSave
                // };

                // var children = originSortingGroup.GetComponentsInChildren<SpriteRenderer>();
                // foreach (var child in children)
                // {
                //     if (child == originSpriteRenderer)
                //     {
                //         continue;
                //     }
                //     var childGameObject = new GameObject(child.name)
                //     {
                //         hideFlags = HideFlags.DontSave
                //     };
                //     ComponentUtility.CopyComponent(child.transform);
                //     ComponentUtility.PasteComponentValues(childGameObject.transform);
                // }
            }

            spriteGameObject.transform.SetParent(parent);
            spriteGameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        public void CleanUpPreview()
        {
            Object.DestroyImmediate(originSpriteRenderer.gameObject);
        }
    }
}