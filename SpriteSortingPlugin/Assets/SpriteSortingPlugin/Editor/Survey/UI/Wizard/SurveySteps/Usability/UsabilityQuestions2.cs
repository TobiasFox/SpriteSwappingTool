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

using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class UsabilityQuestions2 : SurveyStep
    {
        private const int QuestionCounterStart = 11;
        private const float IntendWith = 10;

        private static readonly float TextAreaHeight = EditorGUIUtility.singleLineHeight * 3;

        private static readonly string[] RatingQuestions = new string[]
        {
            "How easy to use was the Sprite Swapping Detector?",
            "How easy to use was the Sprite Data Analysis-window?",
            "How helpful is the functionality to generate Sprite order suggestions?",
        };

        private UsabilityData data;
        private int questionCounter;

        public UsabilityQuestions2(string name, UsabilityData data) : base(name)
        {
            this.data = data;
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Usability", Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);

            questionCounter = QuestionCounterStart;

            EditorGUILayout.LabelField(
                $"The following usability questions addresses specific parts of the {GeneralData.Name} tool.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);

            for (var i = 0; i < RatingQuestions.Length; i++)
            {
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    DrawRatingQuestion(i);
                    questionCounter++;
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawOccuringErrorsText();
                questionCounter++;
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawMissingCriteriaText();
                questionCounter++;
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawMiscellaneousText();
                questionCounter++;
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }

        public override bool IsFilledOut()
        {
            return data.occuringError >= 0;
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = 1;

            var currentProgress = 0;

            if (!IsStarted)
            {
                return currentProgress;
            }

            if (IsFinished)
            {
                return totalProgress;
            }

            if (data.occuringError >= 0)
            {
                currentProgress++;
            }

            return currentProgress;
        }

        private void DrawRatingQuestion(int index)
        {
            EditorGUILayout.LabelField($"{questionCounter}. {RatingQuestions[index]}", Styling.QuestionLabelStyle);

            using (new EditorGUI.IndentLevelScope())
            {
                var entireQuestionRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false));
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    data.ratingAnswers[index] =
                        GUI.HorizontalSlider(entireQuestionRect, data.ratingAnswers[index], 0, 100);
                    if (changeScope.changed)
                    {
                        data.ratingAnswersChanged[index] = true;
                    }
                }
            }
        }

        private void DrawMissingCriteriaText()
        {
            EditorGUILayout.LabelField(
                questionCounter +
                ". Which criteria were missing when using the functionality to generate sorting order suggestions? (optional)",
                Styling.QuestionLabelStyle);

            using (new EditorGUI.IndentLevelScope())
            {
                data.missingCriteriaText =
                    EditorGUILayout.TextArea(data.missingCriteriaText, GUILayout.Height(TextAreaHeight));
            }
        }

        private void DrawOccuringErrorsText()
        {
            EditorGUILayout.LabelField(
                $"{questionCounter}. Did any errors occur while using the {GeneralData.Name} tool?",
                Styling.QuestionLabelStyle);

            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Height(TextAreaHeight),
                        GUILayout.Width(150)))
                    {
                        GUILayout.FlexibleSpace();
                        using (new GUILayout.HorizontalScope())
                        {
                            var occuringErrorRect =
                                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false,
                                    EditorGUIUtility.singleLineHeight));

                            var answers = new[] {"Yes", "No"};
                            data.occuringError =
                                GUI.SelectionGrid(occuringErrorRect, data.occuringError, answers, 2);
                        }

                        GUILayout.FlexibleSpace();
                    }

                    using (new EditorGUILayout.VerticalScope(GUILayout.Height(TextAreaHeight),
                        GUILayout.Width(150)))
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("If yes, which errors? (optional)", GUILayout.ExpandWidth(false));
                        GUILayout.FlexibleSpace();
                    }

                    using (new EditorGUILayout.VerticalScope(GUILayout.Height(TextAreaHeight),
                        GUILayout.ExpandWidth(true)))
                    {
                        using (new EditorGUI.DisabledScope(data.occuringError != 0))
                        {
                            data.occuringErrorsText = EditorGUILayout.TextArea(data.occuringErrorsText,
                                GUILayout.Height(TextAreaHeight));
                        }
                    }
                }
            }

            GUILayout.Space(3f);
        }

        private void DrawMiscellaneousText()
        {
            EditorGUILayout.LabelField(questionCounter + ". Is there anything else you want to share? (optional)",
                Styling.QuestionLabelStyle);

            DrawOptionalTextInputWithBool(ref data.isMiscellaneous, ref data.miscellaneous);
        }

        private void DrawOptionalTextInputWithBool(ref bool isEnabled, ref string inputText)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Height(TextAreaHeight),
                        GUILayout.Width(150)))
                    {
                        GUILayout.FlexibleSpace();
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(IntendWith * (EditorGUI.indentLevel + 1));
                            isEnabled = GUILayout.Toggle(isEnabled, "Yes",
                                Styling.ButtonStyle,
                                GUILayout.Width(90));
                        }

                        GUILayout.FlexibleSpace();
                    }

                    using (new EditorGUILayout.VerticalScope(GUILayout.Height(TextAreaHeight),
                        GUILayout.ExpandWidth(true)))
                    {
                        using (new EditorGUI.DisabledScope(!isEnabled))
                        {
                            inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(TextAreaHeight));
                        }
                    }
                }
            }

            GUILayout.Space(3f);
        }
    }
}