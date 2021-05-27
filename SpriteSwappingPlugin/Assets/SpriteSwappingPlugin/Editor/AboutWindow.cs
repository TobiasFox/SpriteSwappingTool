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

using SpriteSwappingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSwappingPlugin
{
    public class AboutWindow : EditorWindow
    {
        [MenuItem(GeneralData.Name + "/About", false, 4)]
        public static void ShowWindow()
        {
            var window = GetWindow<AboutWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Swapping Tool");
            maxSize = new Vector2(400, 200);
            minSize = maxSize;

            var currentResolution = Screen.currentResolution;
            position = new Rect((currentResolution.width * .5f) - (position.width * .5f),
                (currentResolution.height * .5f) - (position.height * .5f), position.width, position.height);
        }

        private void OnGUI()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                var boldLabelStyle = new GUIStyle(EditorStyles.largeLabel) {fontStyle = FontStyle.Bold};

                GUILayout.Label(GeneralData.Name + " Tool", boldLabelStyle);
                UIUtil.DrawHorizontalLine(true);

                EditorGUILayout.LabelField("Version number", GeneralData.GetFullVersionNumber());
                EditorGUILayout.LabelField("Developed By", GeneralData.DevelopedBy);
                EditorGUILayout.LabelField("Contact", GeneralData.DeveloperMailAddress);
                EditorGUILayout.LabelField("", "All rights reserved");
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Third-party libraries");
            UIUtil.DrawHorizontalLine();


            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField(GeneralData.ClipperLibName + " " + GeneralData.ClipperLibVersion,
                    GeneralData.ClipperLibLicense);
                LinkButton(GeneralData.ClipperLibLink, GeneralData.ClipperLibLink);
            }
        }

        private void LinkButton(string buttonName, string url)
        {
            var buttonContent = new GUIContent(buttonName);
            var linkStyle = Styling.LinkStyle;
            var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
            rect.y += EditorGUIUtility.singleLineHeight;
            var isLinkClicked = GUI.Button(rect, buttonContent, linkStyle);

            rect.width = linkStyle.CalcSize(buttonContent).x;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            var isMouseHoveringLink = rect.Contains(Event.current.mousePosition);
            if (isMouseHoveringLink)
            {
                Handles.BeginGUI();
                Handles.color = linkStyle.normal.textColor;
                Handles.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
                Handles.color = Color.white;
                Handles.EndGUI();
            }

            if (isLinkClicked)
            {
                Application.OpenURL(url);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}