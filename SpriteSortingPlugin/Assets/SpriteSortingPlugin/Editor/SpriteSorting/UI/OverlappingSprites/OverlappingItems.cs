using System;
using System.Collections;
using System.Collections.Generic;

namespace SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites
{
    [Serializable]
    public class OverlappingItems
    {
        private List<OverlappingItem> items;
        private OverlappingItem baseItem;

        private bool hasChangedLayer;
        private OverlappingItemIndexComparer originIndexComparer;
        private OverlappingItemIdentityComparer overlappingItemIdentityComparer;
        private bool isAlreadySorted;
        private bool isContinuouslyReflectingSortingOptionsInScene;

        public List<OverlappingItem> Items => items;
        public OverlappingItem BaseItem => baseItem;
        public bool HasChangedLayer => hasChangedLayer;
        public bool IsContinuouslyReflectingSortingOptionsInScene => isContinuouslyReflectingSortingOptionsInScene;

        public OverlappingItems(OverlappingItem baseItem, List<OverlappingItem> items, bool isAlreadySorted = false)
        {
            this.baseItem = baseItem;
            this.items = items;
            this.isAlreadySorted = isAlreadySorted;

            InitOverlappingItems(false);
        }

        public void Reset()
        {
            if (originIndexComparer == null)
            {
                originIndexComparer = new OverlappingItemIndexComparer();
            }

            items.Sort(originIndexComparer);

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

        public void ConvertSortingOrder(bool isRelative)
        {
            foreach (var overlappingItem in items)
            {
                overlappingItem.ConvertSortingOrder(isRelative);
            }
        }

        public void OnChangedSortingLayerOrder()
        {
            foreach (var overlappingItem in items)
            {
                overlappingItem.sortingLayerDropDownIndex =
                    SortingLayerUtility.GetLayerNameIndex(overlappingItem.sortingLayerName);
            }

            if (overlappingItemIdentityComparer == null)
            {
                overlappingItemIdentityComparer = new OverlappingItemIdentityComparer();
            }

            ArrayList.Adapter(items).Sort(overlappingItemIdentityComparer);
        }

        public void ReOrderItem(int newIndex)
        {
            var itemWithNewIndex = items[newIndex];

            var isAdjustingSortingOrderUpwards = newIndex < items.Count / 2;
            var lastItem = items[newIndex + (isAdjustingSortingOrderUpwards ? 1 : -1)];

            if (itemWithNewIndex.SortingComponent.IsOverlapping(lastItem.SortingComponent))
            {
                if (isAdjustingSortingOrderUpwards && itemWithNewIndex.sortingOrder <= lastItem.sortingOrder)
                {
                    itemWithNewIndex.sortingOrder = lastItem.sortingOrder + 1;
                }
                else if (!isAdjustingSortingOrderUpwards && itemWithNewIndex.sortingOrder >= lastItem.sortingOrder)
                {
                    itemWithNewIndex.sortingOrder = lastItem.sortingOrder - 1;
                }
            }

            UpdateSortingOrder(newIndex);
        }

        public void UpdateSortingOrder(int currentIndex)
        {
            if (currentIndex < 0)
            {
                return;
            }

            var element = items[currentIndex];
            element.UpdatePreviewSortingOrderWithExistingOrder();

            UpdateSurroundingItems(currentIndex);
        }

        public void UpdateSortingLayer(int currentIndex, out int newIndexInList)
        {
            newIndexInList = -1;
            if (currentIndex < 0)
            {
                return;
            }

            var element = items[currentIndex];
            element.UpdatePreviewSortingLayer();

            if (overlappingItemIdentityComparer == null)
            {
                overlappingItemIdentityComparer = new OverlappingItemIdentityComparer();
            }

            items.Sort(overlappingItemIdentityComparer);

            newIndexInList = items.IndexOf(element);
            UpdateSurroundingItems(newIndexInList);
        }

        public bool ContainsSortingGroupsOrSpriteRenderersInstanceId(int instanceId)
        {
            foreach (var overlappingItem in items)
            {
                if (overlappingItem.SortingComponent.GetInstanceId() == instanceId)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateSurroundingItems(int currentIndex)
        {
            UpdateSurroundingItemsUpwards(currentIndex);
            UpdateSurroundingItemsDownwards(currentIndex);
        }

        private void UpdateSurroundingItemsDownwards(int currentIndex)
        {
            var currentItemToCompare = items[currentIndex];
            var layerName = currentItemToCompare.sortingLayerName;

            for (var i = currentIndex + 1; i < items.Count; i++)
            {
                var currentItem = items[i];

                if (!currentItem.sortingLayerName.Equals(layerName))
                {
                    continue;
                }

                var isOverlapping =
                    currentItem.SortingComponent.IsOverlapping(currentItemToCompare.SortingComponent);

                if (!isOverlapping || currentItemToCompare.sortingOrder > currentItem.sortingOrder)
                {
                    continue;
                }

                currentItem.sortingOrder = currentItemToCompare.sortingOrder - 1;
                currentItem.UpdatePreviewSortingOrderWithExistingOrder();
                currentItemToCompare = currentItem;
            }
        }

        public void ApplySortingOption(bool isContinuous = false)
        {
            foreach (var overlappingItem in items)
            {
                overlappingItem.ApplySortingOption(isContinuous);
            }

            if (isContinuous)
            {
                isContinuouslyReflectingSortingOptionsInScene = true;
            }
        }

        public void RestoreSpriteRendererSortingOptions()
        {
            if (!isContinuouslyReflectingSortingOptionsInScene)
            {
                return;
            }

            isContinuouslyReflectingSortingOptionsInScene = false;

            foreach (var overlappingItem in items)
            {
                overlappingItem.RestoreSpriteRendererSortingOptions();
            }
        }

        private void UpdateSurroundingItemsUpwards(int currentIndex)
        {
            var currentItemToCompare = items[currentIndex];
            var layerName = currentItemToCompare.sortingLayerName;

            for (var i = currentIndex - 1; i >= 0; i--)
            {
                var currentItem = items[i];

                if (!currentItem.sortingLayerName.Equals(layerName))
                {
                    continue;
                }

                var isOverlapping =
                    currentItem.SortingComponent.IsOverlapping(currentItemToCompare.SortingComponent);

                if (!isOverlapping || currentItemToCompare.sortingOrder < currentItem.sortingOrder)
                {
                    continue;
                }

                currentItem.sortingOrder = currentItemToCompare.sortingOrder + 1;
                currentItem.UpdatePreviewSortingOrderWithExistingOrder();
                currentItemToCompare = currentItem;
            }
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

        private void SwitchItems(int oldIndex, int newIndex)
        {
            var itemWithNewIndex = items[newIndex];
            var itemWithOldIndex = items[oldIndex];

            var tempSortingOrder = itemWithNewIndex.sortingOrder;
            var tempLayerId = itemWithNewIndex.sortingLayerDropDownIndex;

            itemWithNewIndex.sortingOrder = itemWithOldIndex.sortingOrder;
            itemWithNewIndex.sortingLayerDropDownIndex = itemWithOldIndex.sortingLayerDropDownIndex;

            itemWithOldIndex.sortingOrder = tempSortingOrder;
            itemWithOldIndex.sortingLayerDropDownIndex = tempLayerId;

            itemWithNewIndex.UpdatePreview();
            itemWithOldIndex.UpdatePreview();
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
                    overlappingItem.originSortedIndex = i;
                }

                if (!isAlreadySorted)
                {
                    overlappingItem.sortingOrder = items.Count - (i + 1);
                }
                else
                {
                    overlappingItem.sortingOrder = overlappingItem.originAutoSortingOrder;
                }

                overlappingItem.UpdatePreviewSortingOrderWithExistingOrder();
                overlappingItem.sortingLayerName =
                    SortingLayerUtility.SortingLayerNames[overlappingItem.sortingLayerDropDownIndex];
                overlappingItem.UpdatePreviewSortingLayer();
            }
        }
    }
}