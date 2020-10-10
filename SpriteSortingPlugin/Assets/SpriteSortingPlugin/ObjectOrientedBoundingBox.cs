using System;
using SpriteSortingPlugin.SAT;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class ObjectOrientedBoundingBox
    {
        public string assetGuid;
        [SerializeField] private Vector2[] localWorldPoints = new Vector2[4];
        [SerializeField] private Vector2[] originLocalWorldPoints = new Vector2[4];

        [SerializeField] private float zRotation;
        private Quaternion rotation;
        private Bounds ownBounds;

        [SerializeField] private Vector2[] axes;
        private Vector2[] points = new Vector2[4];

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
        }

        public void UpdateRotation(float zRotation)
        {
            rotation = Quaternion.Euler(0, 0, zRotation);

            //TODO: update points
        }

        private void Initialize()
        {
            originLocalWorldPoints[0] = new Vector3(ownBounds.min.x, ownBounds.max.y, 0); // top left 
            originLocalWorldPoints[1] = new Vector3(ownBounds.min.x, ownBounds.min.y, 0); // bottom left 
            originLocalWorldPoints[2] = new Vector3(ownBounds.max.x, ownBounds.min.y, 0); // bottom right
            originLocalWorldPoints[3] = new Vector3(ownBounds.max.x, ownBounds.max.y, 0); // top right

            var pivot = (Vector2) ownBounds.center;
            for (var i = 0; i < localWorldPoints.Length; i++)
            {
                var dir = originLocalWorldPoints[i] - pivot;
                dir = rotation * dir;
                localWorldPoints[i] = dir + pivot;
            }

            //TODO: consider scale
        }
    }
}