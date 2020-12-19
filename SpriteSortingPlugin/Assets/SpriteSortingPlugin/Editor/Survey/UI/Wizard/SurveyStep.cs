using System.Collections.Generic;
using System.Text;
using SpriteSortingPlugin.Survey.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public abstract class SurveyStep
    {
        protected SurveyStepData surveyStepData;
        protected string jsonData;
        protected string name;

        public SurveyFinishState FinishState => surveyStepData.finishState;
        public string JsonData => jsonData;
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
            surveyStepData.finishState = SurveyFinishState.None;
            surveyStepData.isStarted = true;
            surveyStepData.isFinished = false;
        }

        protected void Finish(SurveyFinishState finishState)
        {
            surveyStepData.finishState = finishState;
            surveyStepData.isFinished = true;
        }

        public virtual void Commit()
        {
            surveyStepData.isFinished = true;
        }

        public virtual void Rollback()
        {
            surveyStepData.isStarted = false;
            surveyStepData.finishState = SurveyFinishState.None;
        }

        public virtual SurveyStepData GetSurveyStepData()
        {
            return surveyStepData;
        }

        public abstract void DrawContent();

        public virtual void CleanUp()
        {
        }

        public virtual bool IsSendingData()
        {
            return false;
        }

        public virtual List<string> CollectFilePathsToCopy()
        {
            return null;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("SurveyStep[").Append(name).Append(", ").Append(nameof(surveyStepData.isStarted)).Append(":")
                .Append(surveyStepData.isStarted).Append(", ").Append(nameof(surveyStepData.isFinished)).Append(":")
                .Append(surveyStepData.isFinished).Append(", ")
                .Append(nameof(surveyStepData.finishState)).Append(":").Append(surveyStepData.finishState).Append("]");

            return builder.ToString();
        }
    }
}