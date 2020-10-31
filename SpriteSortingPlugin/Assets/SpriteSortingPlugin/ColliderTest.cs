using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class ColliderTest : MonoBehaviour
    {
        public PolygonCollider2D col1;
        public PolygonCollider2D col2;

        public SpriteRenderer spriteRenderer1;
        public SpriteRenderer spriteRenderer2;

        public SpriteData spriteData;

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
            var polyColliderGameObject1 = new GameObject("ToCheck- PolygonCollider " +
                                                         spriteRenderer1.name);
            var currentTransform = spriteRenderer1.transform;
            polyColliderGameObject1.transform.SetPositionAndRotation(
                currentTransform.position, currentTransform.rotation);
            polyColliderGameObject1.transform.localScale = currentTransform.lossyScale;

            var polygonColliderToCheck = polyColliderGameObject1.AddComponent<PolygonCollider2D>();
            var guid1 = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(spriteRenderer1.sprite.GetInstanceID()));
            polygonColliderToCheck.points = spriteData.spriteDataDictionary[guid1].outlinePoints.ToArray();


            var polyColliderGameObject =
                new GameObject("PolygonCollider " + spriteRenderer2.name);
            currentTransform = spriteRenderer2.transform;
            polyColliderGameObject.transform.SetPositionAndRotation(
                currentTransform.position, currentTransform.rotation);
            polyColliderGameObject.transform.localScale = currentTransform.lossyScale;

            var otherPolygonColliderToCheck =
                polyColliderGameObject.AddComponent<PolygonCollider2D>();
            var guid2 = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(spriteRenderer2.sprite.GetInstanceID()));
            otherPolygonColliderToCheck.points = spriteData.spriteDataDictionary[guid2].outlinePoints.ToArray();


            var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);

            Debug.DrawLine(distance.pointA,
                new Vector3(distance.pointA.x, distance.pointA.y, -15), Color.blue, 3);
            Debug.DrawLine(distance.pointB,
                new Vector3(distance.pointB.x, distance.pointB.y, -15), Color.cyan, 3);
            Debug.DrawLine(distance.pointA, distance.pointB, Color.green);

            Debug.Log("distance " + distance.distance + " overlapping " + distance.isOverlapped + " isValid " +
                      distance.isValid);

            DrawColliderOutline(polygonColliderToCheck, Color.green, Vector2.zero);
            DrawColliderOutline(otherPolygonColliderToCheck, Color.blue, Vector2.zero);
        }

        private static void DrawColliderOutline(PolygonCollider2D polygonColliderToCheck, Color color, Vector2 offset)
        {
            var lastPoint = polygonColliderToCheck.points[0] + offset;
            for (var i = 1; i < polygonColliderToCheck.points.Length; i++)
            {
                var point = polygonColliderToCheck.points[i] + offset;
                Debug.DrawLine(lastPoint, point, color, 3);
                lastPoint = point;
            }
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