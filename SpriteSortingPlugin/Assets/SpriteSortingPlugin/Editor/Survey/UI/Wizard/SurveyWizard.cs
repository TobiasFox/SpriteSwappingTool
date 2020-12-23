using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SurveyWizard
    {
        private List<SurveyStep> steps;
        private List<SurveyStepGroup> surveyStepGroups;
        private int currentProgress;
        private int currentSurveyStepIndex;
        private int totalProgress;

        public int TotalProgress => totalProgress;

        public int CurrentProgress => currentProgress;

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

        public SurveyStep GetCurrent()
        {
            if (steps == null || currentSurveyStepIndex < 0 || currentSurveyStepIndex >= steps.Count)
            {
                return null;
            }

            return steps[currentSurveyStepIndex];
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
            totalProgress = 0;
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
                    totalProgress += surveyStepGroup.TotalProgress;
                    surveyStepGroups.Add(surveyStepGroup);
                }
                else
                {
                    totalProgress++;
                }
            }

            if (totalProgress > 0)
            {
                totalProgress--;
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

        public List<SortingTaskData> GetSortingTaskDataList()
        {
            var sortingTaskDataList = new List<SortingTaskData>();

            var partCounter = 1;
            foreach (var surveyStep in steps)
            {
                if (!surveyStep.IsFinished)
                {
                    break;
                }

                var isStepGroup = surveyStep is SurveyStepGroup;

                if (!surveyStep.GetSortingTaskData(out var taskDataList))
                {
                    if (isStepGroup)
                    {
                        partCounter++;
                    }

                    continue;
                }

                if (isStepGroup)
                {
                    foreach (var sortingTaskData in taskDataList)
                    {
                        sortingTaskData.surveyPart = partCounter;
                    }

                    partCounter++;
                }

                sortingTaskDataList.AddRange(taskDataList);
            }

            return sortingTaskDataList;
        }

        public List<string> CollectFilePathsToCopy()
        {
            List<string> collectedDataPathList = null;

            for (var i = 0; i < currentSurveyStepIndex; i++)
            {
                var surveyStep = steps[i];
                var collectedPaths = surveyStep.CollectFilePathsToCopy();

                if (collectedPaths == null || collectedPaths.Count <= 0)
                {
                    continue;
                }

                if (collectedDataPathList == null)
                {
                    collectedDataPathList = new List<string>();
                }

                collectedDataPathList.AddRange(collectedPaths);
            }

            return collectedDataPathList;
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
                    break;
                }

                if (surveyStep is SurveyStepGroup surveyStepGroup)
                {
                    currentProgress += surveyStepGroup.CurrentProgress;
                }
                else if (surveyStep.IsFinished)
                {
                    currentProgress++;
                }
            }
        }
    }
}