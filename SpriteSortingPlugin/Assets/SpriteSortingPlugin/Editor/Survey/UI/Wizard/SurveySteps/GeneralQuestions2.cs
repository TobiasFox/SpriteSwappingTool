using System.IO;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class GeneralQuestions2 : SurveyStep
    {
        private const int QuestionCounterStart = 5;

        private static readonly string[] PreviewPrefabPathAndName = new string[]
        {
            "Assets",
            "SpriteSortingPlugin",
            "Editor",
            "Survey",
            "Prefabs",
            "GeneralQuestionPreview.prefab"
        };

        private GeneralQuestionsData data;
        private float space = 17.5f;
        private float previewHeight = 150;
        private SurveyPreview preview;
        private int questionCounter;

        public GeneralQuestions2(string name, GeneralQuestionsData data) : base(name)
        {
            this.data = data;
            preview = new SurveyPreview(Path.Combine(PreviewPrefabPathAndName));
        }

        public override void Commit()
        {
            base.Commit();

            Finish(SurveyFinishState.Succeeded);
            preview.CleanUp();
        }

        public override void Rollback()
        {
            base.Rollback();
            preview.CleanUp();
        }

        public override void DrawContent()
        {
            questionCounter = QuestionCounterStart;
            EditorGUI.indentLevel++;

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawQuestion1();
                questionCounter++;
            }

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawQuestion2();
                questionCounter++;
            }

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawQuestion3();
                questionCounter++;
            }

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawQuestion4();
                questionCounter++;
            }

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawQuestion5();
                questionCounter++;
            }

            EditorGUILayout.Space(space);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawQuestion6();
                questionCounter++;
            }

            EditorGUI.indentLevel--;
        }

        public override void CleanUp()
        {
            preview?.CleanUp();
        }

        public override bool IsSendingData()
        {
            return true;
        }

        private bool IsSkipped()
        {
            if (data.knowingVisualGlitches >= 0)
            {
                return false;
            }

            if (data.workingOnApplicationWithVisualGlitch < 0)
            {
                return true;
            }

            if (data.workingOnApplicationWithVisualGlitch == 1)
            {
                return false;
            }

            if (data.solvedVisualGlitches >= 0)
            {
                return false;
            }

            if (data.workingOnApplicationWithVisualGlitch < 0)
            {
                return true;
            }

            if (data.workingOnApplicationWithVisualGlitch == 1)
            {
                return false;
            }

            if (data.numberOfApplicationsWithVisualGlitches < 0 &&
                !data.isNotKnowingNumberOfApplicationsWithVisualGlitches &&
                !data.isNumberOfApplicationsWithVisualGlitchesNoAnswer)
            {
                return true;
            }

            if (data.solvedVisualGlitches >= 0)
            {
                return false;
            }

            return true;
        }

        private void DrawQuestion1()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". Do you already know the effect of visual glitches, where the order of Sprites to be rendered can swap? (see preview)",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};
            data.knowingVisualGlitches = GUI.SelectionGrid(selectionGrid, data.knowingVisualGlitches,
                answers, 2);

            preview?.DoPreview(previewHeight);

            EditorGUI.indentLevel--;
        }

        private void DrawQuestion2()
        {
            EditorGUILayout.LabelField(questionCounter + ". What do you think can cause such visual glitches?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Reasons");
                data.visualGlitchReasons = EditorGUILayout.TextArea(data.visualGlitchReasons,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight * 3.5f));
            }

            EditorGUI.indentLevel--;
        }

        private void DrawQuestion3()
        {
            EditorGUILayout.LabelField(questionCounter +
                                       ". Have you worked on Unity 2D applications, where such visual glitches occurred?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel++;

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};
            data.workingOnApplicationWithVisualGlitch =
                GUI.SelectionGrid(selectionGrid, data.workingOnApplicationWithVisualGlitch, answers, 2);

            EditorGUI.indentLevel--;
        }

        private void DrawQuestion4()
        {
            EditorGUILayout.LabelField(
                questionCounter + ". If [7] yes, in how many Unity 2D application occurred visual glitches?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel += 2;

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new string[] {"1 - 3", "3 - 6", "6 - 9", "9 - 12", "> 12"};
            using (new EditorGUI.DisabledScope(data.workingOnApplicationWithVisualGlitch != 0))
            {
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    data.numberOfApplicationsWithVisualGlitches = GUI.SelectionGrid(selectionGrid,
                        data.numberOfApplicationsWithVisualGlitches,
                        answers, answers.Length);

                    if (changeScope.changed && data.numberOfApplicationsWithVisualGlitches > -1)
                    {
                        data.isNotKnowingNumberOfApplicationsWithVisualGlitches = false;
                        data.isNumberOfApplicationsWithVisualGlitchesNoAnswer = false;
                    }
                }

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    data.isNotKnowingNumberOfApplicationsWithVisualGlitches = EditorGUILayout.ToggleLeft("Don't know",
                        data.isNotKnowingNumberOfApplicationsWithVisualGlitches);

                    if (changeScope.changed && data.isNotKnowingNumberOfApplicationsWithVisualGlitches)
                    {
                        data.numberOfApplicationsWithVisualGlitches = -1;
                        data.isNumberOfApplicationsWithVisualGlitchesNoAnswer = false;
                    }
                }

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    data.isNumberOfApplicationsWithVisualGlitchesNoAnswer =
                        EditorGUILayout.ToggleLeft("No answer", data.isNumberOfApplicationsWithVisualGlitchesNoAnswer);
                    if (changeScope.changed & data.isNumberOfApplicationsWithVisualGlitchesNoAnswer)
                    {
                        data.numberOfApplicationsWithVisualGlitches = -1;
                        data.isNotKnowingNumberOfApplicationsWithVisualGlitches = false;
                    }
                }
            }

            EditorGUI.indentLevel -= 2;
        }

        private void DrawQuestion5()
        {
            EditorGUILayout.LabelField(
                questionCounter + ". If [7] yes, could these visual glitches be fixed?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel += 2;

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new[] {"Yes", "No"};

            using (new EditorGUI.DisabledScope(data.workingOnApplicationWithVisualGlitch != 0))
            {
                data.solvedVisualGlitches = GUI.SelectionGrid(selectionGrid, data.solvedVisualGlitches, answers, 2);
            }

            EditorGUI.indentLevel -= 2;
        }

        private void DrawQuestion6()
        {
            EditorGUILayout.LabelField(questionCounter + ". If [8] yes, how were these visual glitches fixed?",
                Styling.QuestionLabelStyle);
            EditorGUI.indentLevel += 3;

            var isDisabled = data.workingOnApplicationWithVisualGlitch != 0 || data.solvedVisualGlitches != 0;
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel("Approach");
                    data.solvingVisualGlitchApproach = EditorGUILayout.TextArea(data.solvingVisualGlitchApproach,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight * 3.5f));
                }
            }

            EditorGUI.indentLevel -= 3;
        }
    }
}