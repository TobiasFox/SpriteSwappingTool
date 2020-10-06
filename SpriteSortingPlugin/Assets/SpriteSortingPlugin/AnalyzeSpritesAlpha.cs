using System;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class AnalyzeSpritesAlpha : MonoBehaviour
    {
        private SpriteRenderer ownRenderer;
        private int[] borders;
        float[] adjustedBorder = new float[4];
        Vector3[] sourcePoints = new Vector3[4];
        Vector3[] points = new Vector3[4];
        private Bounds newBounds;

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

            for (var i = 0; i < borders.Length; i++)
            {
                adjustedBorder[i] = borders[i] / ownRenderer.sprite.pixelsPerUnit;
            }

            var width = adjustedBorder[3] - adjustedBorder[1];
            var height = adjustedBorder[2] - adjustedBorder[0];
            newBounds = new Bounds(transform.position, new Vector3(width, height, 0));

            // sourcePoints[0] = new Vector3(adjustedBorder[1], adjustedBorder[0], 0) /*- transform.position*/; // top Left
            // sourcePoints[1] = new Vector3(adjustedBorder[1], adjustedBorder[2], 0) /* - transform.position*/
            //     ; //bottom left
            // sourcePoints[2] = new Vector3(adjustedBorder[3], adjustedBorder[2], 0) /*- transform.position*/
            //     ; //bottom right
            // sourcePoints[3] = new Vector3(adjustedBorder[3], adjustedBorder[0], 0) /*- transform.position*/; //top right

            // // Apply scaling
            // for (int s = 0; s < sourcePoints.Length; s++)
            // {
            //     sourcePoints[s] = new Vector3(sourcePoints[s].x / transform.localScale.x,
            //         sourcePoints[s].y / transform.localScale.y, 0);
            // }
            //
            // // Transform points from local to world space
            // for (int t = 0; t < points.Length; t++)
            // {
            //     points[t] = transform.TransformPoint(sourcePoints[t]);
            // }

            Debug.Log("analyzed within " + (EditorApplication.timeSinceStartup - startTime));
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

            // Gizmos.DrawLine(points[0], points[1]);
            // Gizmos.color=Color.red;
            // Gizmos.DrawLine(points[1], points[2]);
            // Gizmos.color=Color.blue;
            // Gizmos.DrawLine(points[2], points[3]);
            // Gizmos.color=Color.green;
            // Gizmos.DrawLine(points[3], points[0]);

            var extents = newBounds.extents;
            var position = transform.position;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(position + new Vector3(-extents.x, extents.y, 0), 1);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(position + new Vector3(-extents.x, -extents.y, 0), 1);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(position + new Vector3(extents.x, extents.y, 0), 1);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position + new Vector3(extents.x, -extents.y, 0), 1);


            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(newBounds.center + position, newBounds.size);
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