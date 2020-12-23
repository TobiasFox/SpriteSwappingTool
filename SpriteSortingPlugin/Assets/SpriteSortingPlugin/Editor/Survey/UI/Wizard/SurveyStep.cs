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