using SpriteSortingPlugin.SAT;
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
        public SpriteAlphaData spriteAlphaData;
        private ObjectOrientedBoundingBox[] oobbs;
        public Vector2 point1;
        public Vector2 point2;


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

                var oobb = spriteAlphaData.spriteDataDictionary[assetGuid1].objectOrientedBoundingBox;
                if (i == 1 && oobb == oobbs[0])
                {
                    oobb = new ObjectOrientedBoundingBox(oobb);
                }

                oobb.UpdateBox(spriteRenderer.transform);

                oobbs[i] = oobb;
            }

            var isOverlapping = SATCollisionDetection.IsOverlapping(oobbs[0], oobbs[1]);

            Debug.Log(isOverlapping);
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
        }
    }
}