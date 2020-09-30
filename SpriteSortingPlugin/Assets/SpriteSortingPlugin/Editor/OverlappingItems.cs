using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class OverlappingItems
    {
        private List<OverlappingItem> items;
        private OverlappingItem baseItem;
        private List<OverlappingItem> itemsForNewSortingGroup;

        private bool hasChangedLayer;
        private OverlappingItemComparer originIndexComparer;

        public List<OverlappingItem> Items => items;
        public OverlappingItem BaseItem => baseItem;
        public List<OverlappingItem> ItemsForNewSortingGroup => itemsForNewSortingGroup;
        public bool HasChangedLayer => hasChangedLayer;

        public OverlappingItems(OverlappingItem baseItem, List<OverlappingItem> items)
        {
            this.baseItem = baseItem;
            this.items = items;

            InitOverlappingItems(false);
        }

        public void Reset()
        {
            if (originIndexComparer == null)
            {
                originIndexComparer = new OverlappingItemComparer();
            }

            ArrayList.Adapter(items).Sort(originIndexComparer);

            InitOverlappingItems(true);
        }

        public void CheckChangedLayers()
        {
            if (baseItem.HasSortingLayerChanged())
            {
                hasChangedLayer = true;
                return;
            }

            foreach (var item in items)
            {
                if (!item.HasSortingLayerChanged())
                {
                    continue;
                }

                hasChangedLayer = true;
                return;
            }

            hasChangedLayer = false;
        }

        public bool UpdateSortingOrder(int currentIndex)
        {
            if (currentIndex < 0)
            {
                return false;
            }

            var element = items[currentIndex];
            element.UpdatePreviewSortingOrderWithExistingOrder();

            //TODO: update other elements sorting order when one is updated, e.g. when the -1 button is pressed
            // var index = currentIndex;
            // var indexToSwitch = GetIndexToSwitch(currentIndex);
            // if (indexToSwitch >= 0)
            // {
            //     reordableSpriteSortingList.list.RemoveAt(currentIndex);
            //     reordableSpriteSortingList.list.Insert(indexToSwitch, element);
            //     Debug.Log("switch " + currentIndex + " with " + indexToSwitch);
            //     index = indexToSwitch;
            // }

            var itemsHaveChanges = UpdateSurroundingItems(currentIndex);

            /*return indexToSwitch >= 0 || */
            // preview.UpdatePreviewEditor();
            return itemsHaveChanges;
        }

        private bool UpdateSurroundingItems(int currentIndex)
        {
            var itemsHaveChanges = false;

            //not called/used in current implementation
            for (var i = currentIndex - 1; i >= 0; i--)
            {
                var previousItem = items[i + 1];
                var currentItem = items[i];

                if (previousItem.sortingOrder == currentItem.sortingOrder)
                {
                    currentItem.sortingOrder++;
                    itemsHaveChanges = true;
                }
                else
                {
                    break;
                }
            }

            for (var i = currentIndex + 1; i < items.Count; i++)
            {
                var previousItem = items[i - 1];
                var currentItem = items[i];

                if (previousItem.sortingOrder == currentItem.sortingOrder)
                {
                    currentItem.sortingOrder--;
                    itemsHaveChanges = true;
                }
                else
                {
                    break;
                }
            }

            return itemsHaveChanges;
        }

        private int GetIndexToSwitch(int currentIndex)
        {
            int newSortingOrder = items[currentIndex].sortingOrder;
            int itemsCount = items.Count;

            if (currentIndex > 0 && items[currentIndex - 1].sortingOrder <= newSortingOrder)
            {
                int tempIndex = currentIndex;
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    int order = items[i].sortingOrder;
                    if (newSortingOrder >= order)
                    {
                        tempIndex--;
                    }
                    else
                    {
                        break;
                    }
                }

                return tempIndex;
            }

            if (currentIndex + 1 < itemsCount && items[currentIndex + 1].sortingOrder > newSortingOrder)
            {
                int tempIndex = currentIndex;

                for (int i = currentIndex + 1; i < itemsCount; i++)
                {
                    int order = items[i].sortingOrder;
                    if (newSortingOrder < order)
                    {
                        tempIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                return tempIndex;
            }

            return -1;
        }

        private void InitOverlappingItems(bool isReset)
        {
            int sortingLayerIndex =
                SortingLayerUtility.GetLayerNameIndex(items[0].originSortingLayer);

            for (var i = 0; i < items.Count; i++)
            {
                var overlappingItem = items[i];
                overlappingItem.sortingLayerDropDownIndex = sortingLayerIndex;

                if (!isReset)
                {
                    overlappingItem.OriginSortedIndex = i;
                }

                overlappingItem.sortingOrder = items.Count - (i + 1);

                overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();
                overlappingItem.sortingLayerName =
                    SortingLayerUtility.SortingLayerNames[overlappingItem.sortingLayerDropDownIndex];
                overlappingItem.UpdatePreviewSortingLayer();
            }
        }
    }
}