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
            GeneralData.Name + " " + GeneralData.DetectorName + ".\n" +
            "Please solve these glitches so it makes visually sense for you but as fast as possible.",
            "2. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName +
            " with the sorting suggestion functionality.\n" +
            "Please solve these glitches so it makes visually sense for you but as fast as possible."
        };

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

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
                SurveyStepSortingData.sortingTaskDataList[1].FullModifiedScenePath
            };
        }

        public override bool IsFilledOut()
        {
            foreach (var sortingTaskData in SurveyStepSortingData.sortingTaskDataList)
            {
                if (sortingTaskData.taskState == TaskState.NotStarted)
                {
                    return false;
                }

                if (sortingTaskData.timeNeeded < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override void DrawContent()
        {
            GeneralData.isAutomaticSortingActive = true;

            EditorGUI.indentLevel++;

            //neutral formulation
            // EditorGUILayout.LabelField("Now, it is time to use another functionality of the plugin.",
            //     Styling.LabelWrapStyle);

            // EditorGUILayout.Space(10);

            // EditorGUILayout.LabelField(
            //     "It is a functionality, which generates sorting order suggestions after SpriteRenderers are being identified by the " +
            //     GeneralData.Name + " " + GeneralData.DetectorName + ".",
            //     Styling.LabelWrapStyle);
            EditorGUILayout.LabelField(
                "The " + GeneralData.Name + " " + GeneralData.DetectorName +
                " have also a functionality to generate sorting order suggestions after SpriteRenderers are being identified.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "These suggestions are based on several criteria which you can select and modify. More information about each criterion is displayed when hovering over one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "Some criteria need a Sprite Data asset. If you have not created such an asset yet, you might need to create one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField(
                "A two-step process is used to evaluate this functionality.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);

            for (var i = 0; i < SurveyStepSortingData.sortingTaskDataList.Count; i++)
            {
                var currentTaskData = SurveyStepSortingData.sortingTaskDataList[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                    EditorGUILayout.LabelField(QuestionLabels[i],
                        taskLabelStyle);

                    switch (i)
                    {
                        case 0:
                            // EditorGUILayout.LabelField(
                            //     "This is already done, because the data of the last step can be reused :)",
                            //     Styling.LabelWrapStyle);
                            break;
                        case 1:
                            EditorGUILayout.Space(5);

                            EditorGUILayout.LabelField("The same SpriteRenderer setup as in 1. is used.",
                                Styling.LabelWrapStyle);
                            break;
                    }

                    EditorGUILayout.Space(10);

                    var buttonLabel = "Start by opening and focussing scene";
                    var isDisable = currentTaskData.taskState != TaskState.NotStarted;

                    if (i == 1)
                    {
                        var firstSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];
                        isDisable |= firstSortingTaskData.taskState != TaskState.Finished;
                    }

                    using (new EditorGUI.DisabledScope(isDisable))
                    {
                        if (GUILayout.Button(buttonLabel, GUILayout.Height(TaskButtonHeight)))
                        {
                            currentTaskData.StartTask();

                            currentTaskData.LoadedScene =
                                EditorSceneManager.OpenScene(currentTaskData.FullScenePathAndName,
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
                    }

                    EditorGUILayout.Space(10);
                    var wrapCenterStyle = new GUIStyle(Styling.LabelWrapStyle) {alignment = TextAnchor.MiddleCenter};
                    EditorGUILayout.LabelField("Time will be measured.", wrapCenterStyle);
                    EditorGUILayout.LabelField(
                        "It starts when clicking the button above and ends when clicking the finish button.",
                        wrapCenterStyle);
                    EditorGUILayout.Space(10);

                    using (new EditorGUI.DisabledScope(currentTaskData.taskState != TaskState.Started))
                    {
                        if (GUILayout.Button("Finish", GUILayout.Height(TaskButtonHeight)))
                        {
                            currentTaskData.FinishTask();

                            var savePath = currentTaskData.FullModifiedScenePath;
                            EditorSceneManager.SaveScene(currentTaskData.LoadedScene, savePath, true);
                        }
                    }
                }

                EditorGUILayout.Space(20);
            }

            EditorGUI.indentLevel--;
        }
    }
}