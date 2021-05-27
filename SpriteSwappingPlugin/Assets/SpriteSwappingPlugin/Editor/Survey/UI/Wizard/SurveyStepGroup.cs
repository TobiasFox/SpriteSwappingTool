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
    public class SurveyStepGroup : SurveyStep
    {
        private List<SurveyStep> steps;
        private int currentProgress;

        public int CurrentProgress
        {
            get
            {
                if (!surveyStepData.isStarted)
                {
                    return 0;
                }

                if (surveyStepData.isFinished)
                {
                    return currentProgress + 1;
                }

                return currentProgress;
            }
        }

        public int TotalProgress => steps?.Count ?? 0;

        public SurveyStepGroup(List<SurveyStep> surveySteps, string groupName) : base(groupName)
        {
            steps = surveySteps;
        }

        public override void Start()
        {
            base.Start();
            steps[currentProgress].Start();
        }

        public override void Commit()
        {
            if (currentProgress < 0 || currentProgress >= steps.Count)
            {
                return;
            }

            steps[currentProgress].Commit();

            if (currentProgress == steps.Count - 1)
            {
                base.Commit();
            }
        }

        public void Forward()
        {
            if (currentProgress + 1 < steps.Count)
            {
                currentProgress++;
                steps[currentProgress].Start();
            }
        }

        public override bool IsSendingData()
        {
            return steps[currentProgress].IsSendingData();
        }

        public override bool IsFilledOut()
        {
            return steps[currentProgress].IsFilledOut();
        }

        public override bool GetSortingTaskData(out List<SortingTaskData> sortingTaskDataList)
        {
            sortingTaskDataList = null;
            foreach (var surveyStep in steps)
            {
                if (!surveyStep.IsFinished)
                {
                    break;
                }

                if (!surveyStep.GetSortingTaskData(out var taskDataList))
                {
                    continue;
                }

                if (sortingTaskDataList == null)
                {
                    sortingTaskDataList = new List<SortingTaskData>();
                }

                sortingTaskDataList.AddRange(taskDataList);
            }

            return sortingTaskDataList != null;
        }

        public override void DrawContent()
        {
            steps[currentProgress].DrawContent();
        }

        public override List<string> CollectFilePathsToCopy()
        {
            List<string> collectedDataPathList = null;

            for (var i = 0; i < CurrentProgress; i++)
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

        public override int GetProgress(out int totalProgress)
        {
            var tempCurrentProgress = 0;
            totalProgress = 0;

            foreach (var surveyStep in steps)
            {
                tempCurrentProgress += surveyStep.GetProgress(out var surveyStepTotalProgress);
                totalProgress += surveyStepTotalProgress;
            }

            if (!IsStarted)
            {
                return 0;
            }

            if (IsFinished)
            {
                return totalProgress;
            }

            return tempCurrentProgress;
        }
    }
}