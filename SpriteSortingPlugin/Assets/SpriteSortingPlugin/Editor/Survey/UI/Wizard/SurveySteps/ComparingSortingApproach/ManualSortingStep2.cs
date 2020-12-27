using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class ManualSortingStep2 : SurveyStep
    {
        private const string SceneName = "ManualSorting2.unity";
        private const int QuestionNumber = 2;

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private bool isDescriptionVisible;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public ManualSortingStep2(string name) : base(name)
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
            
            EditorGUILayout.LabelField("Manual approach", Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("The setup of this task has slightly more Sprites.",
                Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);

            isDescriptionVisible = EditorGUILayout.Foldout(isDescriptionVisible,
                "Visual glitch description", true);

            if (isDescriptionVisible)
            {
                var visualGlitchDescription =
                    "Depending on the position of a camera, visual glitches happen, when SpriteRenderers overlap and have identical sorting options (Sorting Layer and Sorting Order).";
                EditorGUILayout.LabelField(visualGlitchDescription, Styling.LabelWrapStyle);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "To detect such glitches, move the Unity SceneCamera in 3D perspective mode and watch out for Sprite swaps. To solve a glitch, change the sorting options.",
                    Styling.LabelWrapStyle);
            }

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    $"{QuestionNumber}. Please find and solve all visual glitches in the given scene by using the manual approach.\n" +
                    "Please solve the task as quickly as possible. However, the result should make visual sense to you.",
                    taskLabelStyle);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Please do not modify the positions of the SpriteRenderers.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.LabelField(
                    "Please do not start the play mode. Instead, use the editor mode and move the SceneCamera.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.LabelField(
                    new GUIContent("The time needed will be measured.",
                        "It starts when pressing the \"Start\" button and ends, when pressing the \"Finish\" button"),
                    Styling.LabelWrapStyle);

                var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];

                var buttonLabel = "Start and open Scene";
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
                                currentSortingTaskData.FullScenePathAndName, OpenSceneMode.Single);

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

                EditorGUILayout.Space(20);

                using (new EditorGUI.DisabledScope(currentSortingTaskData.taskState != TaskState.Started))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                        if (GUILayout.Button("Finish", GUILayout.Height(TaskButtonHeight)))
                        {
                            currentSortingTaskData.FinishTask();

                            var combinedPath = currentSortingTaskData.FullModifiedScenePath;
                            EditorSceneManager.SaveScene(currentSortingTaskData.LoadedScene, combinedPath, true);
                        }

                        GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}