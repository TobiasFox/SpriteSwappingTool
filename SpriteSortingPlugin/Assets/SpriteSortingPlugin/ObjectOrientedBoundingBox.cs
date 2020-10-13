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
        [SerializeField, HideInInspector] private Vector2[] localWorldPoints = new Vector2[4];
        [SerializeField] private Vector2[] originLocalWorldPoints = new Vector2[4];

        [SerializeField] private float zRotation;
        private Quaternion rotation;
        private Bounds ownBounds;
        [SerializeField] private Vector2 boundsCenter;
        [SerializeField] private Vector2 boundsSize;
        [SerializeField] private Vector2 boundsCenterOffset;

        [SerializeField, HideInInspector] private Vector2[] axes;
        private Vector2[] points = new Vector2[4];
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
                if (axes != null)
                {
                    return axes;
                }

                axes = new Vector2[2];
                axes[0] = Vector2.Perpendicular(localWorldPoints[3] - localWorldPoints[0]);
                axes[1] = Vector2.Perpendicular(localWorldPoints[0] - localWorldPoints[1]);

                return axes;
            }
        }

        public Vector2[] Points => points;
        public Bounds OwnBounds => ownBounds;

        public ObjectOrientedBoundingBox(Bounds bounds, float zRotation)
        {
            ownBounds = bounds;
            boundsCenter = bounds.center;
            this.zRotation = zRotation;
            rotation = Quaternion.Euler(0, 0, zRotation);
            alphaRectangleBorder = new AlphaRectangleBorder();

            Initialize();
        }

        public Projection ProjectAxis(Vector2 axis)
        {
            double min = Vector2.Dot(axis, localWorldPoints[0]);
            var max = min;

            for (var i = 1; i < localWorldPoints.Length; i++)
            {
                double p = Vector2.Dot(axis, localWorldPoints[i]);
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
            rotation = Quaternion.Euler(0, 0, transform.rotation.z);
            UpdateLocalWorldPoints();

            // Apply scaling
            // var localScale = transform.localScale;
            // for (int s = 0; s < sourcePoints.Length; s++)
            // {
            //     sourcePoints[s] = new Vector3(sourcePoints[s].x / localScale.x, sourcePoints[s].y / localScale.y, 0);
            // }

            // Transform points from local to world space
            for (int t = 0; t < points.Length; t++)
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

            boundsCenterOffset.x += convertedLeftBorder / 2f;
            boundsCenterOffset.x -=
                ((float) alphaRectangleBorder.rightBorder / (float) alphaRectangleBorder.pixelPerUnit) / 2f;

            boundsCenterOffset.y -=
                ((float) alphaRectangleBorder.bottomBorder / (float) alphaRectangleBorder.pixelPerUnit) / 2f;
            boundsCenterOffset.y += convertedTopBorder / 2f;

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
            var pivot = (Vector2) ownBounds.center;
            for (var i = 0; i < localWorldPoints.Length; i++)
            {
                var dir = originLocalWorldPoints[i] - pivot;
                dir = rotation * dir;
                localWorldPoints[i] = dir + pivot;
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