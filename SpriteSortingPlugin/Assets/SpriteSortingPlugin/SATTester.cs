using System;
using System.Runtime.InteropServices.WindowsRuntime;
using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SATTester : MonoBehaviour
    {
        private const float Tolerance = 0.0001f;

        // public Bounds bounds1;
        // public float rotation1;
        //
        // public Bounds bounds2;
        // public float rotation2;

        public SpriteRenderer[] spriteRenderers;
        public SpriteData spriteData;
        private ObjectOrientedBoundingBox[] oobbs;
        public Vector2 point1;
        public Vector2 point2;
        public Vector2 point3;
        public Vector2 point4;
        public Color backgroundColor;
        public Color foregroundColor;
        public Color primaryColor;

        public void Test()
        {
            // var oobb = new ObjectOrientedBoundingBox(bounds1, rotation1);
            // var otherOOBB = new ObjectOrientedBoundingBox(bounds2, rotation2);
            // var oobb = new ObjectOrientedBoundingBox(new Bounds(Vector3.zero, new Vector2(10, 10)),
            //     Quaternion.identity);
            // var otherOOBB = new ObjectOrientedBoundingBox(new Bounds(new Vector2(0, 5), new Vector2(10, 10)),
            //     Quaternion.identity);

            oobbs = new ObjectOrientedBoundingBox[spriteRenderers.Length];
            for (var i = 0; i < spriteRenderers.Length; i++)
            {
                var spriteRenderer = spriteRenderers[i];
                var assetGuid1 =
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

                var oobb = spriteData.spriteDataDictionary[assetGuid1].objectOrientedBoundingBox;
                if (i == 1 && oobb == oobbs[0])
                {
                    oobb = new ObjectOrientedBoundingBox(oobb);
                }

                oobb.UpdateBox(spriteRenderer.transform);

                oobbs[i] = oobb;
            }

            // var isOverlapping = SATCollisionDetection.IsOverlapping(oobbs[0], oobbs[1]);
            // Debug.Log(isOverlapping);

            Debug.Log(spriteRenderers[1].name + " in " + spriteRenderers[0] + oobbs[0].Contains(oobbs[1]));
            Debug.Log(spriteRenderers[0].name + " in " + spriteRenderers[1] + oobbs[1].Contains(oobbs[0]));
        }

        public void Test2()
        {
            var inDirection = point2 - point1;
            var perp = Vector2.Perpendicular(inDirection);

            Debug.DrawLine(point1, new Vector3(point1.x, point1.y, 2));
            Debug.DrawLine(point2, new Vector3(point2.x, point2.y, 2));
            Debug.DrawLine(point1, point2, Color.green);

            var start = (point1 + point2) / 2;

            Debug.DrawLine(start, new Vector3(start.x, start.y, 2));
            Debug.DrawLine(start, perp + start, Color.magenta);
        }

        public void Test3()
        {
            Debug.Log(AreLinesIntersecting(point1, point2, point3, point4));
        }

        public void ContainingPointTest()
        {
            var spriteRenderer = spriteRenderers[0];
            var assetGuid1 =
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

            var oobb = spriteData.spriteDataDictionary[assetGuid1].objectOrientedBoundingBox;
            oobb.UpdateBox(spriteRenderer.transform);
            Debug.Log(oobb.Contains(point1));
        }

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

        public void AnalyzeSharpness()
        {
            var sharpnessAnalyzer = new SharpnessAnalyzer();

            var sharpness = sharpnessAnalyzer.Analyze(spriteRenderers[0].sprite);
            Debug.Log(sharpness);
        }

        public void AnalyzeLightness()
        {
            var lightnessAnalyzer = new LightnessAnalyzer();

            var lightness = lightnessAnalyzer.Analyze(spriteRenderers[0]);
            Debug.Log(lightness);
        }

        public void AnalyzeBrightness2()
        {
            var lightnessAnalyzer = new LightnessAnalyzer();

            var lightness = lightnessAnalyzer.Analyze2(spriteRenderers[0]);
            var lightness2 = lightnessAnalyzer.Analyze(spriteRenderers[0]);
            Debug.Log("rgb " + lightness + " cielab" + lightness2);
        }

        public void ColorTester()
        {
            var color = spriteRenderers[0].color;
            var color2 = spriteRenderers[1].color;

            for (int i = 0; i < 3; i++)
            {
                var isInForeground = IsInForeground(color, color2, i);
            }
        }

        private bool IsInForeground(Color primaryColor, Color otherPrimaryColor, int channel)
        {
            var from = backgroundColor[channel];
            var to = foregroundColor[channel];
            var primaryChannel = primaryColor[channel];
            var otherPrimaryChannel = otherPrimaryColor[channel];

            var tPrimary = Mathf.InverseLerp(from, to, primaryChannel);
            var tOtherPrimary = Mathf.InverseLerp(from, to, otherPrimaryChannel);

            var isInForeground = tPrimary >= tOtherPrimary;
            Debug.LogFormat("channel {0} tPrimary {1} tOtherPrimary {2} isInForeground {3}", channel, tPrimary,
                tOtherPrimary, isInForeground);
            return isInForeground;
        }

        public void LineTest()
        {
            var slope = GetSlope(point1.x, point1.y, point2.x, point2.y);
            var constant = GetConstant(point2.x, point2.y, slope);
            // IsInLine(point3.x, point3.y, slope, constant);

            // Debug.Log(IsInLine(point3.x, point3.y, slope, constant));

            // Debug.Log(inLine(point1, point2, point3));
            // Debug.Log(isPointOnLine(point3, point1, point2));
            Debug.Log(VectorCheck());
        }

        private bool VectorCheck()
        {
            var origin = point1;
            var dir = point2 - point1;

            if (dir.x == 0)
            {
                return Math.Abs(point3.x - origin.x) < Tolerance;
            }

            if (dir.y == 0)
            {
                return Math.Abs(point3.y - origin.y) < Tolerance;
            }

            var t1 = (point3.x - origin.x) / dir.x;
            var t2 = (point3.y - origin.y) / dir.y;

            var isInLine = Math.Abs(t1 - t2) < Tolerance;

            return isInLine;
        }

        private bool IsInLine(float x3, float y3, float m, float c)
        {
            var temp = m * x3 + c;
            var isInLine = Math.Abs(temp - y3) < Tolerance;
            Debug.LogFormat("{0}={1}*{2}+{3} = {0}={4} => {5}", y3, m, x3, c, temp, isInLine);

            return isInLine;

            // return temp.CompareTo(y3) == 0;
        }

        bool isPointOnLine(Vector2 r, Vector2 p, Vector2 v)
        {
            if (v.x == 0)
            {
                return Math.Abs(r.x - p.x) < Tolerance;
            }

            if (v.y == 0)
            {
                return Math.Abs(r.y - p.y) < Tolerance;
            }

            var pX = (p.x - r.x) / v.x;
            var pY = (p.y - r.y) / v.y;
            return Math.Abs(pX - pY) < Tolerance;

            // ((px-rx)*vy-(py-ry)*vx)/sqrt(vx*vx+vy*vy) <= tol
        }

        public bool inLine(Vector2 a, Vector2 b, Vector2 c)
        {
            // if AC is vertical
            var dif1 = Math.Abs(a.x - c.x);
            if (dif1 < Tolerance)
            {
                return Math.Abs(b.x - c.x) < Tolerance;
            }

            // if AC is horizontal
            var dif2 = Math.Abs(a.y - c.y);
            if (dif2 < Tolerance)
            {
                return Math.Abs(b.y - c.y) < Tolerance;
            }
            // match the gradients

            var dif3 = (b.x - c.x);
            var dif4 = (b.y - c.y);
            var sum1 = dif1 * dif2;
            var sum2 = dif3 * dif4;
            return Math.Abs(sum1 - sum2) < Tolerance;
        }

        private float GetConstant(float x1, float y1, float m)
        {
            return y1 - m * x1;
        }

        private float GetSlope(float x1, float y1, float x2, float y2)
        {
            var denominator = x2 - x1;
            if (denominator == 0)
            {
                return 0;
            }

            return (y2 - y1) / denominator;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(point1, 0.1f);
            Gizmos.DrawSphere(point2, 0.1f);
            Gizmos.DrawSphere(point3, 0.1f);
            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
        }
    }

    [CustomEditor(typeof(SATTester))]
    public class SATTesterDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var satTester = (SATTester) target;
            if (GUILayout.Button("Test"))
            {
                satTester.Test();
            }

            if (GUILayout.Button("Test2"))
            {
                satTester.Test2();
            }

            if (GUILayout.Button("Test3"))
            {
                satTester.Test3();
            }

            if (GUILayout.Button(nameof(SATTester.ContainingPointTest)))
            {
                satTester.ContainingPointTest();
            }

            if (GUILayout.Button(nameof(SATTester.AnalyzeSharpness)))
            {
                satTester.AnalyzeSharpness();
            }

            if (GUILayout.Button(nameof(SATTester.AnalyzeLightness)))
            {
                satTester.AnalyzeLightness();
            }

            if (GUILayout.Button(nameof(SATTester.AnalyzeBrightness2)))
            {
                satTester.AnalyzeBrightness2();
            }

            if (GUILayout.Button(nameof(SATTester.ColorTester)))
            {
                satTester.ColorTester();
            }

            if (GUILayout.Button(nameof(SATTester.LineTest)))
            {
                satTester.LineTest();
            }
        }
    }
}