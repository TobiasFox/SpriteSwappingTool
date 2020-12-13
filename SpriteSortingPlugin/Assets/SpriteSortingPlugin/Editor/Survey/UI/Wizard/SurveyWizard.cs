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

        public void Backward()
        {
            if (currentSurveyStepIndex < 0 || currentSurveyStepIndex >= steps.Count)
            {
                return;
            }

            var surveyStep = steps[currentSurveyStepIndex];
            surveyStep.Rollback();

            if (surveyStep is SurveyStepGroup surveyStepGroup && surveyStepGroup.HasPreviousStep())
            {
                surveyStepGroup.Backward();
            }
            else
            {
                if (currentSurveyStepIndex > 0)
                {
                    currentSurveyStepIndex--;
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

        public bool HasNextStep()
        {
            if (steps == null)
            {
                return false;
            }

            var currentStep = steps[currentSurveyStepIndex];

            if (currentStep is SurveyStepGroup surveyStepGroup)
            {
                if (!surveyStepGroup.IsFinished)
                {
                    return true;
                }
            }

            return currentSurveyStepIndex + 1 < steps.Count;
        }

        public bool HasPreviousStep()
        {
            if (steps == null)
            {
                return false;
            }

            var currentStep = steps[currentSurveyStepIndex];

            if (currentStep is SurveyStepGroup surveyStepGroup)
            {
                if (surveyStepGroup.HasPreviousStep())
                {
                    return true;
                }
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
            this.steps[0].Start();

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
            currentProgress = 0;

            if (steps == null || steps.Count == 0)
            {
                return;
            }

            foreach (var surveyStep in steps)
            {
                if (!surveyStep.IsStarted)
                {
                    continue;
                }

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