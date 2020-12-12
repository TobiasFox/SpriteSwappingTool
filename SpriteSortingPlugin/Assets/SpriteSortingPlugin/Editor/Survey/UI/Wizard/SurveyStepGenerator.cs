using System.Collections.Generic;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        public List<SurveyStep> GenerateSurveySteps()
        {
            var surveySteps = new List<SurveyStep>();

            var introStep = new IntroSurveyStep("Intro");
            surveySteps.Add(introStep);
            var introStep2 = new IntroSurveyStep2("Intro");
            surveySteps.Add(introStep2);


            return surveySteps;
        }
    }
}