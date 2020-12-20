using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStep1 : SurveyStep
    {
        private static readonly string[] SceneNames = new string[]
        {
            "",
            "SortingSuggestionExample1.unity"
        };

        private static readonly string[] QuestionLabels = new string[]
        {
            "1. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName + ".",
            "2. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName +
            " with the sorting suggestion functionality."
        };

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public SortingSuggestionStep1(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            foreach (var sceneName in SceneNames)
            {
                var sortingTaskData = new SortingTaskData();
                sortingTaskData.SetSceneName(sceneName);
                SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
            }
        }

        public override void Commit()
        {
            base.Commit();

            var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[1];

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
                SurveyStepSortingData.sortingTaskDataList[1].FullModifiedScenePath
            };
        }

        public override void DrawContent()
        {
            GeneralData.isAutomaticSortingActive = true;

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Okay, very good!", Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Now, it is time to use another functionality of the plugin.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "It is a functionality, which generates sorting order suggestions after SpriteRenderers are being identified by the " +
                GeneralData.Name + " " + GeneralData.DetectorName + ".",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "These suggestions are based on several criteria which you can select and modify. More information about each criterion is displayed when hovering over one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "Some criteria need a Sprite Data asset. If you have not created such an asset yet, you might need to create one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(25);
            EditorGUILayout.LabelField(
                "A two-step process is used for the evaluation of this functionality. However, 1. is already done, because the data from the last step can be reused :)",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            for (var i = 0; i < SurveyStepSortingData.sortingTaskDataList.Count; i++)
            {
                var currentTaskData = SurveyStepSortingData.sortingTaskDataList[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUI.DisabledScope(i == 0))
                    {
                        var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                        EditorGUILayout.LabelField(QuestionLabels[i],
                            taskLabelStyle);

                        switch (i)
                        {
                            case 0:
                                EditorGUILayout.LabelField(
                                    "This is already done, because the data of the last step can be reused :)",
                                    Styling.LabelWrapStyle);
                                break;
                            case 1:
                                EditorGUILayout.Space(5);

                                EditorGUILayout.LabelField("The same SpriteRenderer setup as in 1. is used.",
                                    Styling.LabelWrapStyle);
                                break;
                        }

                        EditorGUILayout.Space(10);

                        var buttonLabel = (currentTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                        if (GUILayout.Button(buttonLabel))
                        {
                            currentTaskData.StartTask();

                            currentTaskData.LoadedScene =
                                EditorSceneManager.OpenScene(currentTaskData.FullScenePathAndName,
                                    OpenSceneMode.Single);
                        }

                        EditorGUILayout.Space(10);

                        using (new EditorGUI.DisabledScope(!currentTaskData.isTaskStarted))
                        {
                            if (GUILayout.Button("Finish"))
                            {
                                currentTaskData.FinishTask();

                                var savePath = currentTaskData.FullModifiedScenePath;
                                EditorSceneManager.SaveScene(currentTaskData.LoadedScene, savePath, true);
                            }
                        }
                    }
                }

                EditorGUILayout.Space(20);
            }

            EditorGUI.indentLevel--;
        }
    }
}