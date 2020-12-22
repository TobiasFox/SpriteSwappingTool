using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class GeneralQuestions1 : SurveyStep
    {
        private const int QuestionCounterStart = 1;
        private const float IndentLevelWidth = 10;

        private GeneralQuestionsData data;
        private float space = 17.5f;
        private int questionCounter;

        public GeneralQuestions1(string name, GeneralQuestionsData data) : base(name)
        {
            this.data = data;
        }

        public override void DrawContent()
        {
            questionCounter = QuestionCounterStart;
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(
                "At the beginning of this survey some general questions are asked.", Styling.LabelWrapStyle);
            EditorGUILayout.Space(20);
            // using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            // {
            //     DrawUnderstandingEnglishQuestion();
            //     questionCounter++;
            // }

            //criterion of exclusion
            // if (data.understandingEnglish == 1)
            // {
            //     DrawExclusion();
            //     EditorGUI.indentLevel--;
            //     return;
            // }

            // using (new EditorGUI.DisabledScope(data.understandingEnglish != 0))
            // {
            // EditorGUILayout.Space(space);
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
            // }

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

        private void DrawExclusion()
        {
            EditorGUILayout.Space(20);
            var centeredStyle = new GUIStyle(Styling.CenteredStyle) {wordWrap = true};

            var excludeMessage =
                "Thank you very much for your interest! However, this survey presumes";

            if (data.understandingEnglish == 1)
            {
                excludeMessage += " that you can read and understand english";
            }
            else if (data.developing2dGames == 1)
            {
                excludeMessage += " experiences in developing 2d Unity applications";
            }

            excludeMessage += ".\nYou can close this window now.";

            EditorGUILayout.LabelField(excludeMessage, centeredStyle);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField(
                "However, if you want you can try using the " + GeneralData.Name +
                " tool, which is located here:",
                centeredStyle);

            EditorGUILayout.LabelField(
                GeneralData.UnityMenuMainCategory + " -> " + GeneralData.Name + " -> " +
                GeneralData.DetectorName,
                centeredStyle);
        }

        private void DrawUnderstandingEnglishQuestion()
        {
            EditorGUILayout.LabelField(questionCounter + ". Can you reed and understand english?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};

            data.understandingEnglish = GUI.SelectionGrid(selectionGrid, data.understandingEnglish, answers, 2);

            EditorGUI.indentLevel--;
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
                ". If you develop games, what best describes your main field of work? (multi-choice)",
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
                                       ". Are you familiar with developing 2D Unity applications? (exclusion criterion)",
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
                                       ". If yes, on how many 2D Unity applications have you already worked?",
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

        private void DrawIsExperiencedVisualGlitchQuestion()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". Have you worked on Unity 2D applications, where you experienced visual glitches (the order of Sprites to be rendered swapped)?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

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