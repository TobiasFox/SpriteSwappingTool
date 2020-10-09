using SpriteSortingPlugin.SAT;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SATTester : MonoBehaviour
    {
        public Bounds bounds1;
        public float rotation1;

        public Bounds bounds2;
        public float rotation2;

        public void Test()
        {
            var oobb = new ObjectOrientedBoundingBox(bounds1, Quaternion.Euler(0, 0, rotation1));
            var otherOOBB = new ObjectOrientedBoundingBox(bounds2, Quaternion.Euler(0, 0, rotation2));
            // var oobb = new ObjectOrientedBoundingBox(new Bounds(Vector3.zero, new Vector2(10, 10)),
            //     Quaternion.identity);
            // var otherOOBB = new ObjectOrientedBoundingBox(new Bounds(new Vector2(0, 5), new Vector2(10, 10)),
            //     Quaternion.identity);

            var isOverlapping = SATCollisionDetection.IsOverlapping(oobb, otherOOBB);

            Debug.Log(isOverlapping);
        }
    }

    [CustomEditor(typeof(SATTester))]
    public class SATTesterDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Test"))
            {
                var satTester = (SATTester) target;
                satTester.Test();
            }
        }
    }
}