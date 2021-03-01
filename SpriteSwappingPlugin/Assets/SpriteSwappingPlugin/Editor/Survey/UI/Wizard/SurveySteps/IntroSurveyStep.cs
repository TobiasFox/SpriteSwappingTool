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
using SpriteSwappingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSwappingPlugin.Survey.UI.Wizard
{
    public class IntroSurveyStep : SurveyStep
    {
        private const float VerticalSpacing = 5;

        private static readonly string[] PreviewPrefabPathAndName = new string[]
        {
            "Assets",
            "SpriteSwappingPlugin",
            "Editor",
            "Survey",
            "Prefabs",
            "SurveyPreviewParent.prefab"
        };

        private SurveyPreview preview;

        public IntroSurveyStep(string name) : base(name)
        {
            preview = new SurveyPreview(Path.Combine(PreviewPrefabPathAndName), false);
        }

        public override void Commit()
        {
            base.Commit();
            preview.CleanUp();
        }

        public override void DrawContent()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label("Thank you very much for taking the time to participate in this survey.",
                    Styling.CenteredStyleBold);
                EditorGUILayout.Space(VerticalSpacing);

                GUILayout.Label(
                    "This survey is about visual glitches in 2D games. As part of my master thesis I developed a Unity tool, which identifies such glitches and helps to solve them.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.Space();

                var wrappedCenterStyle = new GUIStyle(Styling.CenteredStyle) {wordWrap = true};
                GUILayout.Label("I really appreciate your input!", wrappedCenterStyle);
                preview?.DoPreview();
            }


            EditorGUILayout.Space(VerticalSpacing);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                EditorGUILayout.LabelField("Duration", "15 - 20 min");
                var smallerLabelWrapStyle = new GUIStyle(Styling.LabelWrapStyle);
                smallerLabelWrapStyle.fontSize--;
                EditorGUILayout.LabelField(new GUIContent("Data"),
                    new GUIContent("Completely anonymous and only for the purpose of the master thesis",
                        "The data will only be used for the purpose of my master thesis and will be completely deleted after finishing the thesis. At the latest on 23.02.2021."),
                    Styling.LabelWrapStyle);

                EditorGUILayout.LabelField("Developed by",
                    GeneralData.DevelopedBy + " (Games-Master student at HAW Hamburg)");

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Contact", GUILayout.ExpandWidth(false), GUILayout.Width(147));
                    EditorGUILayout.SelectableLabel(GeneralData.DeveloperMailAddress,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
                }

                EditorGUILayout.LabelField("Optional contest",
                    "1x Steam voucher worth 20€ will be given away among all participants. (submit your mail at the end).",
                    Styling.LabelWrapStyle);
            }

            EditorGUILayout.Space(VerticalSpacing);
            EditorGUILayout.Space(VerticalSpacing);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                EditorGUILayout.LabelField(
                    "This editor window guides you through the survey consisting of five short parts.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.Space(7.5f);

                EditorGUILayout.LabelField(new GUIContent(
                    "Please leave this window the whole time opened and make sure this PC has an active internet connection to send the data.",
                    Styling.InfoIcon), Styling.LabelWrapStyle);
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.LabelField(new GUIContent(
                    "If anything unexpected happens whilst using the tool, close and reopen the corresponding window.",
                    Styling.InfoIcon), Styling.LabelWrapStyle);
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.LabelField(new GUIContent(
                    "Also, please do not recompile any code while the survey window is open. If you do so, it will result in errors due to Unity's serialization behavior.",
                    Styling.InfoIcon), Styling.LabelWrapStyle);
            }
        }

        public override void CleanUp()
        {
            preview?.CleanUp();
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = 1;
            return totalProgress;
        }
    }
}