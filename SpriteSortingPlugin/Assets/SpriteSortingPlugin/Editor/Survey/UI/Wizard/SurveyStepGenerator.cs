using System.Collections.Generic;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        private GeneralQuestionsData generalQuestionsData;
        private UsabilityData usabilityData;
        private UserData userData;

        public List<SurveyStep> GenerateSurveySteps()
        {
            var surveySteps = new List<SurveyStep>();

            generalQuestionsData = new GeneralQuestionsData();
            usabilityData = new UsabilityData();
            userData = new UserData();

            var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
            var manualSortingScene1 = "";
            var manualSortingScene2 = "";
            // var manualSortingScene3 = "";
            var comparisonManualSortingStep1 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene1);
            var comparisonManualSortingStep2 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene2);
            // var comparisonManualSortingStep3 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene3);

            surveySteps.Add(comparisonManualSortingStep1);
            surveySteps.Add(comparisonManualSortingStep2);

            // var introStep = new IntroSurveyStep("Intro");
            // surveySteps.Add(introStep);
            // var introStep2 = new IntroSurveyStep2("Intro");
            // surveySteps.Add(introStep2);

            var generalQuestions1 = new GeneralQuestions1("General Questions", generalQuestionsData);
            var generalQuestions2 = new GeneralQuestions2("General Questions", generalQuestionsData);
            // surveySteps.Add(generalQuestions1);
            // surveySteps.Add(generalQuestions2);
            // {
            //     var list = new List<SurveyStep>() {generalQuestions1, generalQuestions2};
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }

            // surveySteps.Add(comparisonManualSortingStep3);

            // {
            //     var list = new List<SurveyStep>();
            //     for (int i = 0; i < 6; i++)
            //     {
            //         list.Add(new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene1));
            //     }
            //
            //     // var list = new List<SurveyStep>() {comparisonManualSortingStep1, comparisonManualSortingStep2};
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }
            //
            // {
            //     var autoSortingLabel = "Evaluation of the functionality to generate sorting order suggestions";
            //     var list = new List<SurveyStep>();
            //     for (int i = 0; i < 4; i++)
            //     {
            //         list.Add(new ComparisonManualSortingStep(manualSortingLabel, autoSortingLabel));
            //     }
            //
            //     // var list = new List<SurveyStep>() {comparisonManualSortingStep1, comparisonManualSortingStep2};
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }

            var usabilityQuestions1 = new UsabilityQuestions1("Usability", usabilityData);
            var usabilityQuestions2 = new UsabilityQuestions2("Usability", usabilityData);

            // surveySteps.Add(usabilityQuestions1);
            // surveySteps.Add(usabilityQuestions2);
            // {
            //     var list = new List<SurveyStep>() {usabilityQuestions1, usabilityQuestions2};
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }


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


            var finishingStep = new FinishingSurvey("Finalizing", userData);
            var sendingResultSurveyStep = new SendingResultSurveyStep("Sending Result");
            surveySteps.Add(finishingStep);
            surveySteps.Add(sendingResultSurveyStep);

            return surveySteps;
        }

        private List<SurveyStep> FinalSurveyStepList()
        {
            var surveySteps = new List<SurveyStep>();

            //intro
            {
                var introStep = new IntroSurveyStep("Intro");
                var introStep2 = new IntroSurveyStep2("Intro");
                surveySteps.Add(introStep);
                surveySteps.Add(introStep2);
            }

            //General questions
            {
                var generalQuestions1 = new GeneralQuestions1("General Questions", generalQuestionsData);
                var generalQuestions2 = new GeneralQuestions2("General Questions", generalQuestionsData);
                var list = new List<SurveyStep>() {generalQuestions1, generalQuestions2};
                var group = new SurveyStepGroup(list, "");
                surveySteps.Add(group);
            }


            {
                var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
                var manualSortingScene1 = "";
                var manualSortingScene2 = "";
                // var manualSortingScene3 = "";
                var comparisonManualSortingStep1 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene1);
                var comparisonManualSortingStep2 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene2);
                // var comparisonManualSortingStep3 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene3);
                
                var list = new List<SurveyStep>();
                for (var i = 0; i < 6; i++)
                {
                    list.Add(new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene1));
                }

                var group = new SurveyStepGroup(list, "");
                surveySteps.Add(group);
            }

            //Usability
            {
                var usabilityQuestions1 = new UsabilityQuestions1("Usability", usabilityData);
                var usabilityQuestions2 = new UsabilityQuestions2("Usability", usabilityData);
                var list = new List<SurveyStep>() {usabilityQuestions1, usabilityQuestions2};
                var group = new SurveyStepGroup(list, "");
                surveySteps.Add(group);
            }

            return surveySteps;
        }
    }
}