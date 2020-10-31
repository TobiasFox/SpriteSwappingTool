using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class ObjectOrientedBoundingBoxComponent : MonoBehaviour
    {
        public SpriteData data;
        public bool isUsingOOBBCopy;
        public bool isShowingLocalPoints;

        public ObjectOrientedBoundingBox oobb;

        private void OnDrawGizmosSelected()
        {
            if (data == null)
            {
                return;
            }

            if (!TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                return;
            }

            var spriteGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteRenderer.sprite));
            if (!data.spriteDataDictionary.TryGetValue(spriteGuid, out var spriteDataItem))
            {
                return;
            }

            oobb = spriteDataItem.objectOrientedBoundingBox;

            if (isUsingOOBBCopy)
            {
                oobb = new ObjectOrientedBoundingBox(oobb);
            }

            oobb.UpdateBox(transform);

            if (isShowingLocalPoints)
            {
                var oobbLocalPoints = oobb.LocalWorldPoints;

                Gizmos.color = Color.white;
                Gizmos.DrawSphere(oobbLocalPoints[0], 0.3f);
                Gizmos.DrawLine(oobbLocalPoints[0], oobbLocalPoints[1]);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(oobbLocalPoints[1], 0.3f);
                Gizmos.DrawLine(oobbLocalPoints[1], oobbLocalPoints[2]);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(oobbLocalPoints[2], 0.3f);
                Gizmos.DrawLine(oobbLocalPoints[2], oobbLocalPoints[3]);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(oobbLocalPoints[3], 0.3f);
                Gizmos.DrawLine(oobbLocalPoints[3], oobbLocalPoints[0]);
            }

            var oobbPoints = oobb.Points;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(oobbPoints[0], 0.3f);
            Gizmos.DrawLine(oobbPoints[0], oobbPoints[1]);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(oobbPoints[1], 0.3f);
            Gizmos.DrawLine(oobbPoints[1], oobbPoints[2]);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(oobbPoints[2], 0.3f);
            Gizmos.DrawLine(oobbPoints[2], oobbPoints[3]);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(oobbPoints[3], 0.3f);
            Gizmos.DrawLine(oobbPoints[3], oobbPoints[0]);
        }
    }

    [CustomEditor(typeof(ObjectOrientedBoundingBoxComponent))]
    public class ObjectOrientedBoundingBoxComponentEditor : Editor
    {
        private ObjectOrientedBoundingBoxComponent targetObject;

        private void OnEnable()
        {
            targetObject = (ObjectOrientedBoundingBoxComponent) target;
        }

        // private void OnSceneGUI()
        // {
        //     var center = targetObject.transform.position;
        //     var bounds = targetObject.oobb.OwnBounds;
        //
        //     // Handles.RectangleHandleCap();
        //
        //
        //     Handles.DrawSolidRectangleWithOutline(
        //         new Rect(targetObject.transform.position - ((Vector3) bounds.extents / 2), bounds.size), Color.blue,
        //         Color.black);
        //     Handles.color = Color.white;
        //
        //     var handlesPos = new[]
        //     {
        //         new Vector3(center.x + bounds.size.x / 2, center.y, center.z),
        //         new Vector3(center.x - bounds.size.x / 2, center.y, center.z),
        //         new Vector3(center.x, center.y + bounds.size.y / 2, center.z),
        //         new Vector3(center.x, center.y - bounds.size.y / 2, center.z)
        //     };
        //
        //     float size = HandleUtility.GetHandleSize(targetObject.transform.position) * 0.2f;
        //     Vector3 snap = Vector3.one * 0.5f;
        //
        //     // EditorGUI.BeginChangeCheck();
        //     Vector3 hPos0 =
        //         Handles.FreeMoveHandle(handlesPos[0], Quaternion.identity, size, snap, Handles.CubeHandleCap);
        //     // if (EditorGUI.EndChangeCheck())
        //     // {
        //     //     Undo.RecordObject(zone, "Changed Scale");
        //     //     float offset = (hPos0.x - handlesPos[0].x) / 2f;
        //     //     zone.transform.position += new Vector3(offset, 0, 0);
        //     //     zone.size = new Vector2(hPos0.x - handlesPos[1].x, handlesPos[2].y - handlesPos[3].y);
        //     // }
        //
        //     // EditorGUI.BeginChangeCheck();
        //     Vector3 hPos1 =
        //         Handles.FreeMoveHandle(handlesPos[1], Quaternion.identity, size, snap, Handles.CubeHandleCap);
        //     // if (EditorGUI.EndChangeCheck())
        //     // {
        //     //     Undo.RecordObject(zone, "Changed Scale");
        //     //     float offset = (hPos1.x - handlesPos[1].x) / 2f;
        //     //     zone.transform.position += new Vector3(offset, 0, 0);
        //     //     zone.size = new Vector2(handlesPos[0].x - hPos1.x, handlesPos[2].y - handlesPos[3].y);
        //     // }
        //
        //     // EditorGUI.BeginChangeCheck();
        //     Vector3 hPos2 =
        //         Handles.FreeMoveHandle(handlesPos[2], Quaternion.identity, size, snap, Handles.CubeHandleCap);
        //     // if (EditorGUI.EndChangeCheck())
        //     // {
        //     //     Undo.RecordObject(zone, "Changed Scale");
        //     //     float offset = (hPos2.y - handlesPos[2].y) / 2f;
        //     //     zone.transform.position += new Vector3(0, offset, 0);
        //     //     zone.size = new Vector2(handlesPos[0].x - handlesPos[1].x, hPos2.y - handlesPos[3].y);
        //     // }
        //
        //     // EditorGUI.BeginChangeCheck();
        //     Vector3 hPos3 =
        //         Handles.FreeMoveHandle(handlesPos[3], Quaternion.identity, size, snap, Handles.CubeHandleCap);
        //     // if (EditorGUI.EndChangeCheck())
        //     // {
        //     //     Undo.RecordObject(zone, "Changed Scale");
        //     //     float offset = (hPos3.y - handlesPos[3].y) / 2f;
        //     //     zone.transform.position += new Vector3(0, offset, 0);
        //     //     zone.size = new Vector2(handlesPos[0].x - handlesPos[1].x, handlesPos[2].y - hPos3.y);
        //     // }
        // }
    }
}