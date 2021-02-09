﻿#region license

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

using SpriteSortingPlugin.UI;
using UnityEditor;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStepIntro : SurveyStep
    {
        private AutoSortingHowToDescription autoSortingHowToDescription;

        public SortingSuggestionStepIntro(string name) : base(name)
        {
            autoSortingHowToDescription = new AutoSortingHowToDescription(false);
        }

        public override void Start()
        {
            base.Start();
            GeneralData.isLoggingActive = true;
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = 1;
            return IsFinished ? totalProgress : 0;
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Sorting order suggestions", Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                $"The {GeneralData.FullDetectorName} also generates sorting order suggestions after SpriteRenderers are being identified.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "These suggestions are based on selectable and modifiable Sorting Criteria:",
                Styling.LabelWrapStyle);

            autoSortingHowToDescription.DrawHowTo();

            EditorGUILayout.Space(20);

            EditorGUI.indentLevel--;
        }
    }
}