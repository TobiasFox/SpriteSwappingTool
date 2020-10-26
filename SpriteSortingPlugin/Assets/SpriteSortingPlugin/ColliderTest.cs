using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class ColliderTest : MonoBehaviour
    {
        public PolygonCollider2D col1;
        public PolygonCollider2D col2;

        public void Test()
        {
            var distance = col1.Distance(col2);
            var isTouching = col1.IsTouching(col2);

            var result = new ContactPoint2D[3];
            var contactFilter2D = new ContactFilter2D();
            contactFilter2D.NoFilter();
            var resultNumber = Physics2D.GetContacts(col1, col2, contactFilter2D, result);

            var result2 = new Collider2D[3];
            var colliderOverlappingNumber = col1.OverlapCollider(contactFilter2D, result2);

            var result3 = new Collider2D[3];
            var contactNumber = col1.GetContacts(result3);

            Debug.Log("distance " + distance.distance + " overlapping " + distance.isOverlapped + " isValid " +
                      distance.isValid + " test1 " +
                      resultNumber + " test2 " + colliderOverlappingNumber + " test3 " + contactNumber);
            int i = 0;
        }

        public void Test2()
        {
        }

        private float lastDistance;

        private void OnDrawGizmos()
        {
            if (col1 == null || col2 == null)
            {
                return;
            }

            var distance = col1.Distance(col2);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(distance.pointA, 0.05f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(distance.pointB, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(distance.pointA, distance.pointB);

            if (!Mathf.Approximately(distance.distance, lastDistance))
            {
                Debug.Log(distance.distance + " isOverlapping: " + distance.isOverlapped);
            }

            lastDistance = distance.distance;
        }
    }

    [CustomEditor(typeof(ColliderTest))]
    public class ColliderTestDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var satTester = (ColliderTest) target;
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