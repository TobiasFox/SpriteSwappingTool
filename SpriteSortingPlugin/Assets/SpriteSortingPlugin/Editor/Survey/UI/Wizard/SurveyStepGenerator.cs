#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGenerator
    {
        public List<SurveyStep> GenerateSurveySteps(SurveyData surveyData)
        {
            return FinalSurveyStepList(surveyData);
        }

        private List<SurveyStep> FinalSurveyStepList(SurveyData surveyData)
        {
            var surveySteps = new List<SurveyStep>();
            var groupCounter = 1;

            //intro
            {
                var introStep = new IntroSurveyStep("Intro");
                surveySteps.Add(introStep);
            }

            //General questions
            {
                var generalQuestions1 = new GeneralQuestions("General Questions", surveyData.generalQuestionsData);
                var list = new List<SurveyStep>() {generalQuestions1};
                var group = new SurveyStepGroup(list, $"Part {groupCounter}");
                surveySteps.Add(group);
                groupCounter++;
            }

            //Comparison manual sorting and plugin usage
            {
                var manualSortingLabel = "Comparing manual approach and using the Sprite Swapping tool";
                var manualSortingStep = new ManualSortingStep(manualSortingLabel);
                var manualSortingStep2 = new ManualSortingStep2(manualSortingLabel);

                var list = new List<SurveyStep>
                    {manualSortingStep, manualSortingStep2};

                var group = new SurveyStepGroup(list, $"Part {groupCounter}");
                surveySteps.Add(group);
                groupCounter++;
            }

            //plugin usage
            {
                var pluginSortingLabel = "Automatically detect visual glitches with the " +
                                         GeneralData.FullDetectorName;
                var pluginSortingIntro = new PluginSortingIntro(pluginSortingLabel);
                var pluginSorting2 = new PluginSorting2(pluginSortingLabel);
                var list = new List<SurveyStep> {pluginSortingIntro, pluginSorting2};

                var group = new SurveyStepGroup(list, $"Part {groupCounter}");
                surveySteps.Add(group);
                groupCounter++;
            }

            //sorting suggestion
            {
                var evaluationAutoSortingSuggestionLabel =
                    "Evaluation of the functionality to generate sorting order suggestions";

                var sortingSuggestionStepIntro = new SortingSuggestionStepIntro(evaluationAutoSortingSuggestionLabel);
                var sortingSuggestionStep1 = new SortingSuggestionStep1(evaluationAutoSortingSuggestionLabel);

                var list = new List<SurveyStep> {sortingSuggestionStepIntro, sortingSuggestionStep1};
                var group = new SurveyStepGroup(list, $"Part {groupCounter}");
                surveySteps.Add(group);
                groupCounter++;
            }

            //Usability
            {
                var usabilityQuestions1 = new UsabilityQuestions1("Usability", surveyData.usabilityData);
                var usabilityQuestions2 = new UsabilityQuestions2("Usability", surveyData.usabilityData);
                var list = new List<SurveyStep>() {usabilityQuestions1, usabilityQuestions2};
                var group = new SurveyStepGroup(list, $"Part {groupCounter}");
                surveySteps.Add(group);
            }

            var finishingStep = new FinishingSurvey("Finalizing");
            surveySteps.Add(finishingStep);

            return surveySteps;
        }
    }
}