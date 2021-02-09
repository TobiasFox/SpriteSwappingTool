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

using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class UsabilityQuestions1 : SurveyStep
    {
        private const int QuestionCounterStart = 1;
        private const float QuestionWidthPercentage = 0.6f;

        private static readonly string[] SusQuestion = new string[]
        {
            "I think that I would like to use this system frequently.",
            "I found the system unnecessarily complex.",
            "I thought the system was easy to use.",
            "I think that I would need the support of a technical person to be able to use this system.",
            "I found the various functions in this system were well integrated.",
            "I thought there was too much inconsistency in this system.",
            "I would imagine that most people would learn to use this system very quickly.",
            "I found the system very cumbersome to use.",
            "I felt very confident using the system.",
            "I needed to learn a lot of things before I could get going with this system."
        };

        private UsabilityData data;
        private int questionCounter;

        public UsabilityQuestions1(string name, UsabilityData data) : base(name)
        {
            this.data = data;
        }

        public override void DrawContent()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("Usability", Styling.LabelWrapStyle);
            }

            EditorGUILayout.Space(5);

            questionCounter = QuestionCounterStart;
            DrawSusQuestions();
        }

        public override bool IsFilledOut()
        {
            foreach (var susAnswer in data.susAnswers)
            {
                if (susAnswer < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = SusQuestion.Length;

            var currentProgress = 0;

            if (!IsStarted)
            {
                return currentProgress;
            }

            if (IsFinished)
            {
                return totalProgress;
            }

            foreach (var susAnswer in data.susAnswers)
            {
                if (susAnswer >= 0)
                {
                    currentProgress++;
                }
            }

            return currentProgress;
        }

        private void DrawSusQuestions()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                var richTextStyle = new GUIStyle(Styling.LabelWrapStyle) {richText = true};
                EditorGUILayout.LabelField(
                    "To evaluate the overall usability, the standardized questionnaire <i>System Usability Score</i> is used.",
                    richTextStyle);
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(
                    $"The system to which all questions refer to is the {GeneralData.Name} tool.",
                    Styling.LabelWrapStyle);
            }

            EditorGUILayout.Space(15);

            UsabilityQuestionsUtility.DrawRatingHeader(QuestionWidthPercentage,
                $"Overall usability of the {GeneralData.Name} Tool (system)",
                "Strongly\nDisagree", "Strongly\nAgree");

            for (var i = 0; i < SusQuestion.Length; i++)
            {
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    data.susAnswers[i] = UsabilityQuestionsUtility.DrawSingleRatingQuestion(data.susAnswers[i],
                        QuestionWidthPercentage, questionCounter, SusQuestion[i]);
                    questionCounter++;
                }

                EditorGUILayout.Space();
            }
        }
    }
}