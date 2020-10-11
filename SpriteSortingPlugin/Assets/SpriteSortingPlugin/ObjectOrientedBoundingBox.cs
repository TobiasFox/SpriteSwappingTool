using System;
using SpriteSortingPlugin.SAT;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class ObjectOrientedBoundingBox
    {
        public string assetGuid;
        public string assetName;
        [SerializeField, HideInInspector] private Vector2[] localWorldPoints = new Vector2[4];
        [SerializeField] private Vector2[] originLocalWorldPoints = new Vector2[4];

        [SerializeField] private float zRotation;
        private Quaternion rotation;
        [SerializeField] private Bounds ownBounds;

        [SerializeField, HideInInspector] private Vector2[] axes;
        private Vector2[] points = new Vector2[4];
        public AlphaRectangleBorder alphaRectangleBorder;

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
            this.zRotation = zRotation;
            rotation = Quaternion.Euler(0, 0, zRotation);

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
            ownBounds.center = center;

            //TODO: update points
            UpdateLocalWorldPoints();
        }

        public void UpdateRotation(float zRotation)
        {
            rotation = Quaternion.Euler(0, 0, zRotation);

            //TODO: update points
            UpdateLocalWorldPoints();
        }

        public void UpdateBox(Transform transform)
        {
            ownBounds.center = transform.position;
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

        public void UpdateBosSizeWithBorder()
        {
            //TODO: consider only moving one side
            var width = alphaRectangleBorder.rightBorder / alphaRectangleBorder.pixelPerUnit -
                        alphaRectangleBorder.leftBorder / alphaRectangleBorder.pixelPerUnit;
            var height = alphaRectangleBorder.bottomBorder / alphaRectangleBorder.pixelPerUnit -
                         alphaRectangleBorder.topBorder / alphaRectangleBorder.pixelPerUnit;
            ownBounds.size = new Vector2(width, height);
            UpdateLocalWorldPoints();
        }

        private void Initialize()
        {
            originLocalWorldPoints[0] = new Vector3(ownBounds.min.x, ownBounds.max.y, 0); // top left 
            originLocalWorldPoints[1] = new Vector3(ownBounds.min.x, ownBounds.min.y, 0); // bottom left 
            originLocalWorldPoints[2] = new Vector3(ownBounds.max.x, ownBounds.min.y, 0); // bottom right
            originLocalWorldPoints[3] = new Vector3(ownBounds.max.x, ownBounds.max.y, 0); // top right

            UpdateLocalWorldPoints();

            //TODO: consider scale
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
    }
}