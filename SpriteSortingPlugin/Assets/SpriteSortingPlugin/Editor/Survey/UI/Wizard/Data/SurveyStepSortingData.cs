using System;
using System.Collections.Generic;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class SurveyStepSortingData : SurveyStepData
    {
        public List<SortingTaskData> sortingTaskDataList;

        public SurveyStepSortingData()
        {
            sortingTaskDataList = new List<SortingTaskData>();
        }
    }
}