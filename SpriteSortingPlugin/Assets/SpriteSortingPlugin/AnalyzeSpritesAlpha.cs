using System;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class AnalyzeSpritesAlpha : MonoBehaviour
    {
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

            var adjustedBorder = new float[4];
            for (var i = 0; i < borders.Length; i++)
            {
                adjustedBorder[i] = borders[i] / ownRenderer.sprite.pixelsPerUnit;
            }

            var width = adjustedBorder[3] - adjustedBorder[1];
            var height = adjustedBorder[2] - adjustedBorder[0];
            ownBounds = new Bounds(Vector3.zero, new Vector2(width, height));

            UpdateRotatedBoundingBoxPoints();

            Debug.Log("analyzed within " + (EditorApplication.timeSinceStartup - startTime));
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
            Gizmos.DrawSphere(points[0], 1);
            Gizmos.DrawLine(points[0], points[1]);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(points[1], 1);
            Gizmos.DrawLine(points[1], points[2]);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(points[2], 1);
            Gizmos.DrawLine(points[2], points[3]);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(points[3], 1);
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
        }
    }
#endif
}