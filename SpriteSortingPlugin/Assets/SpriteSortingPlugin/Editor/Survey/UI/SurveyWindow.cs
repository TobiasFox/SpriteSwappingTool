#region license

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

using System;
using System.IO;
using System.Threading;
using SpriteSortingPlugin.SpriteSorting.Logging;
using SpriteSortingPlugin.Survey.Data;
using SpriteSortingPlugin.Survey.UI.Wizard;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI
{
    public class SurveyWindow : EditorWindow
    {
        private const float SpaceBetweenGroupProgressBars = 15;

        private static readonly string[] SurveyDataOutputPath = new string[]
        {
            "SurveyData"
        };

        [SerializeField] private SurveyData surveyData;

        private SurveyWizard surveyWizard;
        private SurveyStepGenerator surveyStepGenerator;
        private float currentProgress;
        private Vector2 contentScrollPosition = Vector2.zero;
        private float lastHeaderHeight;
        private float lastFooterHeight;
        private SurveyStep currentStep;
        private string resultDataPath;

        [MenuItem(GeneralData.UnityMenuMainCategory + "/" + GeneralData.Name + "/Survey %g", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<SurveyWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent(GeneralData.Name + " Survey");

            surveyStepGenerator = new SurveyStepGenerator();

            surveyData = new SurveyData();
            surveyWizard = new SurveyWizard();
            surveyWizard.SetSurveySteps(surveyStepGenerator.GenerateSurveySteps(surveyData));
            currentStep = surveyWizard.GetCurrent();

            GeneralData.isSurveyActive = true;
            GeneralData.isAutomaticSortingActive = false;
            GeneralData.currentSurveyId = surveyData.UserId.ToString();

            LoggingManager.GetInstance().Clear();
        }

        private void OnGUI()
        {
            var currentEventType = Event.current.type;

            using (var headerScope = new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label(GeneralData.Name + " Survey", Styling.CenteredStyleBold);

                DrawProgressBars();
                DrawHeader();

                EditorGUILayout.Space(10);

                if (currentEventType == EventType.Repaint)
                {
                    lastHeaderHeight = headerScope.rect.height;
                }
            }

            DrawSurveyStepContent();

            CheckForSendingFinalData();

            using (var footerScope = new EditorGUILayout.VerticalScope())
            {
                DrawNavigationButtons();

                EditorGUILayout.Space(20);

                DrawFooter();

                EditorGUILayout.Space(5);
                if (currentEventType == EventType.Repaint)
                {
                    lastFooterHeight = footerScope.rect.height;
                }
            }
        }

        private void CheckForSendingFinalData()
        {
            if (currentStep == null)
            {
                return;
            }

            if (!(currentStep is FinishingSurvey finishingSurvey))
            {
                return;
            }

            if (!finishingSurvey.GetAndConsumeIsSendingDataButtonPressedThisFrame())
            {
                return;
            }

            PrepareAndSendData(true);
        }

        private void DrawHeader()
        {
            if (currentStep == null)
            {
                return;
            }

            GUILayout.Label(currentStep.Name, Styling.CenteredStyle);
        }

        private void DrawProgressBars()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label("Progress", Styling.CenteredStyle);

                int tempCurrentProgress;
                var tempTotalProgress = surveyWizard.TotalProgress;
                float progressPercentage;

                if (surveyData.generalQuestionsData.developing2dGames == 1)
                {
                    tempCurrentProgress = 1;
                    tempTotalProgress = 1;
                    progressPercentage = 1;
                }
                else
                {
                    tempCurrentProgress = surveyWizard.CurrentProgress;
                    progressPercentage = tempCurrentProgress / (float) tempTotalProgress;
                }

                var overallDisplayText =
                    $"{Math.Round(progressPercentage * 100, 2)}% ({tempCurrentProgress}/{tempTotalProgress})";

                var overallProgressBarRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                    GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(overallProgressBarRect, progressPercentage, overallDisplayText);

                var surveyGroups = surveyWizard.GetSurveyStepGroups();
                if (surveyGroups == null || surveyGroups.Count <= 0)
                {
                    return;
                }

                var groupProgressBarsRect =
                    EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                        GUILayout.ExpandWidth(true));

                var summedSpace = (surveyGroups.Count - 1) * SpaceBetweenGroupProgressBars;
                groupProgressBarsRect.width = (groupProgressBarsRect.width - summedSpace) / (float) surveyGroups.Count;

                for (var i = 0; i < surveyGroups.Count; i++)
                {
                    var surveyGroup = surveyGroups[i];

                    var tempGroupCurrentProgress = surveyGroup.GetProgress(out var tempGroupTotalProgress);
                    var surveyGroupCurrentProgress = tempGroupCurrentProgress / (float) tempGroupTotalProgress;

                    var groupDisplayText = $"Part {i + 1}: {Math.Round(surveyGroupCurrentProgress * 100, 2)}%";
                    EditorGUI.ProgressBar(groupProgressBarsRect, surveyGroupCurrentProgress, groupDisplayText);
                    groupProgressBarsRect.x += groupProgressBarsRect.width + SpaceBetweenGroupProgressBars;
                }
            }
        }

        private void DrawSurveyStepContent()
        {
            if (currentStep == null)
            {
                return;
            }

            var remainingContentHeight = position.height - lastHeaderHeight - lastFooterHeight;

            using (var scrollScope =
                new EditorGUILayout.ScrollViewScope(contentScrollPosition, GUILayout.Height(remainingContentHeight)))
            {
                contentScrollPosition = scrollScope.scrollPosition;
                using (new GUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
                {
                    currentStep.DrawContent();
                }
            }
        }

        private void DrawNavigationButtons()
        {
            var isSurveyFinished = surveyWizard.CurrentProgress == surveyWizard.TotalProgress;
            if (isSurveyFinished)
            {
                return;
            }

            var isSendingData = surveyWizard.CurrentProgress == surveyWizard.TotalProgress - 1;

            if (isSendingData)
            {
                var centeredStyle = new GUIStyle(Styling.CenteredStyle) {wordWrap = true};
                EditorGUILayout.LabelField(
                    new GUIContent("Please keep this window open and make sure the PC is connected to the internet.",
                        Styling.InfoIcon),
                    centeredStyle);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var buttonStyle = new GUIStyle(Styling.ButtonStyleBold);
                var heightLayout = GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.25f);
                buttonStyle.fontSize++;
                GUILayout.Space(10);

                var buttonText = "Continue";

                if (surveyWizard.CurrentProgress == 0)
                {
                    buttonText = "Start";
                }
                else if (isSendingData)
                {
                    buttonText = "Finish and Send data";
                }

                var isDisabled = (GeneralData.IsValidatingUserInput && !currentStep.IsFilledOut());

                using (new EditorGUI.DisabledScope(isDisabled))
                {
                    if (GUILayout.Button(buttonText, buttonStyle, heightLayout))
                    {
                        NextSurveyStep();
                        if (isSendingData)
                        {
                            PrepareAndSendData(true);

                            if (currentStep is FinishingSurvey finishingSurvey)
                            {
                                finishingSurvey.SetResultDataPath(resultDataPath, "ResultData.zip");
                            }
                        }
                    }
                }
            }
        }

        private void NextSurveyStep()
        {
            var isSendingData = currentStep.IsSendingData();

            surveyWizard.Forward();

            if (isSendingData)
            {
                PrepareAndSendData();
            }

            currentStep = surveyWizard.GetCurrent();
            contentScrollPosition = Vector2.zero;

            if (currentStep is FinishingSurvey)
            {
                ResetSurveyStateInGeneralData();
            }
        }

        private void PrepareAndSendData(bool isResult = false)
        {
            surveyData.sortingTaskDataList = surveyWizard.GetSortingTaskDataList();
            surveyData.currentProgress = surveyWizard.CurrentProgress;
            surveyData.totalProgress = surveyWizard.TotalProgress;

            string directory;

            if (isResult)
            {
                directory = Path.Combine(Application.persistentDataPath, Path.Combine(SurveyDataOutputPath),
                    surveyData.ResultSaveFolder);
            }
            else
            {
                directory = Path.Combine(Application.temporaryCachePath, Path.Combine(SurveyDataOutputPath),
                    surveyData.SaveFolder);
            }

            var surveyDataJson = JsonUtility.ToJson(surveyData);
            Directory.CreateDirectory(directory);
            var pathAndName = Path.Combine(directory, (isResult ? "Result" : "") + "SurveyData.json");
            File.WriteAllText(pathAndName, surveyDataJson);

            CopyCollectedFiles(directory);
            CopyLogFiles(directory);

            SendMail(isResult, directory);

            if (isResult)
            {
                var dirInfo = new DirectoryInfo(directory);
                resultDataPath = dirInfo.Parent?.FullName;
            }
        }

        private void CopyLogFiles(string directory)
        {
            var logDirArray = new string[]
            {
                Application.temporaryCachePath, "SurveyData", surveyData.UserId.ToString(), "LogFiles"
            };
            var directoryInfo = new DirectoryInfo(Path.Combine(logDirArray));

            if (!directoryInfo.Exists)
            {
                return;
            }

            var targetFolder = Path.Combine(directory, "LogFiles");
            Directory.CreateDirectory(targetFolder);

            foreach (var file in directoryInfo.GetFiles())
            {
                try
                {
                    file.CopyTo(Path.Combine(targetFolder, file.Name), true);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void SendMail(bool isResult, string directory)
        {
            var zipSaveFolder = Directory.GetParent(directory).FullName;

            var thread = new Thread(GenerateAndSendDataThreadFunction);
            thread.Start(new ThreadData()
            {
                zipFolder = directory,
                zipSaveFolder = zipSaveFolder,
                progress = surveyWizard.CurrentProgress,
                isResult = isResult
            });
        }

        private void CopyCollectedFiles(string zipSaveFolder)
        {
            var filePaths = surveyWizard.CollectFilePathsToCopy();
            if (filePaths == null)
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var targetFilePath = Path.Combine(zipSaveFolder, fileName);
                try
                {
                    File.Copy(filePath, targetFilePath, true);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.SelectableLabel(
                "Contact: " + GeneralData.DeveloperMailAddress + " (" + GeneralData.DevelopedBy + ")",
                EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        private void GenerateAndSendDataThreadFunction(object data)
        {
            if (!(data is ThreadData threadData))
            {
                return;
            }

            // Debug.Log("start thread");
            var collectedDataPath = threadData.zipFolder;
            var zipName = threadData.isResult ? "ResultData" : ("ProgressData" + threadData.progress);
            var zipFilePath = Path.Combine(threadData.zipSaveFolder, zipName + ".zip");

            try
            {
                // Debug.Log("start zipping file");
                var fileZipper = new FileZipper();
                var isSucceededZippingFiles =
                    fileZipper.GenerateZip(collectedDataPath, zipFilePath, out var adjustedOutputPath);
                // Debug.Log("zipping succeeded: " + isSucceededZippingFiles);


                if (!isSucceededZippingFiles)
                {
                    if (threadData.isResult)
                    {
                        OnCompleted(TransmitResult.Failed);
                    }

                    return;
                }

                var transmitData = new TransmitData();
                if (threadData.isResult)
                {
                    transmitData.onMailSendCompleted += OnCompleted;
                }

                // Debug.Log("start sending mail");
                transmitData.SendMail(surveyData.UserId, threadData.progress, adjustedOutputPath, threadData.isResult);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnCompleted(TransmitResult transmitResult)
        {
            // Debug.Log("mail sended");

            if (!(currentStep is FinishingSurvey finishingSurvey))
            {
                return;
            }

            finishingSurvey.UpdateWithSendResult(transmitResult);
        }

        private bool isResultSendingFinished;

        private void OnDestroy()
        {
            ResetSurveyStateInGeneralData();

            surveyWizard.CleanUp();
        }

        private void ResetSurveyStateInGeneralData()
        {
            GeneralData.isSurveyActive = false;
            GeneralData.isAutomaticSortingActive = true;
            GeneralData.isLoggingActive = false;
            GeneralData.currentSurveyId = "";
        }

        private void OnInspectorUpdate()
        {
            if (currentStep is FinishingSurvey)
            {
                Repaint();
            }
        }
    }

    internal struct ThreadData
    {
        public int progress;
        public string zipFolder;
        public string zipSaveFolder;
        public bool isResult;
    }
}