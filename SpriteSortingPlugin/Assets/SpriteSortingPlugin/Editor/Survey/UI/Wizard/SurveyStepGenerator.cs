using System.Collections.Generic;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        private GeneralQuestionsData generalQuestionsData;
        private UsabilityData usabilityData;
        private UserData userData;
        private ComparingData comparingData;

        public List<SurveyStep> GenerateSurveySteps()
        {
            var surveySteps = new List<SurveyStep>();

            generalQuestionsData = new GeneralQuestionsData();
            usabilityData = new UsabilityData();
            userData = new UserData();
            comparingData = new ComparingData();

            var evaluationAutoSortingSuggestionLabel =
                "Evaluation of the functionality to generate sorting order suggestions";

            var sortingSuggestionStep1 = new SortingSuggestionStep1(evaluationAutoSortingSuggestionLabel);
            var sortingSuggestionStep2 = new SortingSuggestionStep2(evaluationAutoSortingSuggestionLabel);
            // surveySteps.Add(sortingSuggestionStep1);
            // surveySteps.Add(sortingSuggestionStep2);

            var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
            var manualSortingStep = new ManualSortingStep(manualSortingLabel);
            var manualSortingStep2 = new ManualSortingStep2(manualSortingLabel);
            // surveySteps.Add(comparisonManualSortingStep1);
            // surveySteps.Add(comparisonManualSortingStep2);

            var creatingSpriteData = new CreatingSpriteDataStep("Creating " + nameof(SpriteData) + " asset");
            // surveySteps.Add(creatingSpriteData);

            var pluginSorting1 = new PluginSorting1("Automatically detect visual glitches with the " +
                                                    GeneralData.Name + " " + GeneralData.DetectorName);
            var pluginSorting2 = new PluginSorting2("Automatically detect visual glitches with the " +
                                                    GeneralData.Name + " " + GeneralData.DetectorName);
            // surveySteps.Add(pluginSorting1);
            // surveySteps.Add(pluginSorting2);
            // surveySteps.Add(comparisonManualSortingStep2);


            var introStep = new IntroSurveyStep("Intro");
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

            // var finishingStep = new FinishingSurvey("Finalizing", userData);
            // surveySteps.Add(finishingStep);

            // surveySteps.AddRange(FinalSurveyStepList());

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

            //Comparison manual sorting and plugin usage
            // {
            //     var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
            //     var manualSortingScene1 = "";
            //     var manualSortingScene2 = "";
            //     // var manualSortingScene3 = "";
            //     var comparisonManualSortingStep1 =
            //         new ComparisonManualSortingStep(manualSortingLabel, comparingData);
            //     var comparisonManualSortingStep2 =
            //         new ComparisonManualSortingStep(manualSortingLabel, comparingData);
            //     // var comparisonManualSortingStep3 = new ComparisonManualSortingStep(manualSortingLabel, manualSortingScene3);
            //
            //     var list = new List<SurveyStep>();
            //     for (var i = 0; i < 6; i++)
            //     {
            //         list.Add(new ComparisonManualSortingStep(manualSortingLabel, comparingData));
            //     }
            //
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }

            //auto sorting
            // {
            //     var list = new List<SurveyStep>();
            //
            //     var evaluationAutoSortingSuggestionLabel =
            //         "Evaluation of the functionality to generate sorting order suggestions";
            //     for (int i = 0; i < 4; i++)
            //     {
            //         var sortingSuggestionStep1 = new SortingSuggestionStep1(evaluationAutoSortingSuggestionLabel, comparingData);
            //         list.Add(sortingSuggestionStep1);
            //     }
            //
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }

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