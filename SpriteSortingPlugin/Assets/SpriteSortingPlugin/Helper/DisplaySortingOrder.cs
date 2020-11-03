using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DisplaySortingOrder : MonoBehaviour
    {
        public bool isDisplayingSortingOrder = true;

        private SpriteRenderer ownRenderer;

        private void OnDrawGizmos()
        {
            if (!isDisplayingSortingOrder)
            {
                return;
            }

            if (ownRenderer == null)
            {
                ownRenderer = GetComponent<SpriteRenderer>();
            }

            Handles.BeginGUI();

            var style = new GUIStyle {normal = {background = Texture2D.whiteTexture}, fontStyle = FontStyle.Bold};
            Handles.Label(transform.position, " " + ownRenderer.sortingOrder + " ", style);

            Handles.EndGUI();
        }
    }
}