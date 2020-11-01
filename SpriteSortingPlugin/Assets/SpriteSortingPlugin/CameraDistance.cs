using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CameraDistance : MonoBehaviour
    {
        public Camera camera;
        public SpriteRenderer ownRenderer;
        public bool isDisplayingDistanceToCamera = true;
        public Vector3 offset;

        private void OnDrawGizmos()
        {
            if (!isDisplayingDistanceToCamera || camera == null)
            {
                return;
            }

            if (ownRenderer == null)
            {
                ownRenderer = GetComponent<SpriteRenderer>();
            }

            Handles.BeginGUI();
            if (camera.orthographic)
            {
                var positionOnCameraPlane =
                    new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
                var distance = transform.position - positionOnCameraPlane;
                var dist = distance.magnitude;
                distance = distance * -0.5f + transform.position + offset;
                Handles.Label(distance, dist.ToString("#.00"));
                Handles.DrawLine(transform.position, positionOnCameraPlane);
            }
            else
            {
                var distance = transform.position - camera.transform.position;
                var dist = distance.magnitude;
                distance = distance * -0.5f + transform.position + offset;
                Handles.Label(distance, dist.ToString("#.00"));
                Handles.DrawLine(transform.position, camera.transform.position);
            }

            Handles.EndGUI();
        }
    }
}