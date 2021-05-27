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

using UnityEngine;

namespace SpriteSwappingPlugin.SpriteSwappingDetector
{
    public static class SortingLayerUtility
    {
        public const string SortingLayerNameDefault = "Default";
        private static string[] sortingLayerNames;
        private static GUIContent[] sortingLayerGuiContents;

        public static string[] SortingLayerNames
        {
            get
            {
                // if (sortingLayerNames == null)
                // {
                //     UpdateSortingLayerNames(out var lastSortingLayerNames);
                // }

                return sortingLayerNames;
            }
        }

        public static GUIContent[] SortingLayerGuiContents
        {
            get
            {
                if (sortingLayerGuiContents == null)
                {
                    sortingLayerGuiContents = new GUIContent[sortingLayerNames.Length];
                    for (var i = 0; i < sortingLayerNames.Length; i++)
                    {
                        sortingLayerGuiContents[i] = new GUIContent(sortingLayerNames[i]);
                    }
                }

                return sortingLayerGuiContents;
            }
        }

        public static bool UpdateSortingLayerNames()
        {
            if (sortingLayerNames == null || SortingLayer.layers.Length != sortingLayerNames.Length)
            {
                sortingLayerNames = new string[SortingLayer.layers.Length];
                for (var i = 0; i < SortingLayer.layers.Length; i++)
                {
                    sortingLayerNames[i] = SortingLayer.layers[i].name;
                }

                return true;
            }

            var isSortingLayerArrayHasChanged = false;
            for (var i = 0; i < SortingLayer.layers.Length; i++)
            {
                var sortingLayer = SortingLayer.layers[i];
                var layerName = sortingLayerNames[i];
                if (!sortingLayer.name.Equals(layerName))
                {
                    isSortingLayerArrayHasChanged = true;
                }

                sortingLayerNames[i] = sortingLayer.name;
            }

            return isSortingLayerArrayHasChanged;
        }

        public static int GetLayerNameIndex(int layerId)
        {
            // if (sortingLayerNames == null)
            // {
            //     UpdateSortingLayerNames(out var lastSortingLayerNames);
            // }

            var layerNameToFind = SortingLayer.IDToName(layerId);
            for (var i = 0; i < sortingLayerNames.Length; i++)
            {
                if (sortingLayerNames[i].Equals(layerNameToFind))
                {
                    return i;
                }
            }

            return 0;
        }

        public static int GetLayerNameIndex(string layerName)
        {
            if (layerName == null)
            {
                return -1;
            }

            // if (sortingLayerNames == null)
            // {
            //     UpdateSortingLayerNames(out var lastSortingLayerNames);
            // }

            for (var i = 0; i < sortingLayerNames.Length; i++)
            {
                if (sortingLayerNames[i].Equals(layerName))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}