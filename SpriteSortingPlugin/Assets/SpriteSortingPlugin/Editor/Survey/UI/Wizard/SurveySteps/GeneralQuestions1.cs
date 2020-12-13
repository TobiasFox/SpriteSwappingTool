using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class GeneralQuestions1 : SurveyStep
    {
        private GeneralQuestionsData data;
        private float space = 17.5f;

        public GeneralQuestions1(string name, GeneralQuestionsData data) : base(name)
        {
            this.data = data;
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            DrawQuestion1();

            EditorGUILayout.Space(space);
            DrawQuestion2();

            EditorGUILayout.Space(space);
            DrawQuestion3();

            EditorGUILayout.Space(space);
            DrawQuestion4();
            EditorGUI.indentLevel--;
        }

        private void DrawQuestion1()
        {
            EditorGUILayout.LabelField("1. How are you related to the development of games? (multi-choice)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                data.isGameDevelopmentStudent = EditorGUILayout.ToggleLeft("Student in the field of game development",
                    data.isGameDevelopmentStudent);
                data.isWorkingInGameDevelopment = EditorGUILayout.ToggleLeft("Working in the field of game development",
                    data.isWorkingInGameDevelopment);
                data.isGameDevelopmentHobbyist =
                    EditorGUILayout.ToggleLeft("Developing games in your free time", data.isGameDevelopmentHobbyist);
                data.isNotDevelopingGames =
                    EditorGUILayout.ToggleLeft("Not developing games", data.isNotDevelopingGames);

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
                    if (data.isGameDevelopmentStudent || data.isWorkingInGameDevelopment ||
                        data.isGameDevelopmentHobbyist || data.isNotDevelopingGames ||
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
                    data.isGameDevelopmentStudent = false;
                    data.isWorkingInGameDevelopment = false;
                    data.isGameDevelopmentHobbyist = false;
                    data.isNotDevelopingGames = false;
                    data.isGameDevelopmentRelationOther = false;
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawQuestion2()
        {
            EditorGUILayout.LabelField(
                "2. If you develop games, what best describes your main field of work? (single-choice)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                var mainFields = new string[] {"Design", "Programming", "Audio", "Marketing", "All-rounder"};

                var rows = Mathf.Ceil(mainFields.Length / 2f);
                var height = EditorGUIUtility.singleLineHeight * rows + (Math.Max(0, rows - 1) * 3);

                var selectionGridRect =
                    EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, height));

                using (var changeSelectionGridScope = new EditorGUI.ChangeCheckScope())
                {
                    data.mainFieldOfWork =
                        GUI.SelectionGrid(selectionGridRect, data.mainFieldOfWork, mainFields, 2);

                    if (changeSelectionGridScope.changed && data.isMainFieldOfWorkOther)
                    {
                        data.isMainFieldOfWorkOther = false;
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    using (var isOtherChangeScope = new EditorGUI.ChangeCheckScope())
                    {
                        data.isMainFieldOfWorkOther = EditorGUILayout.ToggleLeft("Other",
                            data.isMainFieldOfWorkOther, GUILayout.ExpandWidth(false));
                        if (isOtherChangeScope.changed && data.mainFieldOfWork >= 0)
                        {
                            data.mainFieldOfWork = -1;
                        }
                    }

                    using (new EditorGUI.DisabledScope(!data.isMainFieldOfWorkOther))
                    {
                        data.mainFieldOfWorkOther = EditorGUILayout.TextArea(data.mainFieldOfWorkOther,
                            GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    }
                }

                if (changeScope.changed)
                {
                    if (data.mainFieldOfWork >= 0 || data.isMainFieldOfWorkOther)
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
                    data.mainFieldOfWork = -1;
                    data.isMainFieldOfWorkOther = false;
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawQuestion3()
        {
            EditorGUILayout.LabelField("3. Are you familiar with developing 2D Unity applications?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;


            var developingGamesSelectionGridRect =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};
            data.developing2dGames = GUI.SelectionGrid(developingGamesSelectionGridRect, data.developing2dGames,
                answers, 2);

            EditorGUI.indentLevel--;
        }

        private void DrawQuestion4()
        {
            EditorGUILayout.LabelField("4. If yes, on how many 2D Unity applications have you been working before?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            var answers = new string[] {"1 - 3", "3 - 6", "6 - 9", "9 - 12", "> 12"};

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
    }
}