using System.Collections.Generic;
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
        public SpriteRenderer spriteRenderer3;
        public SpriteRenderer spriteRenderer4;

        public SpriteData spriteData;

        public Vector2 point1;
        public Vector2 point2;
        public EdgeCollider2D edgeCollider;
        public Transform pointTransform;

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
            polygonColliderToCheck.points = spriteData.spriteDataDictionary[guid1].outlinePoints;


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
            otherPolygonColliderToCheck.points = spriteData.spriteDataDictionary[guid2].outlinePoints;


            var distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);
            Debug.Log("distance1 " + distance.distance + " overlapping " + distance.isOverlapped + " isValid " +
                      distance.isValid);

            // currentTransform = spriteRenderer3.transform;
            // polyColliderGameObject1.transform.SetPositionAndRotation(
            //     currentTransform.position, currentTransform.rotation);
            // polyColliderGameObject1.transform.localScale = currentTransform.lossyScale;
            //
            // currentTransform = spriteRenderer4.transform;
            // otherPolygonColliderToCheck.transform.SetPositionAndRotation(
            //     currentTransform.position, currentTransform.rotation);
            // otherPolygonColliderToCheck.transform.localScale = currentTransform.lossyScale;
            //
            //
            // Physics2D.SyncTransforms();
            //
            // distance = polygonColliderToCheck.Distance(otherPolygonColliderToCheck);
            // Debug.DrawLine(distance.pointA,
            //     new Vector3(distance.pointA.x, distance.pointA.y, -15), Color.blue, 3);
            // Debug.DrawLine(distance.pointB,
            //     new Vector3(distance.pointB.x, distance.pointB.y, -15), Color.cyan, 3);
            // Debug.DrawLine(distance.pointA, distance.pointB, Color.green);

            // Debug.Log("distance2 " + distance.distance + " overlapping " + distance.isOverlapped + " isValid " +
            // distance.isValid);


            // DrawColliderOutline(polygonColliderToCheck, Color.green, Vector2.zero);
            // DrawColliderOutline(otherPolygonColliderToCheck, Color.blue, Vector2.zero);
        }

        public void LineIntersectionTest()
        {
            // var hit = Physics2D.Linecast(point2, point1);

            var isContained = true;
            var counter = -1;
            Vector3 testPosition = Vector2.down;
            foreach (var point in col2.points)
            {
                counter++;
                var transformPoint = col2.transform.TransformPoint(point);
                // var transformPoint = point;
                if (col1.OverlapPoint(transformPoint))
                {
                    continue;
                }

                break;
            }

            Debug.Log(isContained);
        }

        public float CalculatePolygonArea(PolygonCollider2D polyCollider)
        {
            float temp = 0;

            var polyColliderPoints = polyCollider.points;
            for (int i = 0; i < polyColliderPoints.Length; i++)
            {
                var point1 = polyCollider.transform.TransformPoint(polyColliderPoints[i]);
                var point2 =
                    polyCollider.transform.TransformPoint(polyColliderPoints[(i + 1) % polyColliderPoints.Length]);

                float mulA = point1.x * point2.y;
                float mulB = point2.x * point1.y;
                temp += (mulA - mulB);
            }

            temp *= 0.5f;
            return Mathf.Abs(temp);

            // int i = 0 ;
            // for(; i < list.Count ; i++){
            //     if(i != list.Count - 1){
            //         float mulA = list[i].transform.position.x * list[i+1].transform.position.z;
            //         float mulB = list[i+1].transform.position.x * list[i].transform.position.z;
            //         temp = temp + ( mulA - mulB );
            //     }else{
            //         float mulA = list[i].transform.position.x * list[0].transform.position.z;
            //         float mulB = list[0].transform.position.x * list[i].transform.position.z;
            //         temp = temp + ( mulA - mulB );
            //     }
            // }
        }

        private void DrawColliderOutline(PolygonCollider2D polygonColliderToCheck, Color color, Vector2 offset)
        {
            var lastPoint = (Vector2) col2.transform.TransformPoint(polygonColliderToCheck.points[0]) + offset;
            for (var i = 1; i < polygonColliderToCheck.points.Length; i++)
            {
                var point = polygonColliderToCheck.points[i] + offset;
                point = col2.transform.TransformPoint(point);
                Debug.DrawLine(lastPoint, point, color, 3);
                lastPoint = point;
            }
        }

        private float lastDistance;

        private void OnDrawGizmos()
        {
            // Gizmos.DrawLine(point1, point2);
            //
            //
            // if (col1 == null || col2 == null)
            // {
            //     return;
            // }
            //
            // var distance = col1.Distance(col2);
            //
            // Gizmos.color = Color.cyan;
            // Gizmos.DrawSphere(distance.pointA, 0.05f);
            // Gizmos.color = Color.blue;
            // Gizmos.DrawSphere(distance.pointB, 0.05f);
            // Gizmos.color = Color.green;
            // Gizmos.DrawLine(distance.pointA, distance.pointB);
            //
            // if (!Mathf.Approximately(distance.distance, lastDistance))
            // {
            //     Debug.Log(distance.distance + " isOverlapping: " + distance.isOverlapped);
            // }
            //
            // lastDistance = distance.distance;
        }

        public void CalculatePolygonSurface()
        {
            Debug.Log(CalculatePolygonArea(col1));
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

            if (GUILayout.Button("LineIntersectionTest"))
            {
                satTester.LineIntersectionTest();
            }

            if (GUILayout.Button("Surface area Test"))
            {
                satTester.CalculatePolygonSurface();
            }
        }
    }
}