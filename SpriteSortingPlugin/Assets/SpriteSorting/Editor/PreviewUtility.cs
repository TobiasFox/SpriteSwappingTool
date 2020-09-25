using UnityEngine;

namespace SpriteSorting
{
    public static class PreviewUtility
    {
        //TODO: maybe a pool?
        public static GameObject CreateGameObject(Transform parent, string name, bool isDontSave)
        {
            var previewItem = new GameObject(name)
            {
                hideFlags = isDontSave ? HideFlags.DontSave : HideFlags.None
            };

            previewItem.transform.SetParent(parent);

            return previewItem;
        }

        public static void HideAndDontSaveGameObject(GameObject gameObject)
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}