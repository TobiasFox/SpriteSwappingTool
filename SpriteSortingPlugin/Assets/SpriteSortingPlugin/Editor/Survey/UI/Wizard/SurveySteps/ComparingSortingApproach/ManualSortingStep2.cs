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

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private bool isDescriptionVisible;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public ManualSortingStep2(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            var sortingTaskData = new SortingTaskData();
            sortingTaskData.SetSceneName(SceneName);
            SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
        }

        public override void Commit()
        {
            base.Commit();

            Finish(SurveyFinishState.Succeeded);
        }

        public override List<string> CollectFilePathsToCopy()
        {
            if (FinishState != SurveyFinishState.Succeeded)
            {
                return null;
            }

            return new List<string>()
            {
                SurveyStepSortingData.sortingTaskDataList[0].FullModifiedScenePath
            };
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

            EditorGUILayout.LabelField("The setup of this task has slightly more Sprites.",
                Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);

            isDescriptionVisible = EditorGUILayout.Foldout(isDescriptionVisible,
                "Short Description about what causes a visual glitch", true);

            if (isDescriptionVisible)
            {
                var visualGlitchDescription =
                    "Depending on the position of the camera which renders a scene, a visual glitch happens, when SpriteRenderers overlap and have identical sorting options.";
                EditorGUILayout.LabelField(visualGlitchDescription, Styling.LabelWrapStyle);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "To detect potential SpriteRenderer the manual method can be used, by moving around the Unity SceneCamera in 3D perspective mode and watching out for Sprite swaps.",
                    Styling.LabelWrapStyle);
            }

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    "2. Please find and solve all visual glitches in the given scene by using the manual approach.\n" +
                    "Please solve these glitches so it makes visually sense for you but as fast as possible.",
                    taskLabelStyle);

                EditorGUILayout.Space();
                var largeLabel = new GUIStyle(EditorStyles.largeLabel) {wordWrap = true};
                EditorGUILayout.LabelField("Please don't start the play mode.",
                    largeLabel);
                EditorGUILayout.LabelField("Instead, use the editor mode and move the SceneCamera.",
                    largeLabel);

                var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];

                var buttonLabel = "Start by opening and focussing scene";
                var isDisable = currentSortingTaskData.taskState != TaskState.NotStarted;
                using (new EditorGUI.DisabledScope(isDisable))
                {
                    if (GUILayout.Button(buttonLabel, GUILayout.Height(TaskButtonHeight)))
                    {
                        currentSortingTaskData.StartTask();

                        //TODO open Scene and may discard everything before
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
                    if (GUILayout.Button("Finish", GUILayout.Height(TaskButtonHeight)))
                    {
                        currentSortingTaskData.FinishTask();

                        var combinedPath = currentSortingTaskData.FullModifiedScenePath;
                        EditorSceneManager.SaveScene(currentSortingTaskData.LoadedScene, combinedPath, true);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}