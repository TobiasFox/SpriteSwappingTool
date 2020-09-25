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
        private List<SortingGroup> sortingGroups;
        private Transform previewItemParent;
        private Transform spriteRendererParent;
        private Transform sortingGroupParent;
        private List<PreviewItem> childrenSortingGroupElements;

        public PreviewItem(Transform parent)
        {
            previewItemParent = CreateGameObject(parent, "PreviewItem", true).transform;
            HideAndDontSaveGameObject(previewItemParent.gameObject);
        }

        public PreviewItem AddSortingGroup(SortingGroup newSortingGroup)
        {
            if (newSortingGroup == null)
            {
                return null;
            }

            if (sortingGroupParent == null)
            {
                sortingGroupParent = CreateGameObject(previewItemParent, "SortingGroupParent", true).transform;
                HideAndDontSaveGameObject(sortingGroupParent.gameObject);
            }

            var sortingGroupGO = CreateGameObject(sortingGroupParent, "SortingGroup", true);

            var sortingGroup = sortingGroupGO.AddComponent<SortingGroup>();
            sortingGroup.sortingLayerID = newSortingGroup.sortingLayerID;
            sortingGroup.sortingOrder = newSortingGroup.sortingOrder;

            HideAndDontSaveGameObject(sortingGroupGO);
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
            return child;
        }

        public void AddSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (spriteRendererParent == null)
            {
                spriteRendererParent =
                    CreateGameObject(previewItemParent, "SpriteRendererParent", true).transform;
                HideAndDontSaveGameObject(spriteRendererParent.gameObject);
            }

            var child = CreateGameObject(spriteRendererParent, spriteRenderer.name, true);
            var spriteRendererTransform = spriteRenderer.transform;

            child.transform.position = spriteRendererTransform.position;
            child.transform.rotation = spriteRendererTransform.rotation;
            child.transform.localScale = spriteRendererTransform.lossyScale;

            ComponentUtility.CopyComponent(spriteRenderer);
            ComponentUtility.PasteComponentAsNew(child);


            HideAndDontSaveGameObject(child);
        }

        public bool TryGetSortingGroup(SortingGroup originSortingGroup, out PreviewItem preview)
        {
            preview = null;
            if (childrenSortingGroupElements == null)
            {
                return false;
            }

            for (var i = 0; i < sortingGroups.Count; i++)
            {
                var childSortingGroup = sortingGroups[i];

                if (childSortingGroup.sortingLayerID != originSortingGroup.sortingLayerID ||
                    childSortingGroup.sortingOrder != originSortingGroup.sortingOrder)
                {
                    continue;
                }

                preview = childrenSortingGroupElements[i];
                return true;
            }

            return false;
        }

        //TODO: maybe a pool?
        public static GameObject CreateGameObject(Transform parent, string name, bool isDontSave)
        {
            var previewItem = new GameObject(name)
            {
                hideFlags = isDontSave ? HideFlags.DontSave : HideFlags.None
            };

            previewItem.transform.SetParent(parent);

            return previewItem;
        }

        public static void HideAndDontSaveGameObject(GameObject gameObject)
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}