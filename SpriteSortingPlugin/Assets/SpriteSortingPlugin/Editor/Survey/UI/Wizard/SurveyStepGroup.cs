using System.Collections.Generic;

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
                if (!isStarted)
                {
                    return 0;
                }

                return currentProgress + 1;
            }
        }

        public int OverallProgress => steps?.Count ?? 0;

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
                var isGroupSucceeded = true;
                var isGroupSkipped = true;

                foreach (var currentStep in steps)
                {
                    isGroupSucceeded &= currentStep.FinishState == SurveyFinishState.Succeeded;
                    isGroupSkipped &= currentStep.FinishState == SurveyFinishState.Skipped;
                }

                if (isGroupSucceeded)
                {
                    Finish(SurveyFinishState.Succeeded);
                }
                else if (isGroupSkipped)
                {
                    Finish(SurveyFinishState.Skipped);
                }
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
                finishState = SurveyFinishState.None;
                isFinished = false;
            }
            else if (currentProgress == 0)
            {
                finishState = SurveyFinishState.None;
                isFinished = false;
                isStarted = false;
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