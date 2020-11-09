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

        //https://answers.unity.com/questions/684909/how-to-calculate-the-surface-area-of-a-irregular-p.html
        public float CalculatePolygonArea(Transform polygonTransform)
        {
            float surfaceArea = 0;
            if (!IsValidOutline())
            {
                return surfaceArea;
            }

            for (var i = 0; i < outlinePoints.Count; i++)
            {
                var point1 = polygonTransform.TransformPoint(outlinePoints[i]);
                var point2 = polygonTransform.TransformPoint(outlinePoints[(i + 1) % outlinePoints.Count]);

                var mulA = point1.x * point2.y;
                var mulB = point2.x * point1.y;
                surfaceArea += (mulA - mulB);
            }

            surfaceArea *= 0.5f;
            return Mathf.Abs(surfaceArea);
        }
    }
}