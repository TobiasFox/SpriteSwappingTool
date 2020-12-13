using System.Collections.Generic;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        public List<SurveyStep> GenerateSurveySteps()
        {
            var surveySteps = new List<SurveyStep>();
            {
                var list = new List<SurveyStep>() {new IntroSurveyStep("Intro"), new IntroSurveyStep2("Intro")};
                var group = new SurveyStepGroup(list, "Intro");
                surveySteps.Add(group);
            }
            {
                var list = new List<SurveyStep>() {new IntroSurveyStep("Intro"), new IntroSurveyStep2("Intro")};
                var group = new SurveyStepGroup(list, "Intro");
                surveySteps.Add(group);
            }

            // var introStep = new IntroSurveyStep("Intro");
            // surveySteps.Add(introStep);
            // var introStep2 = new IntroSurveyStep2("Intro");
            // surveySteps.Add(introStep2);


            return surveySteps;
        }
    }
}