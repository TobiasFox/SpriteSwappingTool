using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStep2 : SurveyStep
    {
        private const int QuestionCounterStart = 3;

        private static readonly string[] SceneNames = new string[]
        {
            "PluginSortingExample3.unity",
            "SortingSuggestionExample2.unity"
        };

        private static readonly string[] QuestionLabels = new string[]
        {
            ". Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.FullDetectorName + ".\n" +
            "Please solve the task as quickly as possible. However, the result should make visual sense to you.",
            ". Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.FullDetectorName +
            " with the sorting suggestion functionality.\n" +
            "Please solve the task as quickly as possible. However, the result should make visual sense to you."
        };

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private bool isDescriptionVisible;
        private int questionCounter;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public SortingSuggestionStep2(string name) : base(name)
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

        public override bool IsSendingData()
        {
            return true;
        }

        public override List<string> CollectFilePathsToCopy()
        {
            return new List<string>()
            {
                SurveyStepSortingData.sortingTaskDataList[0].FullModifiedScenePath,
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
            questionCounter = QuestionCounterStart;
            GeneralData.isAutomaticSortingActive = true;

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(
                "This time the two-step process has a SpriteRenderer setup with slightly more SpriteRenderers.",
                Styling.LabelWrapStyle);
            isDescriptionVisible = EditorGUILayout.Foldout(isDescriptionVisible,
                "Information about the sorting order suggestion functionality", true);

            if (isDescriptionVisible)
            {
                EditorGUILayout.LabelField(
                    "After SpriteRenderers are being identified by the " + GeneralData.FullDetectorName +
                    " you can optionally use the automatic sorting functionality to generate a sorting suggestion of these SpriteRenderers.",
                    Styling.LabelWrapStyle);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(
                    "The suggestion is based on several criteria which you can select and modify. More information about each criterion is displayed when hovering over one.",
                    Styling.LabelWrapStyle);


                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(
                    "Some criteria need a Sprite Data asset. If you have not created such an asset yet, you might need to create one.",
                    Styling.LabelWrapStyle);
            }


            EditorGUILayout.Space(20);

            for (var i = 0; i < SurveyStepSortingData.sortingTaskDataList.Count; i++)
            {
                var currentTaskData = SurveyStepSortingData.sortingTaskDataList[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                    EditorGUILayout.LabelField($"{questionCounter}{QuestionLabels[i]}",
                        taskLabelStyle);

                    if (i == 1)
                    {
                        EditorGUILayout.Space(5);

                        EditorGUILayout.LabelField("The same SpriteRenderer setup as in 3. is used.",
                            Styling.LabelWrapStyle);
                    }

                    EditorGUILayout.Space(10);

                    var buttonLabel = $"{questionCounter}a Start by opening and focussing scene";
                    var isDisable = currentTaskData.taskState != TaskState.NotStarted;

                    if (i == 1)
                    {
                        var firstSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];
                        isDisable |= firstSortingTaskData.taskState != TaskState.Finished;
                    }

                    using (new EditorGUI.DisabledScope(isDisable))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                            if (GUILayout.Button(buttonLabel, GUILayout.Height(TaskButtonHeight)))
                            {
                                currentTaskData.StartTask();
                                currentTaskData.LoadedScene = EditorSceneManager.OpenScene(
                                    currentTaskData.FullScenePathAndName,
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

                    using (new EditorGUI.DisabledScope(currentTaskData.taskState != TaskState.Started))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                            if (GUILayout.Button($"{questionCounter}b Finish", GUILayout.Height(TaskButtonHeight)))
                            {
                                currentTaskData.FinishTask();

                                var savePath = currentTaskData.FullModifiedScenePath;
                                EditorSceneManager.SaveScene(currentTaskData.LoadedScene, savePath, true);
                            }

                            GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                        }
                    }
                }

                EditorGUILayout.Space(20);
                questionCounter++;
            }

            EditorGUI.indentLevel--;
        }
    }
}