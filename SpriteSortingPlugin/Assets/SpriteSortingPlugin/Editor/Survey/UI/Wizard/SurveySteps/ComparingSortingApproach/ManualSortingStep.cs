using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class ManualSortingStep : SurveyStep
    {
        private const string SceneName = "ManualSorting1.unity";

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public ManualSortingStep(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            var sortingTaskData = new SortingTaskData();
            sortingTaskData.SetSceneName(SceneName);
            SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
        }

        public override void Commit()
        {
            base.Commit();

            var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];

            if (!currentSortingTaskData.isTaskStarted)
            {
                var isFinishedTask = currentSortingTaskData.timeNeeded > 0;
                Finish(isFinishedTask ? SurveyFinishState.Succeeded : SurveyFinishState.Skipped);
            }
            else if (!currentSortingTaskData.isTaskFinished)
            {
                currentSortingTaskData.CancelTask();
                Finish(SurveyFinishState.Skipped);
            }
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

            if (currentSortingTaskData.isTaskStarted)
            {
                return false;
            }

            return currentSortingTaskData.timeNeeded >= 0;
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(
                "This part compares the manual approach to the approach used by the Sprite Swapping Detector. For this comparison, short tasks are given.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Short Description about what causes a visual glitch:", Styling.LabelWrapStyle);
            var visualGlitchDescription =
                "Depending on the position of the camera which renders a scene, a visual glitch happens, when SpriteRenderers overlap and have identical sorting options.";
            EditorGUILayout.LabelField(visualGlitchDescription, Styling.LabelWrapStyle);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "To detect potential SpriteRenderer the manual method can be used, by moving around the Unity SceneCamera in 3D perspective mode and watching out for Sprite swaps.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    "1. Please find and solve all visual glitches in the given scene by using the manual approach.",
                    taskLabelStyle);

                EditorGUILayout.Space();
                var largeLabel = new GUIStyle(EditorStyles.largeLabel) {wordWrap = true};
                EditorGUILayout.LabelField("Please don't start the play mode.",
                    largeLabel);
                EditorGUILayout.LabelField("Instead, use the editor mode and move the SceneCamera.",
                    largeLabel);

                var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];
                var buttonLabel = (currentSortingTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    currentSortingTaskData.StartTask();

                    //TODO open Scene and may discard everything before
                    currentSortingTaskData.LoadedScene = EditorSceneManager.OpenScene(
                        currentSortingTaskData.FullScenePathAndName, OpenSceneMode.Single);
                }

                using (new EditorGUI.DisabledScope(!currentSortingTaskData.isTaskStarted))
                {
                    EditorGUILayout.Space(20);
                    if (GUILayout.Button("Finish"))
                    {
                        currentSortingTaskData.FinishTask();

                        var savePath = currentSortingTaskData.FullModifiedScenePath;
                        EditorSceneManager.SaveScene(currentSortingTaskData.LoadedScene, savePath, true);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}