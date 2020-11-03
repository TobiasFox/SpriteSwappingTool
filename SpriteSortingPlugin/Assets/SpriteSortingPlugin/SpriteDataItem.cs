using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SpriteDataItem
    {
        [SerializeField] private string assetGuid;
        [SerializeField] private string assetName;

        public ObjectOrientedBoundingBox objectOrientedBoundingBox;
        public List<Vector2> outlinePoints;

        public string AssetGuid => assetGuid;
        public string AssetName => assetName;

        public SpriteDataItem(string assetGuid, string assetName)
        {
            this.assetGuid = assetGuid;
            this.assetName = assetName;
        }

        public bool IsValidOOBB()
        {
            return objectOrientedBoundingBox != null && objectOrientedBoundingBox.IsInitialized;
        }

        public bool IsValidOutline()
        {
            return outlinePoints != null && outlinePoints.Count >= 2;
        }
    }
}