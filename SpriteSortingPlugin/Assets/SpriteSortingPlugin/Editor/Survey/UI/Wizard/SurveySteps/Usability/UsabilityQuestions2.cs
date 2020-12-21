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

        private static readonly float HighlightHeight = EditorGUIUtility.singleLineHeight * 3;
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

        public override void Commit()
        {
            base.Commit();

            Finish(SurveyFinishState.Succeeded);
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;
            questionCounter = QuestionCounterStart;

            EditorGUILayout.LabelField(
                "The following usability questions addresses specific parts of the " + GeneralData.Name + " tool.",
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
                DrawHighLowlightQuestion();
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
                DrawOccuringErrorsText();
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

        private void DrawRatingQuestion(int index)
        {
            EditorGUILayout.LabelField(questionCounter + ". " + RatingQuestions[index], Styling.QuestionLabelStyle);

            using (new EditorGUI.IndentLevelScope())
            {
                var entireQuestionRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false));
                data.ratingAnswers[index] =
                    (int) GUI.HorizontalSlider(entireQuestionRect, data.ratingAnswers[index], 0, 100);
            }
        }

        private void DrawHighLowlightQuestion()
        {
            EditorGUILayout.LabelField(
                questionCounter + ". What are your highlights and lowlights of the " + GeneralData.Name +
                " tool? (optional)",
                Styling.QuestionLabelStyle);

            using (new EditorGUI.IndentLevelScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    var heightOption = GUILayout.Height(HighlightHeight);
                    using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        EditorGUILayout.LabelField("Highlights", Styling.CenteredStyle);
                        data.highlights = EditorGUILayout.TextArea(data.highlights, heightOption);
                    }

                    using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        EditorGUILayout.LabelField("Lowlights", Styling.CenteredStyle);
                        data.lowlights = EditorGUILayout.TextArea(data.lowlights, heightOption);
                    }
                }
            }
        }

        private void DrawMissingCriteriaText()
        {
            EditorGUILayout.LabelField(
                questionCounter +
                ". Which criteria was missing when using the functionality to generate Sprite order suggestions? (optional)",
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
                questionCounter + ". Did any errors occur while using the Sprite Swapping tool? (optional)",
                Styling.QuestionLabelStyle);

            DrawOptionalTextInputWithBool(ref data.isOccuringError, ref data.occuringErrorsText);
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