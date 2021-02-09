﻿#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System.Collections.Generic;
using System.IO;
using SpriteSortingPlugin.SpriteSorting.Logging;
using SpriteSortingPlugin.SpriteSorting.UI;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStep2 : SurveyStep
    {
        private const int QuestionCounterStart = 2;

        private static readonly string[] SceneNames = new string[]
        {
            "SortingSuggestionExample_02.unity",
            ""
        };

        private static readonly string[] QuestionLabels = new string[]
        {
            $". Please find and solve all visual glitches in the given scene by using the {GeneralData.FullDetectorName} with the sorting suggestion functionality."
            // + $"Please solve the task as quickly as possible. However, the result should make visual sense to you."
            ,
            $". Please solve all visual glitches in the given scene by using the {GeneralData.FullDetectorName} with the sorting suggestion functionality.\n" +
            $"Please solve the task as quickly as possible. However, the result should make visual sense to you."
        };

        private static readonly float TaskButtonHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private int questionCounter;
        private HowToDescription howToDescription;
        private AutoSortingHowToDescription autoSortingHowToDescription;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public SortingSuggestionStep2(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            questionCounter = QuestionCounterStart;
            foreach (var sceneName in SceneNames)
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    continue;
                }

                var sortingTaskData = new SortingTaskData();
                sortingTaskData.SetSceneName(sceneName);
                sortingTaskData.question = questionCounter.ToString();
                SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);

                questionCounter++;
            }

            howToDescription = new HowToDescription() {isBoldHeader = false};
            autoSortingHowToDescription = new AutoSortingHowToDescription() {isBoldHeader = false};
        }

        public override bool IsSendingData()
        {
            return true;
        }

        public override void Start()
        {
            base.Start();
            GeneralData.isLoggingActive = true;
            GeneralData.questionNumberForLogging = QuestionCounterStart;

            var loggingData = LoggingManager.GetInstance().loggingData;
            loggingData.isFirstSortingQuestion = false;
            loggingData.CurrentFoundGlitchStatistic.question = GeneralData.questionNumberForLogging;
        }

        public override List<string> CollectFilePathsToCopy()
        {
            if (!IsFinished)
            {
                return null;
            }

            var collectFileList = new List<string>();

            foreach (var sortingTaskData in SurveyStepSortingData.sortingTaskDataList)
            {
                var fullModifiedScenePath = sortingTaskData.FullModifiedScenePath;
                if (!File.Exists(fullModifiedScenePath))
                {
                    continue;
                }

                collectFileList.Add(fullModifiedScenePath);
            }

            return collectFileList.Count == 0 ? null : collectFileList;
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

            EditorGUILayout.LabelField("Sorting order suggestions", Styling.LabelWrapStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("The setup of this task has slightly more Sprites.", Styling.LabelWrapStyle);

            autoSortingHowToDescription.DrawHowTo();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                $"Please open the {GeneralData.DetectorName} and use this functionality located at the bottom of the {GeneralData.DetectorName}'s window.",
                Styling.LabelWrapStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                if (GUILayout.Button($"Open {GeneralData.DetectorName}\nwith enabled sorting order suggestion",
                    GUILayout.Width(224)))
                {
                    var detector = EditorWindow.GetWindow<SpriteSwappingDetector>();
                    detector.Show();
                    detector.ActivateAutoSorting();
                }
            }

            howToDescription.DrawHowTo();
            EditorGUILayout.Space(5);
            howToDescription.HowToGenerateSpriteData();

            EditorGUILayout.Space(20);

            for (var i = 0; i < SurveyStepSortingData.sortingTaskDataList.Count; i++)
            {
                var currentTaskData = SurveyStepSortingData.sortingTaskDataList[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                    EditorGUILayout.LabelField($"{questionCounter}{QuestionLabels[i]}",
                        taskLabelStyle);

                    EditorGUILayout.Space(10);

                    EditorGUILayout.LabelField("The result should make visual sense to you.", Styling.LabelWrapStyle);

                    EditorGUILayout.LabelField("Please do not modify the positions of the SpriteRenderers.",
                        Styling.LabelWrapStyle);
                    EditorGUILayout.LabelField(
                        new GUIContent("The time needed will be measured.",
                            "It starts when pressing the \"Start\" button and ends, when pressing the \"Finish\" button"),
                        Styling.LabelWrapStyle);

                    EditorGUILayout.LabelField(
                        "You might need to create a " + nameof(SpriteData) + " asset.",
                        Styling.LabelWrapStyle);

                    EditorGUILayout.Space(10);

                    var buttonLabel = "Start and Open scene";
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

                    EditorGUILayout.Space(20);

                    using (new EditorGUI.DisabledScope(currentTaskData.taskState != TaskState.Started))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(EditorGUIUtility.singleLineHeight * EditorGUI.indentLevel);
                            if (GUILayout.Button("Finish and Save", GUILayout.Height(TaskButtonHeight)))
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