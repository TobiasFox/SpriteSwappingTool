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
using SpriteSwappingPlugin.Survey.Data;

namespace SpriteSwappingPlugin.Survey.UI.Wizard
{
    public class SurveyWizard
    {
        private List<SurveyStep> steps;
        private List<SurveyStepGroup> surveyStepGroups;
        private int currentProgress;
        private int currentSurveyStepIndex;
        private int totalProgress;

        public int TotalProgress => totalProgress;

        public int CurrentProgress => currentProgress;

        public void Forward()
        {
            if (currentSurveyStepIndex < 0 || currentSurveyStepIndex >= steps.Count)
            {
                return;
            }

            var surveyStep = steps[currentSurveyStepIndex];
            surveyStep.Commit();

            if (surveyStep is SurveyStepGroup surveyStepGroup &&
                !surveyStepGroup.IsFinished)
            {
                surveyStepGroup.Forward();
            }
            else
            {
                if (currentSurveyStepIndex + 1 < steps.Count)
                {
                    currentSurveyStepIndex++;
                    steps[currentSurveyStepIndex].Start();
                }
            }

            UpdateCurrentProgress();
        }

        public SurveyStep GetCurrent()
        {
            if (steps == null || currentSurveyStepIndex < 0 || currentSurveyStepIndex >= steps.Count)
            {
                return null;
            }

            return steps[currentSurveyStepIndex];
        }

        public void SetSurveySteps(List<SurveyStep> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                return;
            }

            this.steps = steps;
            this.steps[0].Start();

            currentSurveyStepIndex = 0;
            totalProgress = 0;
            UpdateCurrentProgress();
            if (surveyStepGroups == null)
            {
                surveyStepGroups = new List<SurveyStepGroup>();
            }
            else
            {
                surveyStepGroups.Clear();
            }

            foreach (var surveyStep in steps)
            {
                if (surveyStep is SurveyStepGroup surveyStepGroup)
                {
                    totalProgress += surveyStepGroup.TotalProgress;
                    surveyStepGroups.Add(surveyStepGroup);
                }
                else
                {
                    totalProgress++;
                }
            }

            if (totalProgress > 0)
            {
                totalProgress--;
            }
        }

        public void CleanUp()
        {
            if (steps == null)
            {
                return;
            }

            foreach (var surveyStep in steps)
            {
                surveyStep.CleanUp();
            }
        }

        public List<SurveyStep> GetSurveySteps()
        {
            return steps;
        }

        public List<SurveyStepGroup> GetSurveyStepGroups()
        {
            return surveyStepGroups;
        }

        public List<SortingTaskData> GetSortingTaskDataList()
        {
            var sortingTaskDataList = new List<SortingTaskData>();

            var partCounter = 1;
            foreach (var surveyStep in steps)
            {
                if (!surveyStep.IsFinished)
                {
                    break;
                }

                var isStepGroup = surveyStep is SurveyStepGroup;

                if (!surveyStep.GetSortingTaskData(out var taskDataList))
                {
                    if (isStepGroup)
                    {
                        partCounter++;
                    }

                    continue;
                }

                if (isStepGroup)
                {
                    foreach (var sortingTaskData in taskDataList)
                    {
                        sortingTaskData.surveyPart = partCounter;
                    }

                    partCounter++;
                }

                sortingTaskDataList.AddRange(taskDataList);
            }

            return sortingTaskDataList;
        }

        public List<string> CollectFilePathsToCopy()
        {
            List<string> collectedDataPathList = null;

            for (var i = 0; i < currentSurveyStepIndex; i++)
            {
                var surveyStep = steps[i];
                var collectedPaths = surveyStep.CollectFilePathsToCopy();

                if (collectedPaths == null || collectedPaths.Count <= 0)
                {
                    continue;
                }

                if (collectedDataPathList == null)
                {
                    collectedDataPathList = new List<string>();
                }

                collectedDataPathList.AddRange(collectedPaths);
            }

            return collectedDataPathList;
        }

        private void UpdateCurrentProgress()
        {
            currentProgress = 0;

            if (steps == null || steps.Count == 0)
            {
                return;
            }

            foreach (var surveyStep in steps)
            {
                if (!surveyStep.IsStarted)
                {
                    break;
                }

                if (surveyStep is SurveyStepGroup surveyStepGroup)
                {
                    currentProgress += surveyStepGroup.CurrentProgress;
                }
                else if (surveyStep.IsFinished)
                {
                    currentProgress++;
                }
            }
        }
    }
}