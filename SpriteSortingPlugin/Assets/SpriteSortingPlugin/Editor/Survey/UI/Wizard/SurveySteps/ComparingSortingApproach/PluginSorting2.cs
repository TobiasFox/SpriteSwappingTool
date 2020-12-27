using System.Collections.Generic;
using SpriteSortingPlugin.SpriteSorting.UI;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class PluginSorting2 : SurveyStep
    {
        private const string SceneName = "PluginSortingExample2.unity";
        private const int QuestionNumber = 4;

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private bool isDescriptionVisible;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public PluginSorting2(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            var sortingTaskData = new SortingTaskData();
            sortingTaskData.SetSceneName(SceneName);
            sortingTaskData.question = QuestionNumber.ToString();
            SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
        }

        public override bool IsSendingData()
        {
            return true;
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

            EditorGUILayout.LabelField("Plugin approach", Styling.LabelWrapStyle);
            GUILayout.Space(5);

            EditorGUILayout.LabelField("The setup of this task has slightly more Sprites.", Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Please, open the " + GeneralData.FullDetectorName,
                Styling.LabelWrapStyle);
            var openDetectorContent = new GUIContent(GeneralData.UnityMenuMainCategory + " -> " + GeneralData.Name +
                                                     " -> " + GeneralData.DetectorName);
            EditorGUILayout.LabelField(openDetectorContent, Styling.LabelWrapStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                var width = Styling.ButtonStyle.CalcSize(openDetectorContent).x;
                if (GUILayout.Button("Open " + GeneralData.DetectorName, GUILayout.Width(width)))
                {
                    var detector = EditorWindow.GetWindow<SpriteRendererSwappingDetectorEditorWindow>();
                    detector.Show();
                }
            }

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    $"{QuestionNumber}. Please find and solve all visual glitches in the given scene by using the {GeneralData.FullDetectorName}.\n" +
                    "Please solve the task as quickly as possible. However, the result should make visual sense to you.",
                    taskLabelStyle);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Please do not modify the positions of the SpriteRenderers.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.LabelField(
                    new GUIContent("The time needed will be measured.",
                        "It starts when pressing the \"Start\" button and ends, when pressing the \"Finish\" button"),
                    Styling.LabelWrapStyle);

                EditorGUILayout.LabelField(
                    "Optionally: Generate a SpriteData asset for a more accurate Sprite outline.",
                    Styling.LabelWrapStyle);

                EditorGUILayout.Space(10);

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