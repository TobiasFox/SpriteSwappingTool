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

using System.IO;
using SpriteSortingPlugin.SpriteSorting.UI;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class PluginSortingIntro : SurveyStep
    {
        private const string SceneName = "PluginSortingExample_01.unity";

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private HowToDescription howToDescription;
        private bool isDetectorOpened;

        public PluginSortingIntro(string name) : base(name)
        {
            howToDescription = new HowToDescription(false);
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = 1;

            return IsFinished ? totalProgress : 0;
        }

        public override bool IsFilledOut()
        {
            return isDetectorOpened;
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Plugin approach", Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                $"The {GeneralData.FullDetectorName} automatically identifies visual glitches and helps to solve them.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Please open the {GeneralData.FullDetectorName}",
                Styling.LabelWrapStyle);

            var openDetectorContent = new GUIContent(GeneralData.UnityMenuMainCategory + " -> " + GeneralData.Name +
                                                     " -> " + GeneralData.DetectorName);
            EditorGUILayout.LabelField(openDetectorContent, Styling.LabelWrapStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                var width = Styling.ButtonStyle.CalcSize(openDetectorContent).x;
                if (GUILayout.Button($"Open Scene and {GeneralData.DetectorName}", GUILayout.Width(width)))
                {
                    var path = Path.Combine(Path.Combine(SortingTaskData.SceneFolderPath), SceneName);
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    EditorWindow.FocusWindowIfItsOpen<SceneView>();

                    var setupGameObject = GameObject.Find("setup");
                    if (setupGameObject != null)
                    {
                        Selection.objects = new Object[] {setupGameObject};
                        SceneView.FrameLastActiveSceneView();
                        EditorGUIUtility.PingObject(setupGameObject);
                    }

                    EditorApplication.delayCall += OpenDetector;
                    isDetectorOpened = true;
                }
            }

            EditorGUILayout.Space(20);

            var labelStyle = new GUIStyle(Styling.LabelWrapStyle)
                {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold};
            EditorGUILayout.LabelField(
                $"Please try out the {GeneralData.FullDetectorName} in the given scene. When you are finished, continue with the survey.",
                labelStyle);

            EditorGUILayout.Space(20);
            howToDescription.DrawHowTo();

            EditorGUI.indentLevel--;
        }

        private void OpenDetector()
        {
            var detector = EditorWindow.GetWindow<SpriteRendererSwappingDetectorEditorWindow>();
            detector.Show();
        }
    }
}