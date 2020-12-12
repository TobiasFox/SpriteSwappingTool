using System.Collections.Generic;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyWizard
    {
        private List<SurveyStep> steps;
        private List<SurveyStepGroup> surveyStepGroups;
        private int currentProgress;
        private int currentSurveyStepIndex;
        private int overallProgress;

        public int OverallProgress => overallProgress;

        public int CurrentProgress
        {
            get { return currentProgress; }
        }

        public void Forward()
        {
            if (currentSurveyStepIndex < 0 || currentSurveyStepIndex >= steps.Count)
            {
                return;
            }

            var surveyStep = steps[currentSurveyStepIndex];
            surveyStep.Commit();

            if (!(surveyStep is SurveyStepGroup) && currentSurveyStepIndex < steps.Count)
            {
                currentSurveyStepIndex++;
            }

            UpdateCurrentProgress();
        }

        public void Backward()
        {
            if (currentSurveyStepIndex < 0 || currentSurveyStepIndex >= steps.Count)
            {
                return;
            }

            var surveyStep = steps[currentSurveyStepIndex];
            surveyStep.Rollback();

            if (!(surveyStep is SurveyStepGroup) && currentSurveyStepIndex < steps.Count)
            {
                currentSurveyStepIndex--;
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

        public bool HasNextStep()
        {
            if (steps == null)
            {
                return false;
            }

            return currentSurveyStepIndex + 1 < steps.Count;
        }

        public bool HasPreviousStep()
        {
            if (steps == null)
            {
                return false;
            }

            return currentSurveyStepIndex > 0;
        }

        public void SetSurveySteps(List<SurveyStep> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                return;
            }

            this.steps = steps;

            currentSurveyStepIndex = 0;
            overallProgress = 0;
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
                    overallProgress += surveyStepGroup.OverallProgress;
                    surveyStepGroups.Add(surveyStepGroup);
                }
                else
                {
                    overallProgress++;
                }
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

        private void UpdateCurrentProgress()
        {
            if (steps == null || steps.Count == 0)
            {
                currentProgress = 0;
                return;
            }

            currentProgress = 1;
            for (var i = 0; i < currentSurveyStepIndex; i++)
            {
                var surveyStep = steps[currentSurveyStepIndex];

                if (surveyStep is SurveyStepGroup surveyStepGroup)
                {
                    currentProgress += surveyStepGroup.CurrentProgress;
                }
                else
                {
                    currentProgress++;
                }
            }
        }
    }
}