using System;
using SpriteSortingPlugin.SAT;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class ObjectOrientedBoundingBox : ISerializationCallbackReceiver
    {
        public string assetGuid;
        public string assetName;
        [SerializeField, HideInInspector] public Vector2[] localWorldPoints = new Vector2[4];
        [SerializeField] private Vector2[] originLocalWorldPoints = new Vector2[4];

        [SerializeField] public float zRotation;
        private Quaternion rotation;
        private Bounds ownBounds;
        [SerializeField] private Vector2 boundsCenter;
        [SerializeField] private Vector2 boundsSize;
        [SerializeField] private Vector2 boundsCenterOffset;

        [SerializeField] private Vector2[] axes;
        private Vector2[] points;
        [SerializeField] private AlphaRectangleBorder alphaRectangleBorder;
        [SerializeField, HideInInspector] private AlphaRectangleBorder originAlphaRectangleBorder;

        [SerializeField, HideInInspector] private bool isOriginAlphaRectangleBorderSet;

        public AlphaRectangleBorder AlphaRectangleBorder
        {
            get => alphaRectangleBorder;
            set
            {
                alphaRectangleBorder = value;

                if (!isOriginAlphaRectangleBorderSet)
                {
                    originAlphaRectangleBorder = (AlphaRectangleBorder) alphaRectangleBorder.Clone();
                    isOriginAlphaRectangleBorderSet = true;
                }
            }
        }

        public Vector2[] Axes
        {
            get
            {
                CheckValidWorldPoints();

                axes = new Vector2[2];
                axes[0] = Vector2.Perpendicular(points[3] - points[0]);
                axes[1] = Vector2.Perpendicular(points[0] - points[1]);

                return axes;
            }
        }

        public Vector2[] Points
        {
            get
            {
                CheckValidWorldPoints();
                return points;
            }
        }

        public Bounds OwnBounds => ownBounds;
        public Vector2 BoundsCenterOffset => boundsCenterOffset;

        public ObjectOrientedBoundingBox(Bounds bounds, float zRotation)
        {
            ownBounds = bounds;
            boundsCenter = bounds.center;
            this.zRotation = zRotation;
            rotation = Quaternion.Euler(0, 0, zRotation);
            alphaRectangleBorder = new AlphaRectangleBorder();
            points = new Vector2[localWorldPoints.Length];

            Initialize();
        }

        public ObjectOrientedBoundingBox(AlphaRectangleBorder alphaRectangleBorder, float zRotation = 0)
        {
            this.zRotation = zRotation;
            rotation = Quaternion.Euler(0, 0, zRotation);
            this.alphaRectangleBorder = alphaRectangleBorder;
            points = new Vector2[localWorldPoints.Length];
            originAlphaRectangleBorder = (AlphaRectangleBorder) alphaRectangleBorder.Clone();

            var width = alphaRectangleBorder.rightBorder - alphaRectangleBorder.leftBorder;
            var height = alphaRectangleBorder.bottomBorder - alphaRectangleBorder.topBorder;
            boundsCenter = Vector2.zero;
            ownBounds = new Bounds(boundsCenter, new Vector2(width, height));

            Initialize();
        }

        public Projection ProjectAxis(Vector2 axis)
        {
            CheckValidWorldPoints();

            double min = Vector2.Dot(axis, points[0]);
            var max = min;

            for (var i = 1; i < points.Length; i++)
            {
                double p = Vector2.Dot(axis, points[i]);
                if (p < min)
                {
                    min = p;
                }
                else if (p > max)
                {
                    max = p;
                }
            }

            var proj = new Projection(min, max);
            return proj;
        }

        public void UpdateCenter(Vector2 center)
        {
            boundsCenter = center;
            ownBounds.center = center + boundsCenterOffset;

            UpdateLocalWorldPoints();
        }

        public void UpdateRotation(float zRotation)
        {
            rotation = Quaternion.Euler(0, 0, zRotation);

            UpdateLocalWorldPoints();
        }

        public void UpdateBox(Transform transform)
        {
            boundsCenter = transform.position;
            ownBounds.center = boundsCenter + boundsCenterOffset;
            zRotation = transform.rotation.z;
            rotation = Quaternion.Euler(0, 0, zRotation);
            UpdateLocalWorldPoints();

            // Apply scaling
            // var localScale = transform.localScale;
            // for (int s = 0; s < sourcePoints.Length; s++)
            // {
            //     sourcePoints[s] = new Vector3(sourcePoints[s].x / localScale.x, sourcePoints[s].y / localScale.y, 0);
            // }

            //TODO: check initialization of point array
            if (points == null || points.Length != 4)
            {
                points = new Vector2[4];
            }

            // Transform points from local to world space
            for (int t = 0; t < localWorldPoints.Length; t++)
            {
                points[t] = transform.TransformPoint(localWorldPoints[t]);
            }
        }

        public void UpdateBoxSizeWithBorder()
        {
            var convertedLeftBorder = alphaRectangleBorder.leftBorder / alphaRectangleBorder.pixelPerUnit;
            var convertedTopBorder = alphaRectangleBorder.topBorder / alphaRectangleBorder.pixelPerUnit;
            var convertedRightBorder = (alphaRectangleBorder.spriteWidth - alphaRectangleBorder.rightBorder) /
                                       alphaRectangleBorder.pixelPerUnit;
            var convertedBottomBorder = (alphaRectangleBorder.spriteHeight - alphaRectangleBorder.bottomBorder) /
                                        alphaRectangleBorder.pixelPerUnit;

            var width = convertedRightBorder - convertedLeftBorder;
            var height = convertedBottomBorder - convertedTopBorder;
            ownBounds.size = new Vector2(width, height);

            boundsCenterOffset = Vector2.zero;

            boundsCenterOffset.x -= convertedLeftBorder / 2f;
            boundsCenterOffset.x +=
                ((float) alphaRectangleBorder.rightBorder / (float) alphaRectangleBorder.pixelPerUnit) / 2f;

            boundsCenterOffset.y +=
                ((float) alphaRectangleBorder.bottomBorder / (float) alphaRectangleBorder.pixelPerUnit) / 2f;
            boundsCenterOffset.y -= convertedTopBorder / 2f;

            ownBounds.center = boundsCenter + boundsCenterOffset;
            UpdateLocalWorldPoints();
        }

        public void ResetAlphaRectangleBorder()
        {
            alphaRectangleBorder = (AlphaRectangleBorder) originAlphaRectangleBorder.Clone();
            boundsCenterOffset = Vector2.zero;
            ownBounds.center = boundsCenter;
        }

        private void Initialize()
        {
            originLocalWorldPoints[0] = new Vector3(ownBounds.min.x, ownBounds.max.y, 0); // top left 
            originLocalWorldPoints[1] = new Vector3(ownBounds.min.x, ownBounds.min.y, 0); // bottom left 
            originLocalWorldPoints[2] = new Vector3(ownBounds.max.x, ownBounds.min.y, 0); // bottom right
            originLocalWorldPoints[3] = new Vector3(ownBounds.max.x, ownBounds.max.y, 0); // top right

            //TODO: consider scale
            UpdateLocalWorldPoints();
        }

        private void UpdateLocalWorldPoints()
        {
            //apply border
            var convertedLeftBorder = (alphaRectangleBorder.leftBorder - originAlphaRectangleBorder.leftBorder) /
                                      alphaRectangleBorder.pixelPerUnit;
            var convertedTopBorder = (alphaRectangleBorder.topBorder - originAlphaRectangleBorder.topBorder) /
                                     alphaRectangleBorder.pixelPerUnit;
            var convertedRightBorder = (alphaRectangleBorder.rightBorder - originAlphaRectangleBorder.rightBorder) /
                                       alphaRectangleBorder.pixelPerUnit;
            var convertedBottomBorder = (alphaRectangleBorder.bottomBorder - originAlphaRectangleBorder.bottomBorder) /
                                        alphaRectangleBorder.pixelPerUnit;

            localWorldPoints[0] = new Vector3(originLocalWorldPoints[0].x + convertedLeftBorder,
                originLocalWorldPoints[0].y - convertedTopBorder, 0); // top left 

            localWorldPoints[1] = new Vector3(originLocalWorldPoints[1].x + convertedLeftBorder,
                originLocalWorldPoints[1].y + convertedBottomBorder, 0); // bottom left 

            localWorldPoints[2] = new Vector3(originLocalWorldPoints[2].x - convertedRightBorder,
                originLocalWorldPoints[2].y + convertedBottomBorder, 0); // bottom right

            localWorldPoints[3] = new Vector3(originLocalWorldPoints[3].x - convertedRightBorder,
                originLocalWorldPoints[3].y - convertedTopBorder, 0); // top right

            //apply rotation
            var pivot = (Vector2) ownBounds.center;
            for (var i = 0; i < localWorldPoints.Length; i++)
            {
                var dir = localWorldPoints[i] - pivot;
                dir = rotation * dir;
                localWorldPoints[i] = dir + pivot;
            }
        }

        private void CheckValidWorldPoints()
        {
            if (points == null || points.Length != 4)
            {
                points = new Vector2[4];
                UpdateLocalWorldPoints();
            }
        }

        public void OnBeforeSerialize()
        {
            boundsCenter = (Vector2) ownBounds.center - boundsCenterOffset;
            boundsSize = ownBounds.size;
        }

        public void OnAfterDeserialize()
        {
            ownBounds = new Bounds(boundsCenter + boundsCenterOffset, boundsSize);
        }
    }
}