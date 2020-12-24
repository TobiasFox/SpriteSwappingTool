﻿using System.Collections.Generic;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStep1 : SurveyStep
    {
        private const int QuestionCounterStart = 1;

        private static readonly string[] SceneNames = new string[]
        {
            "",
            "SortingSuggestionExample1.unity"
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

        private int questionCounter;
        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public SortingSuggestionStep1(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();
            questionCounter = QuestionCounterStart;
            foreach (var sceneName in SceneNames)
            {
                var sortingTaskData = new SortingTaskData();
                sortingTaskData.SetSceneName(sceneName);
                sortingTaskData.question = questionCounter.ToString();
                SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);

                questionCounter++;
            }
        }

        public override void Start()
        {
            base.Start();
            GeneralData.isLoggingActive = true;
            GeneralData.questionNumberForLogging = questionCounter + 1;
        }

        public override List<string> CollectFilePathsToCopy()
        {
            if (!IsFinished)
            {
                return null;
            }

            return new List<string>()
            {
                SurveyStepSortingData.sortingTaskDataList[1].FullModifiedScenePath
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

            //neutral formulation
            EditorGUILayout.LabelField(
                "The " + GeneralData.FullDetectorName +
                " can also generate sorting order suggestions after SpriteRenderers are being identified.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "These suggestions are based on several criteria which you can select and modify. You can find more information about each criterion directly in the " +
                GeneralData.FullDetectorName,
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
                    EditorGUILayout.LabelField($"{questionCounter}{QuestionLabels[i]}",
                        taskLabelStyle);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Please do not modify the positions of the SpriteRenderers.",
                        Styling.LabelWrapStyle);
                    if (i == 1)
                    {
                        EditorGUILayout.LabelField("The same SpriteRenderer setup as in 1. is used.",
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