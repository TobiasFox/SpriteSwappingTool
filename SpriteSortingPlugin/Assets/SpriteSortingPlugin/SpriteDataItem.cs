using System;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SpriteDataItem
    {
        [SerializeField, HideInInspector] private string assetGuid;
        [SerializeField] private string assetName;

        public ObjectOrientedBoundingBox objectOrientedBoundingBox;
        public Vector2[] outlinePoints;
        public SpriteAnalysisData spriteAnalysisData;

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
            return outlinePoints != null && outlinePoints.Length >= 2;
        }

        //https://answers.unity.com/questions/684909/how-to-calculate-the-surface-area-of-a-irregular-p.html
        public float CalculatePolygonArea(Transform polygonTransform)
        {
            float area = 0;
            if (!IsValidOutline())
            {
                return area;
            }

            for (var i = 0; i < outlinePoints.Length; i++)
            {
                var point1 = polygonTransform.TransformPoint(outlinePoints[i]);
                var point2 = polygonTransform.TransformPoint(outlinePoints[(i + 1) % outlinePoints.Length]);

                var mulA = point1.x * point2.y;
                var mulB = point2.x * point1.y;
                area += (mulA - mulB);
            }

            area *= 0.5f;
            return Mathf.Abs(area);
        }
    }
}