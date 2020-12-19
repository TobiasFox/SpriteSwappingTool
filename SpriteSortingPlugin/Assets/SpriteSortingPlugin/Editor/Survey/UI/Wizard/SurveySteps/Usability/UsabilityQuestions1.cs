using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;

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
            questionCounter = QuestionCounterStart;
            DrawSusQuestions();
        }

        public override void Commit()
        {
            base.Commit();

            Finish(SurveyFinishState.Succeeded);
        }

        private bool IsSkipped()
        {
            foreach (var susAnswer in data.susAnswers)
            {
                if (susAnswer >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void DrawSusQuestions()
        {
            UsabilityQuestionsUtility.DrawRatingHeader(QuestionWidthPercentage, "Overall usability of the system",
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