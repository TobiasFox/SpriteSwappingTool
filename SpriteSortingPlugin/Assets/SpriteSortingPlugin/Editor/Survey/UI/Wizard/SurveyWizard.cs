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
            get
            {
                var progress = 0;
                for (var i = 0; i < currentSurveyStepIndex; i++)
                {
                    var surveyStep = steps[currentSurveyStepIndex];

                    if (surveyStep is SurveyStepGroup surveyStepGroup)
                    {
                        progress += surveyStepGroup.CurrentProgress;
                    }
                    else
                    {
                        progress++;
                    }
                }

                return progress;
            }
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
        }

        public SurveyStep GetCurrent()
        {
            return steps[currentSurveyStepIndex];
        }

        public void SetSurveySteps(List<SurveyStep> steps)
        {
            this.steps = steps;

            overallProgress = 0;
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

        public List<SurveyStep> GetSurveySteps()
        {
            return steps;
        }

        public List<SurveyStepGroup> GetSurveyStepGroups()
        {
            return surveyStepGroups;
        }
    }
}