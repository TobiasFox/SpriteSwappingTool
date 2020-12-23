using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class PluginSorting1 : SurveyStep
    {
        private const string SceneName = "PluginSortingExample1.unity";
        private const int QuestionNumber = 3;

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public PluginSorting1(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            var sortingTaskData = new SortingTaskData();
            sortingTaskData.SetSceneName(SceneName);
            sortingTaskData.question = QuestionNumber.ToString();
            SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
        }

        public override List<string> CollectFilePathsToCopy()
        {
            if (!IsFinished)
            {
                return null;
            }

            return new List<string>()
            {
                SurveyStepSortingData.sortingTaskDataList[0].FullModifiedScenePath
            };
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = SurveyStepSortingData.sortingTaskDataList.Count * 2;

            if (!IsStarted)
            {
                return 0;
            }

            if (IsFinished)
            {
                return totalProgress;
            }

            var currentProgress = 0;

            foreach (var currentSortingTaskData in SurveyStepSortingData.sortingTaskDataList)
            {
                switch (currentSortingTaskData.taskState)
                {
                    case TaskState.Started:
                        currentProgress++;
                        break;
                    case TaskState.Finished:
                        currentProgress += 2;
                        break;
                }
            }

            return currentProgress;
        }

        public override bool IsFilledOut()
        {
            var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];

            if (currentSortingTaskData.taskState == TaskState.NotStarted)
            {
                return false;
            }

            return currentSortingTaskData.timeNeeded >= 0;
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("After the manual approach is used, the usage with the plugin is evaluated.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "The " + GeneralData.FullDetectorName +
                " automatically identifies overlapping and unsorted SpritesRenderers and helps to sort them.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "You can find the " + GeneralData.FullDetectorName + " here:\n" +
                GeneralData.UnityMenuMainCategory + " -> " + GeneralData.Name + " -> " + GeneralData.DetectorName,
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    $"{QuestionNumber}. Please find and solve all visual glitches in the given scene by using the " +
                    GeneralData.FullDetectorName + ".\n" +
                    "Please solve the task as quickly as possible. However, the result should make visual sense to you.",
                    taskLabelStyle);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(
                    "You can optionally generate more accurate Sprite outlines by using a " + nameof(SpriteData) +
                    ". Such an asset can be created with the " + GeneralData.FullDataAnalysisName + " window.",
                    Styling.LabelWrapStyle);

                EditorGUILayout.Space(10);

                var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];

                var buttonLabel = $"{QuestionNumber}a Start by opening and focussing scene";
                var isDisable = currentSortingTaskData.taskState != TaskState.NotStarted;
                using (new EditorGUI.DisabledScope(isDisable))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                        if (GUILayout.Button(buttonLabel, GUILayout.Height(TaskButtonHeight)))
                        {
                            currentSortingTaskData.StartTask();
                            currentSortingTaskData.LoadedScene = EditorSceneManager.OpenScene(
                                currentSortingTaskData.FullScenePathAndName,
                                OpenSceneMode.Single);

                            EditorWindow.FocusWindowIfItsOpen<SceneView>();

                            var setupGameObject = GameObject.Find("setup");
                            if (setupGameObject != null)
                            {
                                Selection.objects = new Object[] {setupGameObject};
                                SceneView.FrameLastActiveSceneView();
                                EditorGUIUtility.PingObject(setupGameObject);
                            }
                        }

                        GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                    }
                }

                EditorGUILayout.Space(10);
                var wrapCenterStyle = new GUIStyle(Styling.LabelWrapStyle) {alignment = TextAnchor.MiddleCenter};
                EditorGUILayout.LabelField("Time will be measured.", wrapCenterStyle);
                EditorGUILayout.LabelField(
                    "It starts when clicking the button above and ends when clicking the finish button.",
                    wrapCenterStyle);
                EditorGUILayout.Space(10);

                using (new EditorGUI.DisabledScope(currentSortingTaskData.taskState != TaskState.Started))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                        if (GUILayout.Button($"{QuestionNumber}b Finish", GUILayout.Height(TaskButtonHeight)))
                        {
                            currentSortingTaskData.FinishTask();

                            var savePath = currentSortingTaskData.FullModifiedScenePath;
                            EditorSceneManager.SaveScene(currentSortingTaskData.LoadedScene, savePath, true);
                        }

                        GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}