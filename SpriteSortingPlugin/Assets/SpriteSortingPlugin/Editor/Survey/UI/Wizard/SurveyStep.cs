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
using System.Text;
using SpriteSortingPlugin.Survey.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public abstract class SurveyStep
    {
        protected SurveyStepData surveyStepData;
        protected string name;

        public string Name => name;
        public bool IsFinished => surveyStepData.isFinished;
        public bool IsStarted => surveyStepData.isStarted;

        public SurveyStep(string name)
        {
            this.name = name;
            surveyStepData = new SurveyStepData();
        }

        public virtual void Start()
        {
            surveyStepData.isStarted = true;
            surveyStepData.isFinished = false;
            GeneralData.isLoggingActive = false;
        }

        public virtual void Commit()
        {
            surveyStepData.isFinished = true;
        }

        public abstract void DrawContent();

        public virtual void CleanUp()
        {
        }

        public virtual bool IsSendingData()
        {
            return false;
        }

        public virtual bool IsFilledOut()
        {
            return true;
        }

        public virtual List<string> CollectFilePathsToCopy()
        {
            return null;
        }

        public virtual bool GetSortingTaskData(out List<SortingTaskData> sortingTaskDataList)
        {
            if (surveyStepData is SurveyStepSortingData surveyStepSortingData)
            {
                sortingTaskDataList = surveyStepSortingData.sortingTaskDataList;
                return true;
            }

            sortingTaskDataList = null;
            return false;
        }

        public abstract int GetProgress(out int totalProgress);

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("SurveyStep[").Append(name).Append(", ").Append(nameof(surveyStepData.isStarted)).Append(":")
                .Append(surveyStepData.isStarted).Append(", ").Append(nameof(surveyStepData.isFinished)).Append(":")
                .Append(surveyStepData.isFinished).Append("]");

            return builder.ToString();
        }
    }
}