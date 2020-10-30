using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SpriteDataItem
    {
        public string assetGuid;
        public string assetName;
        
        public ObjectOrientedBoundingBox objectOrientedBoundingBox;
        public List<Vector2> outlinePoints;

        public SpriteDataItem(string assetGuid, string assetName)
        {
            this.assetGuid = assetGuid;
            this.assetName = assetName;
        }
    }
}
