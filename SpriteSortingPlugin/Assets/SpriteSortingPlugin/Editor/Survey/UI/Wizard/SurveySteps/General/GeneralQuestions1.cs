using System;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class GeneralQuestions1 : SurveyStep
    {
        private const int QuestionCounterStart = 1;

        private static readonly string[] MainFields = new string[]
        {
            "Design", "Programming", "Game Design", "3D Modelling", "Audio", "Texture Artist", "VFX Artist",
            "Animator", "Testing/QA", "Marketing"
        };

        private GeneralQuestionsData data;
        private float space = 17.5f;
        private int questionCounter;

        public GeneralQuestions1(string name, GeneralQuestionsData data) : base(name)
        {
            this.data = data;
        }

        public override void Commit()
        {
            base.Commit();

            // var isSkipped = IsSkipped();
            Finish(SurveyFinishState.Succeeded);
        }

        public override void DrawContent()
        {
            questionCounter = QuestionCounterStart;
            EditorGUI.indentLevel++;

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

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawIsDevelopingGamesQuestion();
                questionCounter++;
            }

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawNumberOfApplicationsQuestions();
                questionCounter++;
            }

            EditorGUI.indentLevel--;
        }

        private bool IsSkipped()
        {
            if (data.isGameDevelopmentStudent || data.isWorkingInGameDevelopment ||
                data.isGameDevelopmentHobbyist || data.isNotDevelopingGames ||
                data.isGameDevelopmentRelationOther || data.isGameDevelopmentRelationNoAnswer)
            {
                return false;
            }

            if (data.mainFieldOfWork >= 0 || data.isMainFieldOfWorkOther || data.isMainFieldOfWorkNoAnswer)
            {
                return false;
            }

            if (data.developing2dGames < 0)
            {
                return true;
            }

            if (data.developing2dGames == 1)
            {
                return false;
            }

            if (data.numberOfDeveloped2dGames >= 0 || data.isNumberOfDeveloped2dGamesNoAnswer)
            {
                return false;
            }

            return true;
        }

        private void DrawGameDevelopmentRelation()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". How are you related to the development of games? (multi-choice)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                data.isGameDevelopmentStudent =
                    EditorGUILayout.ToggleLeft("Professional", data.isGameDevelopmentStudent);
                data.isWorkingInGameDevelopment =
                    EditorGUILayout.ToggleLeft("Student", data.isWorkingInGameDevelopment);
                data.isGameDevelopmentHobbyist =
                    EditorGUILayout.ToggleLeft("Hobbyist", data.isGameDevelopmentHobbyist);
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

        private void DrawMainFieldOfWork()
        {
            EditorGUILayout.LabelField(
                questionCounter +
                ". If you develop games, what best describes your main field of work? (single-choice)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                var rows = Mathf.Ceil(MainFields.Length / 2f);
                var height = EditorGUIUtility.singleLineHeight * rows + (Math.Max(0, rows - 1) * 3);

                var selectionGridRect =
                    EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, height));

                using (var changeSelectionGridScope = new EditorGUI.ChangeCheckScope())
                {
                    data.mainFieldOfWork =
                        GUI.SelectionGrid(selectionGridRect, data.mainFieldOfWork, MainFields, 2);

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

        private void DrawIsDevelopingGamesQuestion()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". Are you familiar with developing 2D Unity applications?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;


            var developingGamesSelectionGridRect =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};
            data.developing2dGames = GUI.SelectionGrid(developingGamesSelectionGridRect, data.developing2dGames,
                answers, 2);

            EditorGUI.indentLevel--;
        }

        private void DrawNumberOfApplicationsQuestions()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". If yes, on how many 2D Unity applications have you been working before?",
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