using System.Collections.Generic;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
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

                return currentProgress + 1;
            }
        }

        public int TotalProgress => steps?.Count ?? 0;

        private SurveyStepGroupData SurveyStepGroupData => (SurveyStepGroupData) surveyStepData;

        public SurveyStepGroup(List<SurveyStep> surveySteps, string groupName) : base(groupName)
        {
            steps = surveySteps;
            surveyStepData = new SurveyStepGroupData();
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

                var isGroupSucceeded = true;

                foreach (var currentStep in steps)
                {
                    isGroupSucceeded &= currentStep.FinishState == SurveyFinishState.Succeeded;
                }

                Finish(isGroupSucceeded ? SurveyFinishState.Succeeded : SurveyFinishState.Skipped);
            }
        }

        public override void Rollback()
        {
            if (currentProgress < 0 || currentProgress >= steps.Count)
            {
                return;
            }

            steps[currentProgress].Rollback();

            if (currentProgress == steps.Count - 1)
            {
                surveyStepData.finishState = SurveyFinishState.None;
                surveyStepData.isFinished = false;
            }
            else if (currentProgress == 0)
            {
                surveyStepData.finishState = SurveyFinishState.None;
                surveyStepData.isFinished = false;
                surveyStepData.isStarted = false;
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

        public void Backward()
        {
            if (currentProgress > 0)
            {
                currentProgress--;
                steps[currentProgress].Start();
            }
        }

        public override SurveyStepData GetSurveyStepData()
        {
            SurveyStepGroupData.SurveyStepsData = new List<SurveyStepData>();
            foreach (var surveyStep in steps)
            {
                var stepData = surveyStep.GetSurveyStepData();
                if (!stepData.isStarted)
                {
                    break;
                }

                SurveyStepGroupData.SurveyStepsData.Add(stepData);
            }

            return SurveyStepGroupData;
        }

        public override void DrawContent()
        {
            steps[currentProgress].DrawContent();
        }

        public bool HasNextStep()
        {
            if (steps == null)
            {
                return false;
            }

            return currentProgress + 1 < steps.Count;
        }

        public bool HasPreviousStep()
        {
            if (steps == null)
            {
                return false;
            }

            return currentProgress > 0;
        }

        private void UpdateData()
        {
            //TODO modify jsonData
        }
    }
}