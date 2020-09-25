using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSorting
{
    [Serializable]
    public class PreviewItem
    {
        //TODO: consider using a composite pattern
        private List<SortingGroup> sortingGroups;
        private Transform previewItemParent;
        private Transform spriteRendererParent;
        private Transform sortingGroupParent;
        private List<PreviewItem> childrenSortingGroupElements;

        public PreviewItem(Transform parent)
        {
            previewItemParent = PreviewUtility.CreateGameObject(parent, "PreviewItem", true).transform;
            PreviewUtility.HideAndDontSaveGameObject(previewItemParent.gameObject);
        }

        public SortingGroup AddSortingGroup(SortingGroup newSortingGroup)
        {
            if (newSortingGroup == null)
            {
                return null;
            }

            if (sortingGroupParent == null)
            {
                sortingGroupParent = PreviewUtility.CreateGameObject(previewItemParent, "SortingGroupParent", true)
                    .transform;
                PreviewUtility.HideAndDontSaveGameObject(sortingGroupParent.gameObject);
            }

            var sortingGroupGO = PreviewUtility.CreateGameObject(sortingGroupParent, "SortingGroup", true);

            var sortingGroup = sortingGroupGO.AddComponent<SortingGroup>();
            sortingGroup.sortingLayerID = newSortingGroup.sortingLayerID;
            sortingGroup.sortingOrder = newSortingGroup.sortingOrder;

            PreviewUtility.HideAndDontSaveGameObject(sortingGroupGO);
            if (sortingGroups == null)
            {
                sortingGroups = new List<SortingGroup>();
            }

            sortingGroups.Add(sortingGroup);

            var child = new PreviewItem(sortingGroupGO.transform);

            if (childrenSortingGroupElements == null)
            {
                childrenSortingGroupElements = new List<PreviewItem>();
            }

            childrenSortingGroupElements.Add(child);
            return sortingGroup;
        }

        public SpriteRenderer AddSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return null;
            }

            if (spriteRendererParent == null)
            {
                spriteRendererParent =
                    PreviewUtility.CreateGameObject(previewItemParent, "SpriteRendererParent", true).transform;
                PreviewUtility.HideAndDontSaveGameObject(spriteRendererParent.gameObject);
            }

            var child = PreviewUtility.CreateGameObject(spriteRendererParent, spriteRenderer.name, true);
            var spriteRendererTransform = spriteRenderer.transform;

            child.transform.position = spriteRendererTransform.position;
            child.transform.rotation = spriteRendererTransform.rotation;
            child.transform.localScale = spriteRendererTransform.lossyScale;

            ComponentUtility.CopyComponent(spriteRenderer);
            ComponentUtility.PasteComponentAsNew(child);


            PreviewUtility.HideAndDontSaveGameObject(child);

            return child.GetComponent<SpriteRenderer>();
        }

        public bool TryGetSortingGroup(SortingGroup sortingGroupToSearch, out SortingGroup sortingGroup)
        {
            sortingGroup = null;
            if (childrenSortingGroupElements == null || sortingGroupToSearch == null)
            {
                return false;
            }

            var index = GetSortingGroupIndex(sortingGroupToSearch.sortingLayerID, sortingGroupToSearch.sortingOrder);
            if (index < 0)
            {
                return false;
            }

            sortingGroup = sortingGroups[index];
            return true;
        }

        private int GetSortingGroupIndex(int layerID, int sortingOrder)
        {
            for (var i = 0; i < sortingGroups.Count; i++)
            {
                var currentSortingGroup = sortingGroups[i];
                if (currentSortingGroup.sortingLayerID != layerID || currentSortingGroup.sortingOrder != sortingOrder)
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        public bool TryGetPreviewItem(SortingGroup originSortingGroup, out PreviewItem previewItem)
        {
            previewItem = null;
            if (childrenSortingGroupElements == null || originSortingGroup == null)
            {
                return false;
            }

            var index = GetSortingGroupIndex(originSortingGroup.sortingLayerID, originSortingGroup.sortingOrder);
            if (index < 0)
            {
                return false;
            }

            previewItem = childrenSortingGroupElements[index];
            return true;
        }
    }
}