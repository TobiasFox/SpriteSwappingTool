using UnityEngine;

namespace SpriteSortingPlugin
{
    public static class SortingLayerUtility
    {
        private static string[] sortingLayerNames;

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