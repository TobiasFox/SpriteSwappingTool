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

using SpriteSortingPlugin.Survey.UI.Wizard;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class HowToWindow : EditorWindow
    {
        private bool isDetectorDescriptionExpanded = true;
        private bool isAnalyzeDescriptionExpanded;
        private HowToDescription howToDescription;
        private AutoSortingHowToDescription autoSortingHowToDescription;


        [MenuItem(GeneralData.UnityMenuMainCategory + "/" + GeneralData.Name + "/How to", false, 3)]
        public static void ShowWindow()
        {
            var window = GetWindow<HowToWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Swapping How to");
            howToDescription = new HowToDescription(true, true) {};
            autoSortingHowToDescription = new AutoSortingHowToDescription() {isBoldHeader = false};
        }

        private void OnGUI()
        {
            var descriptionLabelStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleCenter, wordWrap = true
            };
            var descriptionLabelContent = new GUIContent(
                "This tool identifies and helps to sort overlapping and unsorted SpriteRenderers, since such renderers often lead to unwanted Sprite swaps (visual glitches).",
                UITooltipConstants.SortingEditorSpriteSwapDescriptionTooltip);

            GUILayout.Label(descriptionLabelContent, descriptionLabelStyle, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space(10);

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    isDetectorDescriptionExpanded =
                        EditorGUILayout.Foldout(isDetectorDescriptionExpanded, GeneralData.FullDetectorName, true);

                    if (isDetectorDescriptionExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(
                                $"The {GeneralData.DetectorName} automatically identifies visual glitches and helps to solve them.",
                                Styling.LabelWrapStyle);

                            EditorGUILayout.Space(10);
                            howToDescription.DrawHowTo();
                            EditorGUILayout.Space(10);

                            EditorGUILayout.LabelField(
                                $"The {GeneralData.DetectorName} also generates sorting order suggestions after SpriteRenderers are being identified.",
                                Styling.LabelWrapStyle);

                            EditorGUILayout.LabelField(
                                "These suggestions are based on selectable and modifiable Sorting Criteria:",
                                Styling.LabelWrapStyle);
                            EditorGUILayout.Space(10);

                            autoSortingHowToDescription.DrawHowTo();
                        }
                    }
                }
            }

            EditorGUILayout.Space(20);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    isAnalyzeDescriptionExpanded =
                        EditorGUILayout.Foldout(isAnalyzeDescriptionExpanded, GeneralData.FullDataAnalysisName, true);

                    if (isAnalyzeDescriptionExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(
                                $"With the {GeneralData.DataAnalysisName} {nameof(SpriteData)} assets can be generated and modified. ",
                                Styling.LabelWrapStyle);
                            EditorGUILayout.LabelField(
                                $"Such an asset is used to have a more accurate Sprite outline or to use Sorting Criteria for the sorting order suggestion functionality.",
                                Styling.LabelWrapStyle);
                        }
                    }
                }
            }
        }
    }
}