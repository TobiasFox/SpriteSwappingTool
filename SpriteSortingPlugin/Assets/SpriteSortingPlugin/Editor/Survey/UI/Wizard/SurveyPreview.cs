#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

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