using System.Collections.Generic;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        private GeneralQuestionsData generalQuestionsData;

        public List<SurveyStep> GenerateSurveySteps()
        {
            var surveySteps = new List<SurveyStep>();
            generalQuestionsData = new GeneralQuestionsData();


            var generalQuestions1 = new GeneralQuestions1("General Questions", generalQuestionsData);
            var generalQuestions2 = new GeneralQuestions2("General Questions", generalQuestionsData);
            // surveySteps.Add(generalQuestions1);
            // surveySteps.Add(generalQuestions2);


            var introStep = new IntroSurveyStep("Intro");
            surveySteps.Add(introStep);
            var introStep2 = new IntroSurveyStep2("Intro");
            surveySteps.Add(introStep2);
            
            {
                var list = new List<SurveyStep>() {generalQuestions1, generalQuestions2};
                var group = new SurveyStepGroup(list, "General Questions");
                surveySteps.Add(group);
            }

            // {
            //     var list = new List<SurveyStep>() {new IntroSurveyStep("Intro"), new IntroSurveyStep2("Intro")};
            //     var group = new SurveyStepGroup(list, "Intro");
            //     surveySteps.Add(group);
            // }
            // {
            //     var list = new List<SurveyStep>() {new IntroSurveyStep("Intro"), new IntroSurveyStep2("Intro")};
            //     var group = new SurveyStepGroup(list, "Intro");
            //     surveySteps.Add(group);
            // }


            return surveySteps;
        }
    }
}