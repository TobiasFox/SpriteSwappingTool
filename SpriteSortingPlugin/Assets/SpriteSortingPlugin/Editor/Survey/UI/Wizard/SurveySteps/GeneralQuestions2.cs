using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class GeneralQuestions2 : SurveyStep
    {
        private const int QuestionCounterStart = 5;

        private GeneralQuestionsData data;
        private float space = 17.5f;
        private float previewHeight = 150;
        private SurveyPreview preview;
        private int questionCounter;

        public GeneralQuestions2(string name, GeneralQuestionsData data) : base(name)
        {
            this.data = data;
            preview = new SurveyPreview();
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
                                       ". Have you been working on 2D Unity applications where such visual glitches have happened?",
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
            EditorGUILayout.LabelField(questionCounter + ". If yes, how many?", Styling.QuestionLabelStyle);
            EditorGUI.indentLevel += 2;

            var selectionGrid =
                EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            var answers = new string[] {"1 - 3", "3 - 6", "6 - 9", "9 - 12", "> 12"};
            using (new EditorGUI.DisabledScope(data.workingOnApplicationWithVisualGlitch != 0))
            {
                data.numberOfApplicationsWithVisualGlitches = GUI.SelectionGrid(selectionGrid,
                    data.numberOfApplicationsWithVisualGlitches,
                    answers, answers.Length);
            }

            EditorGUI.indentLevel -= 2;
        }

        private void DrawQuestion5()
        {
            EditorGUILayout.LabelField(
                questionCounter + ". If yes, was there a solution found for these visual glitches?",
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
            EditorGUILayout.LabelField(questionCounter + ". If yes, how were these visual glitches been solved?",
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