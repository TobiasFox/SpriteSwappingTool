using System.Collections.Generic;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        public List<SurveyStep> GenerateSurveySteps(SurveyData surveyData)
        {
            var surveySteps = new List<SurveyStep>();

            var introStep = new IntroSurveyStep("Intro");
            var introStep2 = new IntroSurveyStep2("Intro");
            // surveySteps.Add(introStep);
            // surveySteps.Add(introStep2);

            var evaluationAutoSortingSuggestionLabel =
                "Evaluation of the functionality to generate sorting order suggestions";

            var sortingSuggestionStep1 = new SortingSuggestionStep1(evaluationAutoSortingSuggestionLabel);
            var sortingSuggestionStep2 = new SortingSuggestionStep2(evaluationAutoSortingSuggestionLabel);
            // surveySteps.Add(sortingSuggestionStep1);
            // surveySteps.Add(sortingSuggestionStep2);

            var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
            var manualSortingStep = new ManualSortingStep(manualSortingLabel);
            var manualSortingStep2 = new ManualSortingStep2(manualSortingLabel);
            // surveySteps.Add(manualSortingStep);
            // surveySteps.Add(manualSortingStep2);

            var creatingSpriteData = new CreatingSpriteDataStep("Creating " + nameof(SpriteData) + " asset");
            // surveySteps.Add(creatingSpriteData);

            var pluginSorting1 = new PluginSorting1("Automatically detect visual glitches with the " +
                                                    GeneralData.Name + " " + GeneralData.DetectorName);
            var pluginSorting2 = new PluginSorting2("Automatically detect visual glitches with the " +
                                                    GeneralData.Name + " " + GeneralData.DetectorName);
            // surveySteps.Add(pluginSorting1);
            // surveySteps.Add(pluginSorting2);
            // {
            //     var list = new List<SurveyStep>() {pluginSorting1, pluginSorting2};
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }
            // surveySteps.Add(introStep2);


            var generalQuestions1 = new GeneralQuestions1("General Questions", surveyData.generalQuestionsData);
            var generalQuestions2 = new GeneralQuestions2("General Questions", surveyData.generalQuestionsData);
            // surveySteps.Add(generalQuestions1);
            // surveySteps.Add(generalQuestions2);
            // {
            //     var list = new List<SurveyStep>() {generalQuestions1, generalQuestions2};
            //     var group = new SurveyStepGroup(list, "");
            //     surveySteps.Add(group);
            // }

            var usabilityQuestions1 = new UsabilityQuestions1("Usability", surveyData.usabilityData);
            var usabilityQuestions2 = new UsabilityQuestions2("Usability", surveyData.usabilityData);

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

            var finishingStep = new FinishingSurvey("Finalizing");
            // surveySteps.Add(finishingStep);

            surveySteps.AddRange(FinalSurveyStepList(surveyData));

            return surveySteps;
        }

        private List<SurveyStep> FinalSurveyStepList(SurveyData surveyData)
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
                var generalQuestions1 = new GeneralQuestions1("General Questions", surveyData.generalQuestionsData);
                var generalQuestions2 = new GeneralQuestions2("General Questions", surveyData.generalQuestionsData);
                var list = new List<SurveyStep>() {generalQuestions1, generalQuestions2};
                var group = new SurveyStepGroup(list, "General Questions");
                surveySteps.Add(group);
            }

            //Comparison manual sorting and plugin usage
            {
                var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
                var pluginSortingLabel = "Automatically detect visual glitches with the " +
                                         GeneralData.Name + " " + GeneralData.DetectorName;

                var manualSortingStep = new ManualSortingStep(manualSortingLabel);
                var manualSortingStep2 = new ManualSortingStep2(manualSortingLabel);
                var pluginSorting1 = new PluginSorting1(pluginSortingLabel);
                var pluginSorting2 = new PluginSorting2(pluginSortingLabel);

                var list = new List<SurveyStep> {manualSortingStep, manualSortingStep2, pluginSorting1, pluginSorting2};

                var group = new SurveyStepGroup(list, "Comparing manual approach and using the Sprite Swapping tool");
                surveySteps.Add(group);
            }

            //auto sorting
            {
                var evaluationAutoSortingSuggestionLabel =
                    "Evaluation of the functionality to generate sorting order suggestions";

                var sortingSuggestionStep1 = new SortingSuggestionStep1(evaluationAutoSortingSuggestionLabel);
                var sortingSuggestionStep2 = new SortingSuggestionStep2(evaluationAutoSortingSuggestionLabel);

                var list = new List<SurveyStep> {sortingSuggestionStep1, sortingSuggestionStep2};
                var group = new SurveyStepGroup(list, evaluationAutoSortingSuggestionLabel);
                surveySteps.Add(group);
            }

            //Usability
            {
                var usabilityQuestions1 = new UsabilityQuestions1("Usability", surveyData.usabilityData);
                var usabilityQuestions2 = new UsabilityQuestions2("Usability", surveyData.usabilityData);
                var list = new List<SurveyStep>() {usabilityQuestions1, usabilityQuestions2};
                var group = new SurveyStepGroup(list, "Usability");
                surveySteps.Add(group);
            }

            var finishingStep = new FinishingSurvey("Finalizing");
            surveySteps.Add(finishingStep);

            return surveySteps;
        }
    }
}