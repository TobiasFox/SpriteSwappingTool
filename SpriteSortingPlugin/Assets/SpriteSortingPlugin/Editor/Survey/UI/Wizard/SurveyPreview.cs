using SpriteSortingPlugin.SpriteSorting.UI.Preview;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyPreview
    {
        private const float PreviewHeight = 300;
        private static readonly Quaternion DefaultPreviewRotation = Quaternion.Euler(0, 120f, 0);

        private GameObject previewGameObject;
        private Editor previewEditor;
        private bool isPreviewVisible;
        private string previewPrefabPathAndName;

        public SurveyPreview(string assetPath, bool isPreviewVisible = true)
        {
            previewPrefabPathAndName = assetPath;
            this.isPreviewVisible = isPreviewVisible;
        }

        private void GeneratePreview()
        {
            previewGameObject = PreviewUtility.CreateGameObject(null, "SurveyPreviewParent", true);
            var parentTransform = previewGameObject.transform;
            parentTransform.rotation = DefaultPreviewRotation;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(previewPrefabPathAndName);
            var overlappingSpritesGO = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, parentTransform);
            overlappingSpritesGO.hideFlags = HideFlags.DontSave;
            PreviewUtility.HideAndDontSaveGameObject(overlappingSpritesGO);

            PreviewUtility.HideAndDontSaveGameObject(previewGameObject);

            previewEditor = Editor.CreateEditor(previewGameObject);
        }

        public void DoPreview(float previewHeight = -1)
        {
            if (previewGameObject == null)
            {
                GeneratePreview();
            }

            isPreviewVisible = EditorGUILayout.Foldout(isPreviewVisible, "Visual glitch preview", true);

            if (!isPreviewVisible)
            {
                previewGameObject.SetActive(false);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Rotate by click and drag");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset Rotation", GUILayout.Width(95)))
                {
                    previewGameObject.transform.rotation = DefaultPreviewRotation;
                    Object.DestroyImmediate(previewEditor);
                    previewEditor = Editor.CreateEditor(previewGameObject);
                }
            }


            var bgColor = new GUIStyle {normal = {background = Styling.SpriteSortingPreviewBackgroundTexture}};
            var previewRect = GUILayoutUtility.GetRect(1f, previewHeight < 0 ? PreviewHeight : previewHeight);
            previewRect = EditorGUI.IndentedRect(previewRect);

            //hack for not seeing the previewGameObject in the scene view 
            previewGameObject.SetActive(true);
            previewEditor.OnInteractivePreviewGUI(previewRect, bgColor);
            previewGameObject.SetActive(false);
        }

        public void CleanUp()
        {
            if (previewGameObject != null)
            {
                Object.DestroyImmediate(previewGameObject);
                previewGameObject = null;
            }

            if (previewEditor != null)
            {
                Object.DestroyImmediate(previewEditor);
                previewEditor = null;
            }
        }
    }
}