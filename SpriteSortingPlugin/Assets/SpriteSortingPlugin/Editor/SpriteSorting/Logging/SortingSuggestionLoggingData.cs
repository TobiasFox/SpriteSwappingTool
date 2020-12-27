#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

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