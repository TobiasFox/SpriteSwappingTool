using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SATTester : MonoBehaviour
    {
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
        }
    }
}