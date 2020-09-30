using System;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SortingLayerUtility
    {
        private static string[] sortingLayerNames;

        public static string[] SortingLayerNames => sortingLayerNames;

        public static void UpdateSortingLayerNames()
        {
            sortingLayerNames = new string[SortingLayer.layers.Length];
            for (var i = 0; i < SortingLayer.layers.Length; i++)
            {
                sortingLayerNames[i] = SortingLayer.layers[i].name;
            }
        }

        public static int GetLayerNameIndex(int layerId)
        {
            if (sortingLayerNames == null)
            {
                UpdateSortingLayerNames();
            }

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
    }
}