using System.Collections.Generic;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyStepGroup : SurveyStep
    {
        private List<SurveyStep> steps;
        private int currentProgress;

        public int CurrentProgress => currentProgress;
        public int OverallProgress => steps?.Count ?? 0;

        public SurveyStepGroup(List<SurveyStep> surveySteps, string groupName) : base(groupName)
        {
            steps = surveySteps;
        }

        public override void Commit()
        {
            if (currentProgress < 0 || currentProgress >= steps.Count)
            {
                return;
            }

            steps[currentProgress].Commit();

            if (currentProgress < steps.Count)
            {
                currentProgress++;
            }
        }

        public override void Rollback()
        {
            if (currentProgress < 0 || currentProgress >= steps.Count)
            {
                return;
            }

            steps[currentProgress].Rollback();

            if (currentProgress > 0)
            {
                currentProgress--;
            }
        }

        public override void DrawContent()
        {
            steps[currentProgress].DrawContent();
        }

        public void UpdateBools()
        {
            // var tempIsCompleted = true;
            // var tempIsSkipped = true;
            // for (var i = 0; i < currentStepIndex; i++)
            // {
            //     var currentStep = steps[i];
            //     tempIsCompleted &= currentStep.IsCompleted;
            //     tempIsSkipped &= currentStep.IsSkipped;
            // }
            //
            // isCompleted = tempIsCompleted;
            // isSkipped = tempIsSkipped;
        }

        private void UpdateData()
        {
            //TODO modify jsonData
        }
    }
}