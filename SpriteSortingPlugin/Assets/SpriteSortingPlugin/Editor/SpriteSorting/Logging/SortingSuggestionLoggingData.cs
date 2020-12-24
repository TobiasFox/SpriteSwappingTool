using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteSorting.UI.OverlappingSprites;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.Logging
{
    [Serializable]
    public class SortingSuggestionLoggingData
    {
        public int question;

        public List<DefaultSortingCriterionData> defaultCriterionDataList =
            new List<DefaultSortingCriterionData>();

        public List<PrimaryColorSortingCriterionData> primaryColorCriterionDataList =
            new List<PrimaryColorSortingCriterionData>();

        public List<ContainmentSortingCriterionData> containmentCriterionDataList =
            new List<ContainmentSortingCriterionData>();

        public OverlappingItemLoggingData[] overlappingItems;
        public SortingLayerLoggingData[] sortingLayers;

        public List<SortingSuggestionModificationData> modificationList = new List<SortingSuggestionModificationData>();

        public void Init(List<OverlappingItem> overlappingItems, SortingCriterionData[] sortingCriterionDataArray)
        {
            foreach (var criterionData in sortingCriterionDataArray)
            {
                switch (criterionData)
                {
                    case PrimaryColorSortingCriterionData tempPrimaryColorSortingCriterionData:

                        primaryColorCriterionDataList.Add(tempPrimaryColorSortingCriterionData);
                        break;
                    case ContainmentSortingCriterionData tempContainmentSortingCriterionData:
                        containmentCriterionDataList.Add(tempContainmentSortingCriterionData);
                        break;
                    case DefaultSortingCriterionData defaultSortingCriterionData:
                    {
                        defaultCriterionDataList.Add(defaultSortingCriterionData);
                        break;
                    }
                }
            }

            this.overlappingItems = new OverlappingItemLoggingData[overlappingItems.Count];
            for (var i = 0; i < overlappingItems.Count; i++)
            {
                var overlappingItem = overlappingItems[i];
                var tempOverlappingItemLoggingData = new OverlappingItemLoggingData
                {
                    isBaseItem = overlappingItem.IsBaseItem,
                    originSortingOrder = overlappingItem.originSortingOrder,
                    originAutoSortingOrder = overlappingItem.originAutoSortingOrder,
                    spriteRendererName = overlappingItem.SortingComponent.SpriteRenderer.name,
                    originSortingLayerIndex = SortingLayerUtility.GetLayerNameIndex(overlappingItem.originSortingLayer)
                };

                this.overlappingItems[i] = tempOverlappingItemLoggingData;
            }

            sortingLayers = new SortingLayerLoggingData[SortingLayer.layers.Length];
            for (var i = 0; i < SortingLayer.layers.Length; i++)
            {
                var currentLayer = SortingLayer.layers[i];
                sortingLayers[i] = new SortingLayerLoggingData()
                    {name = currentLayer.name, layerIndex = currentLayer.value};
            }
        }

        public void AddModification(SortingSuggestionModificationData data)
        {
            modificationList.Add(data);
        }
    }
}