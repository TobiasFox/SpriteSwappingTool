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
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class GeneralQuestions : SurveyStep
    {
        private const int QuestionCounterStart = 1;
        private const float IndentLevelWidth = 10;

        private static readonly string[] TestExampleScenesFolderPath = new string[]
        {
            "Assets", "_Scenes", "TestExamples"
        };

        private static readonly string[] PreviewPrefabPathAndName = new string[]
        {
            "Assets",
            "SpriteSortingPlugin",
            "Editor",
            "Survey",
            "Prefabs",
            "SurveyPreviewParent.prefab"
        };

        private GeneralQuestionsData data;
        private float space = 17.5f;
        private int questionCounter;
        private SurveyPreview preview;
        private HowToDescription howToDescription;
        private AutoSortingHowToDescription autoSortingHowToDescription;

        public GeneralQuestions(string name, GeneralQuestionsData data) : base(name)
        {
            this.data = data;
            preview = new SurveyPreview(Path.Combine(PreviewPrefabPathAndName), false);
            howToDescription = new HowToDescription() {isBoldHeader = false};
            autoSortingHowToDescription = new AutoSortingHowToDescription() {isBoldHeader = false};
        }

        public override void DrawContent()
        {
            questionCounter = QuestionCounterStart;
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("General questions", Styling.LabelWrapStyle);
            EditorGUILayout.Space(20);

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawIsDevelopingGamesQuestion();
                questionCounter++;
            }

            //criterion of exclusion
            if (data.developing2dGames == 1)
            {
                DrawExclusion();

                EditorGUI.indentLevel--;
                return;
            }

            using (new EditorGUI.DisabledScope(data.developing2dGames != 0))
            {
                EditorGUILayout.Space(space);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    DrawNumberOfApplicationsQuestions();
                    questionCounter++;
                }

                EditorGUILayout.Space(space);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    DrawIsExperiencedVisualGlitchQuestion();
                    questionCounter++;
                }

                EditorGUILayout.Space(space);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    DrawGameDevelopmentRelation();
                    questionCounter++;
                }

                EditorGUILayout.Space(space);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    DrawMainFieldOfWork();
                    questionCounter++;
                }
            }

            EditorGUI.indentLevel--;
        }

        public override bool IsFilledOut()
        {
            if (data.developing2dGames != 0)
            {
                return false;
            }

            if (data.numberOfDeveloped2dGames < 0 && !data.isNumberOfDeveloped2dGamesNoAnswer)
            {
                return false;
            }

            if (!data.isStudent && !data.isProfessional && !data.isHobbyist &&
                !data.isNotDevelopingGames && !data.isGameDevelopmentRelationNoAnswer &&
                !data.isGameDevelopmentRelationOther)
            {
                return false;
            }

            if (data.workingOnApplicationWithVisualGlitch < 0)
            {
                return false;
            }

            if (!data.IsAnyMainFieldOfWork() && !data.isMainFieldOfWorkOther && !data.isMainFieldOfWorkNoAnswer)
            {
                return false;
            }

            return true;
        }

        public override int GetProgress(out int totalProgress)
        {
            if (data.developing2dGames == 1)
            {
                totalProgress = 1;
                return 1;
            }

            totalProgress = 5;

            if (!IsStarted)
            {
                return 0;
            }

            if (IsFinished)
            {
                return totalProgress;
            }

            var currentProgress = 0;
            if (data.developing2dGames == 0)
            {
                currentProgress++;
            }

            if (data.isStudent || data.isProfessional || data.isHobbyist ||
                data.isNotDevelopingGames || data.isGameDevelopmentRelationNoAnswer ||
                data.isGameDevelopmentRelationOther)
            {
                currentProgress++;
            }

            if (data.numberOfDeveloped2dGames >= 0 || data.isNumberOfDeveloped2dGamesNoAnswer)
            {
                currentProgress++;
            }

            if (data.workingOnApplicationWithVisualGlitch >= 0)
            {
                currentProgress++;
            }

            if (data.IsAnyMainFieldOfWork())
            {
                currentProgress++;
            }

            return currentProgress;
        }

        public override void Commit()
        {
            base.Commit();
            preview?.CleanUp();
        }

        public override void CleanUp()
        {
            base.CleanUp();
            preview?.CleanUp();
        }

        private void DrawExclusion()
        {
            EditorGUILayout.Space(20);
            var centeredStyle = new GUIStyle(Styling.CenteredStyle) {wordWrap = true};

            var excludeMessage =
                "Thank you very much for your interest! However, this survey assumes";
            if (data.developing2dGames == 1)
            {
                excludeMessage +=
                    " experiences in developing 2D Unity applications as well as knowing how to adjust SpriteRenderers' sorting options";
            }

            excludeMessage += ".\nYou can close this window now.";

            EditorGUILayout.LabelField(excludeMessage, centeredStyle);

            EditorGUILayout.Space(20);

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField(
                $"If you want, you can test the {GeneralData.Name} tool within some example scenes.",
                Styling.LabelWrapStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    var openDetectorContent = new GUIContent(
                        $"{GeneralData.UnityMenuMainCategory} -> {GeneralData.Name} -> {GeneralData.DetectorName}");
                    EditorGUILayout.LabelField($"{GeneralData.FullDetectorName}",
                        Styling.LabelWrapStyle);
                    EditorGUILayout.LabelField(openDetectorContent, Styling.LabelWrapStyle);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20);
                        var width = Styling.ButtonStyle.CalcSize(openDetectorContent).x;
                        if (GUILayout.Button("Open " + GeneralData.DetectorName, GUILayout.Width(width)))
                        {
                            var detector = EditorWindow.GetWindow<SpriteRendererSwappingDetectorEditorWindow>();
                            detector.Show();
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Example scenes at",
                        Styling.LabelWrapStyle);
                    var folderPath = Path.Combine(TestExampleScenesFolderPath);
                    EditorGUILayout.LabelField(folderPath, Styling.LabelWrapStyle);
                    if (GUILayout.Button("Show Folder", GUILayout.Width(150)))
                    {
                        var path = folderPath;

                        if (folderPath[folderPath.Length - 1].Equals(Path.PathSeparator))
                        {
                            path = folderPath.Substring(0, folderPath.Length - 1);
                        }

                        var testExampleSceneFolder = AssetDatabase.LoadAssetAtPath<Object>(path);
                        Selection.activeObject = testExampleSceneFolder;
                        EditorGUIUtility.PingObject(testExampleSceneFolder);
                    }
                }
            }

            howToDescription.DrawHowTo();
            autoSortingHowToDescription.DrawHowTo();
        }

        private void DrawGameDevelopmentRelation()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". How are you related to the development of games? (multi-choice)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(3 * IndentLevelWidth);
                    data.isStudent =
                        GUILayout.Toggle(data.isStudent, "Student", Styling.ButtonStyle);

                    data.isProfessional = GUILayout.Toggle(data.isProfessional, "Professional",
                        Styling.ButtonStyle);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(3 * IndentLevelWidth);
                    data.isHobbyist =
                        GUILayout.Toggle(data.isHobbyist, "Hobbyist", Styling.ButtonStyle);

                    data.isNotDevelopingGames = GUILayout.Toggle(data.isNotDevelopingGames, "Not developing games",
                        Styling.ButtonStyle);
                }

                using (new GUILayout.HorizontalScope())
                {
                    data.isGameDevelopmentRelationOther = EditorGUILayout.ToggleLeft("Other",
                        data.isGameDevelopmentRelationOther, GUILayout.ExpandWidth(false));
                    using (new EditorGUI.DisabledScope(!data.isGameDevelopmentRelationOther))
                    {
                        data.gameDevelopmentRelationOther = EditorGUILayout.TextArea(data.gameDevelopmentRelationOther,
                            GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    }
                }

                if (changeScope.changed)
                {
                    if (data.isStudent || data.isProfessional ||
                        data.isHobbyist || data.isNotDevelopingGames ||
                        data.isGameDevelopmentRelationOther)
                    {
                        data.isGameDevelopmentRelationNoAnswer = false;
                    }
                }
            }

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                data.isGameDevelopmentRelationNoAnswer =
                    EditorGUILayout.ToggleLeft("No answer", data.isGameDevelopmentRelationNoAnswer);
                if (changeScope.changed && data.isGameDevelopmentRelationNoAnswer)
                {
                    data.isStudent = false;
                    data.isProfessional = false;
                    data.isHobbyist = false;
                    data.isNotDevelopingGames = false;
                    data.isGameDevelopmentRelationOther = false;
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawMainFieldOfWork()
        {
            EditorGUILayout.LabelField(
                questionCounter +
                ". If you develop games, what best describes your main fields of work? (multi-choice)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                var rows = (int) Mathf.Ceil(GeneralQuestionsData.MainFieldsOfWork.Length / 2f);

                for (var i = 0; i < rows; i++)
                {
                    var currentIndex = 2 * i;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(3 * IndentLevelWidth);

                        data.mainFieldOfWork[currentIndex] = GUILayout.Toggle(
                            data.mainFieldOfWork[currentIndex], GeneralQuestionsData.MainFieldsOfWork[currentIndex],
                            Styling.ButtonStyle);

                        if (currentIndex + 1 < GeneralQuestionsData.MainFieldsOfWork.Length)
                        {
                            data.mainFieldOfWork[currentIndex + 1] = GUILayout.Toggle(
                                data.mainFieldOfWork[currentIndex + 1],
                                GeneralQuestionsData.MainFieldsOfWork[currentIndex + 1],
                                Styling.ButtonStyle);
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    data.isMainFieldOfWorkOther = EditorGUILayout.ToggleLeft("Other",
                        data.isMainFieldOfWorkOther, GUILayout.ExpandWidth(false));

                    using (new EditorGUI.DisabledScope(!data.isMainFieldOfWorkOther))
                    {
                        data.mainFieldOfWorkOther = EditorGUILayout.TextArea(data.mainFieldOfWorkOther,
                            GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    }
                }

                if (changeScope.changed)
                {
                    if (data.IsAnyMainFieldOfWork() || data.isMainFieldOfWorkOther)
                    {
                        data.isMainFieldOfWorkNoAnswer = false;
                    }
                }
            }

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                data.isMainFieldOfWorkNoAnswer =
                    EditorGUILayout.ToggleLeft("No answer", data.isMainFieldOfWorkNoAnswer);
                if (changeScope.changed & data.isMainFieldOfWorkNoAnswer)
                {
                    data.isMainFieldOfWorkOther = false;
                    data.ResetMainFieldOfWork();
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawIsDevelopingGamesQuestion()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". Are you familiar with developing 2D Unity applications and do you know how to adjust SpriteRenderers' sorting options (Sorting Layer and Sorting Order)? (exclusion criterion)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            var developingGamesSelectionGridRect =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                data.developing2dGames = GUI.SelectionGrid(developingGamesSelectionGridRect, data.developing2dGames,
                    answers, 2);
                if (changeScope.changed)
                {
                    if (data.developing2dGames == 0)
                    {
                        GeneralData.isSurveyActive = true;
                        GeneralData.isAutomaticSortingActive = false;
                    }
                    else
                    {
                        GeneralData.isSurveyActive = false;
                        GeneralData.isAutomaticSortingActive = true;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawNumberOfApplicationsQuestions()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". If yes, on how many 2D Unity applications have you already worked?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            var answers = new string[] {"1 - 5", "6 - 10", "11 - 15", "> 15"};

            var selectionGridRect =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            using (new EditorGUI.DisabledScope(data.developing2dGames != 0))
            {
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    data.numberOfDeveloped2dGames =
                        GUI.SelectionGrid(selectionGridRect, data.numberOfDeveloped2dGames, answers, answers.Length);
                    if (changeScope.changed && data.numberOfDeveloped2dGames >= 0)
                    {
                        data.isNumberOfDeveloped2dGamesNoAnswer = false;
                    }
                }

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    data.isNumberOfDeveloped2dGamesNoAnswer =
                        EditorGUILayout.ToggleLeft("No answer", data.isNumberOfDeveloped2dGamesNoAnswer);
                    if (changeScope.changed & data.isNumberOfDeveloped2dGamesNoAnswer)
                    {
                        data.numberOfDeveloped2dGames = -1;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawIsExperiencedVisualGlitchQuestion()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". Have you worked on Unity 2D applications, where you experienced visual glitches (the order of Sprites to be rendered swapped)? (see preview)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            preview?.DoPreview();

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};

            using (new EditorGUI.DisabledScope(data.developing2dGames != 0))
            {
                data.workingOnApplicationWithVisualGlitch =
                    GUI.SelectionGrid(selectionGrid, data.workingOnApplicationWithVisualGlitch, answers, 2);
            }

            EditorGUI.indentLevel--;
        }
    }
}