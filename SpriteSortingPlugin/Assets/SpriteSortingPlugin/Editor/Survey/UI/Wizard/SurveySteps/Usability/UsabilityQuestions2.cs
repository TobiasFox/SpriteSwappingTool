using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class UsabilityQuestions2 : SurveyStep
    {
        private const int QuestionCounterStart = 10;
        private const float QuestionWidthPercentage = 0.6f;

        private static readonly float HighlightHeight = EditorGUIUtility.singleLineHeight * 4;
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

        // public override bool IsSendingData()
        // {
        //     return true;
        // }

        public override void DrawContent()
        {
            questionCounter = QuestionCounterStart;

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawHighLowlightQuestion();
                questionCounter++;
            }

            EditorGUILayout.Space();
            DrawRatingQuestions();

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawMissingCriteriaText();
                questionCounter++;
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                DrawMissingFunctionalityText();
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
        }

        private bool IsSkipped()
        {
            foreach (var ratingAnswer in data.ratingAnswers)
            {
                if (ratingAnswer >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void DrawHighLowlightQuestion()
        {
            EditorGUILayout.LabelField(
                questionCounter + ". What are your highlights and lowlights of the tool?",
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
                ". Which criteria was missing when using the functionality to generate Sprite order suggestions?",
                Styling.QuestionLabelStyle);
            using (new EditorGUI.IndentLevelScope())
            {
                data.missingCriteria = EditorGUILayout.TextArea(data.missingCriteria, GUILayout.Height(TextAreaHeight));
            }
        }

        private void DrawMissingFunctionalityText()
        {
            EditorGUILayout.LabelField(
                questionCounter +
                ". Is there any functionality desired and has not yet been implemented by the Sprite Swapping tool?",
                Styling.QuestionLabelStyle);
            using (new EditorGUI.IndentLevelScope())
            {
                data.missingFunctionality =
                    EditorGUILayout.TextArea(data.missingFunctionality, GUILayout.Height(TextAreaHeight));
            }
        }

        private void DrawOccuringErrorsText()
        {
            EditorGUILayout.LabelField(questionCounter + ". Did any errors occur while using the Sprite Swapping tool?",
                Styling.QuestionLabelStyle);
            using (new EditorGUI.IndentLevelScope())
            {
                data.occuringErrors = EditorGUILayout.TextArea(data.occuringErrors, GUILayout.Height(TextAreaHeight));
            }
        }

        private void DrawMiscellaneousText()
        {
            EditorGUILayout.LabelField(
                questionCounter +
                ". Is there anything else you want to share?",
                Styling.QuestionLabelStyle);
            using (new EditorGUI.IndentLevelScope())
            {
                data.miscellaneous = EditorGUILayout.TextArea(data.miscellaneous, GUILayout.Height(TextAreaHeight));
            }
        }

        private void DrawRatingQuestions()
        {
            UsabilityQuestionsUtility.DrawRatingHeader(QuestionWidthPercentage, "Usage rating", "Hard to use",
                "Easy to use", false);
            for (var i = 0; i < RatingQuestions.Length; i++)
            {
                if (i == 2)
                {
                    UsabilityQuestionsUtility.DrawRatingHeader(QuestionWidthPercentage, "Helpfulness", "Helpful",
                        "Not helpful", false);
                }

                using (new EditorGUILayout.HorizontalScope(Styling.HelpBoxStyle))
                {
                    data.ratingAnswers[i] = UsabilityQuestionsUtility.DrawSingleRatingQuestion(data.ratingAnswers[i],
                        QuestionWidthPercentage,
                        questionCounter, RatingQuestions[i]);
                    questionCounter++;
                }

                EditorGUILayout.Space();
            }
        }
    }
}