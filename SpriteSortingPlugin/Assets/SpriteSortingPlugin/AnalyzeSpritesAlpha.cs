using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class AnalyzeSpritesAlpha : MonoBehaviour
    {
        private const float Tolerance = 0.001f;

        [SerializeField] private bool liveUpdateBounds;

        private SpriteRenderer ownRenderer;
        private int[] borders;
        private Bounds ownBounds;

        private readonly Vector3[] sourcePoints = new Vector3[4];
        private readonly Vector3[] points = new Vector3[4];

        public void Generate()
        {
            var startTime = EditorApplication.timeSinceStartup;

            if (!Validate())
            {
                return;
            }

            // GenerateOutmostAlpha();

            // var pixelList = AnalyzeOutlinePixels();
            // var pointList = SortOutlinePoints(pixelList);


            var pointList = AnalyzeAlphaOutline();


            CreatePolygonCollider(pointList);
            Debug.Log("analyzed within " + (EditorApplication.timeSinceStartup - startTime));
        }

        private Color[] pixels;
        private int spriteHeight;
        private int spriteWidth;
        private int[] pixelDirections = new int[8];
        private int[] pixelDirectionPriority = new int[8];
        private int[] visitPixelSteps = new int[8];
        private bool[] visitedPixels;
        private int startPosition = -1;

        private List<Vector2> AnalyzeAlphaOutline()
        {
            var colliderPointList = new List<Vector2>();
            var spritePixelsPerUnit = ownRenderer.sprite.pixelsPerUnit;
            var spriteTexture = ownRenderer.sprite.texture;

            pixels = spriteTexture.GetPixels();
            spriteHeight = spriteTexture.height;
            spriteWidth = spriteTexture.width;
            visitedPixels = new bool[pixels.Length];

            var currentPixelIndex = GetFirstSpritePixelIndex();
            if (currentPixelIndex < 0)
            {
                Debug.Log("sprite is completely transparent");
                return colliderPointList;
            }

            pixelDirections[0] = -spriteWidth;
            pixelDirections[1] = -spriteWidth + 1;
            pixelDirections[2] = +1;
            pixelDirections[3] = spriteWidth + 1;
            pixelDirections[4] = spriteWidth;
            pixelDirections[5] = spriteWidth - 1;
            pixelDirections[6] = -1;
            pixelDirections[7] = -spriteWidth - 1;

            pixelDirectionPriority[0] = 2;
            pixelDirectionPriority[1] = 1;
            pixelDirectionPriority[2] = 2;
            pixelDirectionPriority[3] = 1;
            pixelDirectionPriority[4] = 2;
            pixelDirectionPriority[5] = 1;
            pixelDirectionPriority[6] = 2;
            pixelDirectionPriority[7] = 1;

            visitPixelSteps[0] = -1;
            visitPixelSteps[1] = 2;
            visitPixelSteps[2] = -1;
            visitPixelSteps[3] = 4;
            visitPixelSteps[4] = -1;
            visitPixelSteps[5] = 6;
            visitPixelSteps[6] = -1;
            visitPixelSteps[7] = 0;

            var iterationsToAddCheckOfStartPosition = 3;
            startPosition = -1;
            var setStartPosition = true;
            var newStartPosition = -1;

            var rectCenter = ownRenderer.sprite.rect.center;
            var halfPixelOffset = 1f / spritePixelsPerUnit / 2f;
            var offset = new Vector2(rectCenter.x / spritePixelsPerUnit - halfPixelOffset,
                rectCenter.y / spritePixelsPerUnit - halfPixelOffset);

            while (HasPixelOutlineNeighbour(currentPixelIndex, out var nextPixelIndex))
            {
                var pixelHeight = nextPixelIndex / spriteWidth;
                var pixelWidth = nextPixelIndex % spriteWidth;

                colliderPointList.Add(new Vector2((pixelWidth / spritePixelsPerUnit),
                    (pixelHeight / spritePixelsPerUnit)) - offset);
                currentPixelIndex = nextPixelIndex;

                if (newStartPosition < 0)
                {
                    newStartPosition = nextPixelIndex;
                }

                if (iterationsToAddCheckOfStartPosition > 0)
                {
                    iterationsToAddCheckOfStartPosition--;
                }
                else if (setStartPosition)
                {
                    setStartPosition = false;
                    startPosition = newStartPosition;
                }
            }


            return colliderPointList;
        }

        //TODO: consider edge pixel
        private bool HasPixelOutlineNeighbour(int currentPixelIndex, out int nextPixelIndex)
        {
            var potentialNeighbours = new bool[8];

            for (var i = 0; i < pixelDirections.Length; i++)
            {
                var pixelDirection = pixelDirections[i];
                // if (currentPixelIndex + pixelDirection < 0 || currentPixelIndex + pixelDirection >= pixels.Length)
                // {
                //     Debug.Log("out of bound");
                //     return false;
                // }

                var pixelIndexToCheck = currentPixelIndex + pixelDirection;

                //start pixel reached
                if (startPosition >= 0 && pixelIndexToCheck == startPosition)
                {
                    Debug.Log("start reached");
                    nextPixelIndex = -1;
                    return false;
                }

                //node already visited
                if (visitedPixels[pixelIndexToCheck])
                {
                    continue;
                }

                if (pixels[pixelIndexToCheck].a == 0)
                {
                    potentialNeighbours[i] = true;
                }
            }

            var lastNeighbourPriority = -1;
            var validNeighbour = 0;
            for (var i = potentialNeighbours.Length - 1; i >= 0; i--)
            {
                if (!potentialNeighbours[i])
                {
                    continue;
                }

                var currentNeighbourPriority = GetNeighbourPriorityOfPixel(currentPixelIndex + pixelDirections[i]);

                if (currentNeighbourPriority == 0)
                {
                    continue;
                }

                if (lastNeighbourPriority == -1)
                {
                    lastNeighbourPriority = currentNeighbourPriority;
                    validNeighbour = i;
                    continue;
                }

                if (currentNeighbourPriority >= lastNeighbourPriority)
                {
                    validNeighbour = i;
                }

                break;
            }

            //no neighbour detected
            if (lastNeighbourPriority < 0)
            {
                Debug.Log("no further neighbour");
                nextPixelIndex = -1;
                return false;
            }

            //found one neighbour
            nextPixelIndex = currentPixelIndex + pixelDirections[validNeighbour];
            visitedPixels[nextPixelIndex] = true;

            if (visitPixelSteps[validNeighbour] >= 0)
            {
                visitedPixels[currentPixelIndex + pixelDirections[visitPixelSteps[validNeighbour]]] = true;
            }

            return true;
        }

        private int GetNeighbourPriorityOfPixel(int currentPixelIndex)
        {
            var currentWidth = currentPixelIndex % spriteWidth;
            var currentHeight = currentPixelIndex / spriteWidth;
            var currentPriority = 0;

            for (var i = 0; i < pixelDirections.Length; i++)
            {
                if (currentWidth == 0)
                {
                    if (i >= 5)
                    {
                        break;
                    }
                }
                else if (currentWidth == spriteWidth - 1)
                {
                    if (i >= 1 && i <= 3)
                    {
                        continue;
                    }
                }

                if (currentHeight == 0)
                {
                    if (i == 0 || i == 1 || i == 7)
                    {
                        continue;
                    }
                }
                else if (currentHeight == spriteHeight - 1)
                {
                    if (i >= 3 && i <= 5)
                    {
                        continue;
                    }
                }

                var pixelDirection = pixelDirections[i];
                var pixelToCheckIndex = currentPixelIndex + pixelDirection;

                if (pixels[pixelToCheckIndex].a > 0)
                {
                    currentPriority += pixelDirectionPriority[i];
                }
            }

            return currentPriority;
        }

        // nextPixelIndex = currentPixelIndex + pixelDirections[neighbourOne];
        // //found one neighbour
        // if (neighbourTwo < 0)
        // {
        //     visitedPixels[nextPixelIndex] = true;
        //
        //     if (visitPixelSteps[neighbourOne] > 0)
        //     {
        //         visitedPixels[currentPixelIndex + pixelDirections[visitPixelSteps[neighbourOne]]] = true;
        //     }
        //
        //     return true;
        // }

        //found two neighbours, check priority

        // var lastNeighbourPrioritys = GetNeighbourPriorityOfPixel(nextPixelIndex);
        // var secondToLastNeighbourPriority =
        //     GetNeighbourPriorityOfPixel(currentPixelIndex + pixelDirections[neighbourTwo]);
        // var activeNeighbour = neighbourOne;
        //
        // if (secondToLastNeighbourPriority > lastNeighbourPrioritys)
        // {
        //     nextPixelIndex = currentPixelIndex + pixelDirections[neighbourTwo];
        //     activeNeighbour = neighbourTwo;
        // }
        //
        // visitedPixels[nextPixelIndex] = true;
        //
        // if (visitPixelSteps[activeNeighbour] > 0)
        // {
        //     visitedPixels[currentPixelIndex + pixelDirections[visitPixelSteps[activeNeighbour]]] = true;
        // }

        // return true;


        private int GetFirstSpritePixelIndex()
        {
            var counter = 0;

            for (var y = spriteHeight - 1; y >= 0; y--)
            {
                for (var x = 0; x < spriteWidth; x++)
                {
                    var alphaValue = pixels[counter].a;

                    if (alphaValue > 0)
                    {
                        return counter;
                    }

                    counter++;
                }
            }

            return -1;
        }

        private List<Vector2> SortOutlinePoints(List<Vector2> pixelList)
        {
            var colliderPointList = new List<Vector2>();
            var spritePixelsPerUnit = ownRenderer.sprite.pixelsPerUnit;

            var firstPoint = pixelList[0];
            var secondPoint = pixelList[1];

            if (secondPoint.x < firstPoint.x)
            {
                firstPoint = pixelList[1];
                secondPoint = pixelList[0];
            }

            Debug.Log("first point " + firstPoint);
            Debug.Log("second Point " + secondPoint);

            pixelList.RemoveAt(0);
            pixelList.RemoveAt(0);

            colliderPointList.Add(firstPoint / spritePixelsPerUnit);
            colliderPointList.Add(secondPoint / spritePixelsPerUnit);

            var lastEdge = firstPoint - secondPoint;
            var lastPoint = secondPoint;

            while (pixelList.Count != 0)
            {
                var nextPointIndex = 0;
                var maxAngle = float.NegativeInfinity;
                for (var i = 0; i < pixelList.Count; i++)
                {
                    var point = pixelList[i];
                    var nextEdge = point - lastPoint;
                    var angle = Vector2.SignedAngle(lastEdge, nextEdge);
                    if (angle < 0)
                    {
                        angle = 360f - Mathf.Abs(angle);
                    }

                    if (angle > maxAngle)
                    {
                        maxAngle = angle;
                        nextPointIndex = i;
                    }
                }

                var nextPoint = pixelList[nextPointIndex];
                pixelList.RemoveAt(nextPointIndex);
                colliderPointList.Add(nextPoint / spritePixelsPerUnit);
                Debug.Log("found next point " + nextPoint + " with angle " + maxAngle);

                lastEdge = nextPoint - lastPoint;
                lastPoint = nextPoint;
            }

            // var item = new Vector2(point.x / spritePixelsPerUnit, point.y / spritePixelsPerUnit);
            // colliderPointList.Add(item);
            // Debug.LogFormat("found outline pixel on ({0},{1}) resulting in colliderPoint ({2},{3})", point.x,
            //     point.y, item.x, item.y);
            return colliderPointList;
        }

        private void CreatePolygonCollider(List<Vector2> pixelList)
        {
            var hasPolygonCollider = TryGetComponent<PolygonCollider2D>(out var polyCollider);
            if (hasPolygonCollider)
            {
                polyCollider.points = null;
            }
            else
            {
                polyCollider = gameObject.AddComponent<PolygonCollider2D>();
            }

            polyCollider.points = pixelList.ToArray();
        }

        private Vector2[] GetFirstPolygonEdge(List<Vector2> pointList)
        {
            if (pointList == null || pointList.Count < 3)
            {
                return null;
            }

            var firstPoint = pointList[0];
            var secondPoint = pointList[1];

            if (firstPoint.x >= secondPoint.x)
            {
                return new[] {firstPoint, secondPoint};
            }

            return new[] {secondPoint, firstPoint};

            // for (var i = 0; i < pointList.Count; i++)
            // {
            //     var point = pointList[i];
            //     if (Math.Abs(point.y - minY) > Tolerance)
            //     {
            //         return i;
            //     }
            // }
            //
            // return -1;
        }

        private List<Vector2> AnalyzeOutlinePixels()
        {
            var spriteTexture = ownRenderer.sprite.texture;
            var pixelArray = spriteTexture.GetPixels();
            var counter = 0;
            var outlineList = new List<Vector2>();

            for (int y = spriteTexture.height - 1; y >= 0; y--)
            {
                for (int x = 0; x < spriteTexture.width; x++)
                {
                    var alphaValue = pixelArray[counter].a;

                    if (alphaValue > 0)
                    {
                        counter++;
                        continue;
                    }

                    //TODO consider edge pixel
                    // if (y == 0 || y == spriteTexture.height - 1 || x == 0 || x == spriteTexture.width - 1)
                    // {
                    //     //edge pixel
                    //     outlineList.Add(new Point(x, y));
                    //     continue;
                    // }

                    //left
                    if (x > 0)
                    {
                        if (pixelArray[counter - 1].a > 0)
                        {
                            outlineList.Add(new Vector2(x, y));
                            counter++;
                            continue;
                        }
                    }

                    //right
                    if (x < spriteTexture.width - 1)
                    {
                        if (pixelArray[counter + 1].a > 0)
                        {
                            outlineList.Add(new Vector2(x, y));
                            counter++;
                            continue;
                        }
                    }

                    //top
                    if (y > 0)
                    {
                        if (pixelArray[counter + spriteTexture.width].a > 0)
                        {
                            outlineList.Add(new Vector2(x, y));
                            counter++;
                            continue;
                        }
                    }

                    //bottom
                    if (y < spriteTexture.height - 1)
                    {
                        if (pixelArray[counter - spriteTexture.width].a > 0)
                        {
                            outlineList.Add(new Vector2(x, y));
                            counter++;
                            continue;
                        }
                    }

                    counter++;
                }
            }

            return outlineList;
        }

        private void GenerateOutmostAlpha()
        {
            var spriteTexture = ownRenderer.sprite.texture;
            var pixelArray = spriteTexture.GetPixels();
            borders = new int[] {spriteTexture.height, spriteTexture.width, 0, 0};
            var counter = 0;

            for (int y = 0; y < spriteTexture.height; y++)
            {
                for (int x = 0; x < spriteTexture.width; x++)
                {
                    var color = pixelArray[counter];

                    AnalyzeOutmostAlpha(x, y, color.a);
                    counter++;
                }
            }

            //more performant around 0,124s
            // for (int i = 0; i < pixelArray.Length; i++)
            // {
            //     var color = pixelArray[i];
            //
            //     if (color.a == 0)
            //     {
            //         continue;
            //     }
            //
            //     var tempHeight = i % spriteTexture.width;
            //     var row = i / spriteTexture.width;
            //
            //     AnalyzeOutmostAlpha(tempHeight, row, color.a);
            //     counter++;
            // }

            //offset of about 1 pixel in all directions
            borders[0] = Math.Max(borders[0] - 1, 0);
            borders[1] = Math.Max(borders[1] - 1, 0);
            borders[2] = Math.Min(borders[2] + 1, spriteTexture.height);
            borders[3] = Math.Min(borders[3] + 1, spriteTexture.width);

            var adjustedBorder = new float[4];
            for (var i = 0; i < borders.Length; i++)
            {
                adjustedBorder[i] = borders[i] / ownRenderer.sprite.pixelsPerUnit;
            }

            var width = adjustedBorder[3] - adjustedBorder[1];
            var height = adjustedBorder[2] - adjustedBorder[0];
            ownBounds = new Bounds(Vector3.zero, new Vector2(width, height));

            UpdateRotatedBoundingBoxPoints();
        }

        private void UpdateRotatedBoundingBoxPoints()
        {
            var position = transform.position;
            ownBounds.center = position;

            sourcePoints[0] = new Vector3(ownBounds.min.x, ownBounds.max.y, 0) - position; // top left 
            sourcePoints[1] = new Vector3(ownBounds.min.x, ownBounds.min.y, 0) - position; // bottom left 
            sourcePoints[2] = new Vector3(ownBounds.max.x, ownBounds.min.y, 0) - position; // bottom right
            sourcePoints[3] = new Vector3(ownBounds.max.x, ownBounds.max.y, 0) - position; // top right

            // Apply scaling
            var localScale = transform.localScale;
            for (int s = 0; s < sourcePoints.Length; s++)
            {
                sourcePoints[s] = new Vector3(sourcePoints[s].x / localScale.x, sourcePoints[s].y / localScale.y, 0);
            }

            // Transform points from local to world space
            for (int t = 0; t < points.Length; t++)
            {
                points[t] = transform.TransformPoint(sourcePoints[t]);
            }
        }

        private void AnalyzeOutmostAlpha(int x, int y, float alpha)
        {
            if (alpha == 0)
            {
                return;
            }

            //top
            if (y < borders[0])
            {
                borders[0] = y;
            }

            //left
            if (x < borders[1])
            {
                borders[1] = x;
            }

            //bottom
            if (y > borders[2])
            {
                borders[2] = y;
            }

            //right
            if (x > borders[3])
            {
                borders[3] = x;
            }
        }

        private void OnDrawGizmos()
        {
            if (borders == null || borders.Length == 0)
            {
                return;
            }

            if (liveUpdateBounds)
            {
                UpdateRotatedBoundingBoxPoints();
            }

            Gizmos.color = Color.white;
            // Gizmos.DrawSphere(points[0], 1);
            Gizmos.DrawLine(points[0], points[1]);
            Gizmos.color = Color.red;
            // Gizmos.DrawSphere(points[1], 1);
            Gizmos.DrawLine(points[1], points[2]);
            Gizmos.color = Color.blue;
            // Gizmos.DrawSphere(points[2], 1);
            Gizmos.DrawLine(points[2], points[3]);
            Gizmos.color = Color.yellow;
            // Gizmos.DrawSphere(points[3], 1);
            Gizmos.DrawLine(points[3], points[0]);


            // Gizmos.color = Color.green;
            // Gizmos.DrawWireCube(newBounds.center, newBounds.size);
        }

        private bool Validate()
        {
            // var PGC2D = GetComponent<PolygonCollider2D>();
            // if (PGC2D == null)
            // {
            //     throw new Exception(
            //         $"PixelCollider2D could not be regenerated because there is no PolygonCollider2D component on \"{gameObject.name}\".");
            // }

            var hasSpriteRenderer = TryGetComponent(out ownRenderer);
            if (!hasSpriteRenderer)
            {
                throw new Exception(
                    $"PixelCollider2D could not be regenerated because there is no SpriteRenderer component on \"{gameObject.name}\".");
            }

            if (ownRenderer.sprite == null)
            {
                return false;
            }

            if (ownRenderer.sprite.texture == null)
            {
                return false;
            }

            if (ownRenderer.sprite.texture.isReadable == false)
            {
                throw new Exception(
                    $"PixelCollider2D could not be regenerated because on \"{gameObject.name}\" because the sprite does not allow read/write operations.");
            }

            return true;
        }

        public void Optimize()
        {
        }
    }

    struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AnalyzeSpritesAlpha))]
    public class AnalyzeSpritesAlphaEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var analyzeSpritesAlpha = (AnalyzeSpritesAlpha) target;
            if (GUILayout.Button("Regenerate Collider"))
            {
                analyzeSpritesAlpha.Generate();
            }

            if (GUILayout.Button("Optimize"))
            {
                analyzeSpritesAlpha.Optimize();
            }
        }
    }
#endif
}