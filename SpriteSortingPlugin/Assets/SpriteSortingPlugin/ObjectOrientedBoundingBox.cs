using System;
using SpriteSortingPlugin.SAT;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class ObjectOrientedBoundingBox : ISerializationCallbackReceiver, ICloneable
    {
        [SerializeField] private bool isInitialized;
        [SerializeField, HideInInspector] private Vector2[] localWorldPoints = new Vector2[4];
        [SerializeField] private Vector2[] originLocalWorldPoints = new Vector2[4];

        [SerializeField] public float zRotation;
        private Quaternion rotation;
        private Bounds ownBounds;
        [SerializeField] private Vector2 boundsCenter;
        [SerializeField] private Vector2 boundsSize;
        [SerializeField] private Vector2 boundsCenterOffset;

        private Vector2 lastGlobalScale;
        private Vector2[] axes;
        private Vector2[] points;
        [SerializeField] private AlphaRectangleBorder alphaRectangleBorder;
        [SerializeField, HideInInspector] private AlphaRectangleBorder originAlphaRectangleBorder;

        public bool IsInitialized => isInitialized;

        public AlphaRectangleBorder AlphaRectangleBorder
        {
            get => alphaRectangleBorder;
            set => alphaRectangleBorder = value;
        }

        public Vector2[] Axes => axes;
        public Vector2[] Points => points;
        public Vector2[] LocalWorldPoints => localWorldPoints;
        public Vector2 BoundsCenterOffset => boundsCenterOffset;

        public ObjectOrientedBoundingBox(AlphaRectangleBorder alphaRectangleBorder, Vector2 center, float zRotation = 0)
        {
            this.zRotation = zRotation;
            rotation = Quaternion.Euler(0, 0, zRotation);
            this.alphaRectangleBorder = alphaRectangleBorder;
            originAlphaRectangleBorder = (AlphaRectangleBorder) alphaRectangleBorder.Clone();
            points = new Vector2[localWorldPoints.Length];

            var width = (alphaRectangleBorder.spriteWidth - alphaRectangleBorder.rightBorder -
                         alphaRectangleBorder.leftBorder) / alphaRectangleBorder.pixelPerUnit;
            var height = (alphaRectangleBorder.spriteHeight - alphaRectangleBorder.bottomBorder -
                          alphaRectangleBorder.topBorder) / alphaRectangleBorder.pixelPerUnit;
            boundsCenter = center;
            boundsSize = new Vector2(width, height);
            ownBounds = new Bounds(boundsCenter, boundsSize);

            Initialize();
            isInitialized = true;
        }

        public ObjectOrientedBoundingBox(ObjectOrientedBoundingBox otherOOBB)
        {
            zRotation = otherOOBB.zRotation;
            rotation = Quaternion.Euler(0, 0, zRotation);
            alphaRectangleBorder = (AlphaRectangleBorder) otherOOBB.alphaRectangleBorder.Clone();
            originAlphaRectangleBorder = (AlphaRectangleBorder) otherOOBB.originAlphaRectangleBorder.Clone();

            boundsCenterOffset = new Vector2(otherOOBB.boundsCenterOffset.x, otherOOBB.boundsCenterOffset.y);
            boundsCenter = new Vector2(otherOOBB.boundsCenter.x, otherOOBB.boundsCenter.y);
            boundsSize = new Vector2(otherOOBB.boundsSize.x, otherOOBB.boundsSize.y);
            ownBounds = new Bounds(boundsCenter + boundsCenterOffset, boundsSize);

            localWorldPoints = new Vector2[otherOOBB.localWorldPoints.Length];

            for (int i = 0; i < localWorldPoints.Length; i++)
            {
                var localPoint = otherOOBB.localWorldPoints[i];
                localWorldPoints[i] = new Vector2(localPoint.x, localPoint.y);
            }

            originLocalWorldPoints = new Vector2[otherOOBB.originLocalWorldPoints.Length];
            for (int i = 0; i < localWorldPoints.Length; i++)
            {
                var originPoint = otherOOBB.originLocalWorldPoints[i];
                originLocalWorldPoints[i] = new Vector2(originPoint.x, originPoint.y);
            }

            points = new Vector2[otherOOBB.points.Length];
            for (int i = 0; i < localWorldPoints.Length; i++)
            {
                var worldPoint = otherOOBB.points[i];

                points[i] = new Vector2(worldPoint.x, worldPoint.y);
            }

            isInitialized = true;
        }

        public Projection ProjectAxis(Vector2 axis)
        {
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

        public void UpdateBox(Transform transform)
        {
            boundsCenter = transform.position;
            ownBounds.center = boundsCenter + boundsCenterOffset;
            zRotation = transform.rotation.eulerAngles.z;
            rotation = Quaternion.Euler(0, 0, zRotation);
            UpdateLocalWorldPoints(false);

            // Apply scaling
            lastGlobalScale = transform.lossyScale;
            for (var i = 0; i < localWorldPoints.Length; i++)
            {
                points[i] = new Vector2(localWorldPoints[i].x * lastGlobalScale.x,
                    localWorldPoints[i].y * lastGlobalScale.y);
            }

            // Transform points from local to world space
            for (var i = 0; i < points.Length; i++)
            {
                points[i] += (Vector2) transform.position;
            }

            axes = new Vector2[2];
            axes[0] = Vector2.Perpendicular(points[3] - points[0]);
            axes[1] = Vector2.Perpendicular(points[0] - points[1]);
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
            ownBounds.center = (Vector2) ownBounds.center - boundsCenter;
            boundsCenterOffset = Vector2.zero;
        }

        public bool Contains(ObjectOrientedBoundingBox otherOOBB)
        {
            foreach (var otherPoint1 in otherOOBB.points)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    var ownPoint1 = points[j];
                    var ownPoint2 = points[(j + 1) % points.Length];
                    var isIntersecting = AreLinesIntersecting(ownPoint1, ownPoint2, otherPoint1, ownBounds.center);

                    if (isIntersecting)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        private bool AreLinesIntersecting(Vector2 line1Point1, Vector2 line1Point2, Vector2 line2Point1,
            Vector2 line2Point2)
        {
            var isIntersecting = false;

            var denominator = (line2Point2.y - line2Point1.y) * (line1Point2.x - line1Point1.x) -
                              (line2Point2.x - line2Point1.x) * (line1Point2.y - line1Point1.y);

            //check for parallelism
            if (denominator == 0f)
            {
                return false;
            }

            var u = ((line2Point2.x - line2Point1.x) * (line1Point1.y - line2Point1.y) -
                     (line2Point2.y - line2Point1.y) * (line1Point1.x - line2Point1.x)) / denominator;
            var v = ((line1Point2.x - line1Point1.x) * (line1Point1.y - line2Point1.y) -
                     (line1Point2.y - line1Point1.y) * (line1Point1.x - line2Point1.x)) / denominator;

            // check if line intersection lies on line segment (including start and end)
            if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }

        private void Initialize()
        {
            originLocalWorldPoints[0] = new Vector3(ownBounds.min.x, ownBounds.max.y, 0); // top left 
            originLocalWorldPoints[1] = new Vector3(ownBounds.min.x, ownBounds.min.y, 0); // bottom left 
            originLocalWorldPoints[2] = new Vector3(ownBounds.max.x, ownBounds.min.y, 0); // bottom right
            originLocalWorldPoints[3] = new Vector3(ownBounds.max.x, ownBounds.max.y, 0); // top right

            UpdateLocalWorldPoints();
        }

        private void UpdateLocalWorldPoints(bool isUpdatingPoints = true)
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

            localWorldPoints[0] = new Vector2(originLocalWorldPoints[0].x + convertedLeftBorder,
                originLocalWorldPoints[0].y - convertedTopBorder); // top left 

            localWorldPoints[1] = new Vector2(originLocalWorldPoints[1].x + convertedLeftBorder,
                originLocalWorldPoints[1].y + convertedBottomBorder); // bottom left 

            localWorldPoints[2] = new Vector2(originLocalWorldPoints[2].x - convertedRightBorder,
                originLocalWorldPoints[2].y + convertedBottomBorder); // bottom right

            localWorldPoints[3] = new Vector2(originLocalWorldPoints[3].x - convertedRightBorder,
                originLocalWorldPoints[3].y - convertedTopBorder); // top right

            for (var i = 0; i < localWorldPoints.Length; i++)
            {
                localWorldPoints[i] = rotation * localWorldPoints[i];
            }

            if (!isUpdatingPoints)
            {
                return;
            }

            for (var i = 0; i < localWorldPoints.Length; i++)
            {
                var localPoint = localWorldPoints[i];
                points[i] = new Vector2(localPoint.x, localPoint.y);
            }

            axes = new Vector2[2];
            axes[0] = Vector2.Perpendicular(points[3] - points[0]);
            axes[1] = Vector2.Perpendicular(points[0] - points[1]);
        }

        public float GetSurfaceArea()
        {
            return boundsSize.x * lastGlobalScale.x * boundsSize.y * lastGlobalScale.y;
        }

        public void OnBeforeSerialize()
        {
            boundsCenter = (Vector2) ownBounds.center - boundsCenterOffset;
            boundsSize = ownBounds.size;
        }

        public void OnAfterDeserialize()
        {
            ownBounds = new Bounds(boundsCenter + boundsCenterOffset, boundsSize);
            points = new Vector2[localWorldPoints.Length];
        }

        public object Clone()
        {
            return new ObjectOrientedBoundingBox(this);
        }
    }
}