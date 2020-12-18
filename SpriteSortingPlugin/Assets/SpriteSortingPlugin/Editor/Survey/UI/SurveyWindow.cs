using System;
using System.IO;
using System.Threading;
using SpriteSortingPlugin.Survey.UI.Wizard;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
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

            if (!finishingSurvey.IsSendingDataButtonPressedThisFrame)
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

                var overallProgressBarRect =
                    EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                        GUILayout.ExpandWidth(true));
                var tempCurrentProgress = surveyWizard.CurrentProgress;
                var progressPercentage = tempCurrentProgress / (float) surveyWizard.TotalProgress;

                EditorGUI.ProgressBar(overallProgressBarRect, progressPercentage,
                    (Math.Round(progressPercentage * 100, 2)) + "% (" + tempCurrentProgress + "/" +
                    surveyWizard.TotalProgress +
                    ")");

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

                    var tempGroupProgress = surveyGroup.CurrentProgress;
                    var surveyGroupCurrentProgress = tempGroupProgress / (float) surveyGroup.TotalProgress;

                    var round = Math.Round(surveyGroupCurrentProgress * 100, 2);
                    var displayText = "Part " + (i + 1) /*+ ": " + surveyGroup.Name + ", "*/ + ": " +
                                      round + "% (" + tempGroupProgress +
                                      "/" + surveyGroup.TotalProgress + ")";

                    EditorGUI.ProgressBar(groupProgressBarsRect, surveyGroupCurrentProgress,
                        displayText);
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
            using (new EditorGUILayout.HorizontalScope())
            {
                var buttonStyle = new GUIStyle(Styling.ButtonStyleBold);
                var heightLayout = GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.25f);
                buttonStyle.fontSize++;
                GUILayout.Space(10);

                if (!surveyWizard.HasPreviousStep())
                {
                    using (new EditorGUI.DisabledScope(currentStep == null))
                    {
                        if (GUILayout.Button("Start", buttonStyle, heightLayout))
                        {
                            surveyWizard.Forward();
                            currentStep = surveyWizard.GetCurrent();
                            PrepareAndSendData();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("<-- Back", buttonStyle, heightLayout))
                    {
                        surveyWizard.Backward();
                        currentStep = surveyWizard.GetCurrent();
                        PrepareAndSendData();
                    }

                    GUILayout.Space(10);
                    using (new EditorGUI.DisabledScope(!surveyWizard.HasNextStep()))
                    {
                        if (GUILayout.Button("Next -->", buttonStyle, heightLayout))
                        {
                            surveyWizard.Forward();
                            currentStep = surveyWizard.GetCurrent();
                            PrepareAndSendData();
                        }
                    }
                }

                GUILayout.Space(10);
            }
        }

        private void PrepareAndSendData(bool isResult = false)
        {
            surveyData.SurveyStepDataList = surveyWizard.GetData();
            surveyData.currentProgress = surveyWizard.CurrentProgress;
            surveyData.totalProgress = surveyWizard.TotalProgress;

            var directory = Application.temporaryCachePath + Path.DirectorySeparatorChar +
                            Path.Combine(SurveyDataOutputPath) + Path.DirectorySeparatorChar +
                            (isResult ? surveyData.ResultSaveFolder : surveyData.SaveFolder);

            Directory.CreateDirectory(directory);
            var pathAndName = directory + Path.DirectorySeparatorChar + (isResult ? "Result" : "") + "SurveyData.json";

            var json = surveyData.GenerateJson();
            File.WriteAllText(pathAndName, json);

            if (isResult)
            {
                CopyAllModifiedScenes(directory);
            }

            SendMail(isResult, directory);
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

        private void CopyAllModifiedScenes(string zipSaveFolder)
        {
            var scenePath = Path.Combine(SortingTaskData.ModifiedSceneFolderPath);

            var dirInfo = new DirectoryInfo(scenePath);
            foreach (var fileInfo in dirInfo.EnumerateFiles())
            {
                if (!fileInfo.Extension.Equals(".unity"))
                {
                    continue;
                }

                fileInfo.CopyTo(zipSaveFolder + Path.DirectorySeparatorChar + fileInfo.Name, true);
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
            var zipFilePath = threadData.zipSaveFolder + Path.DirectorySeparatorChar + zipName + ".zip";

            try
            {
                // Debug.Log("start zipping file");
                var fileZipper = new FileZipper();
                var isSucceededZippingFiles = fileZipper.GenerateZip(collectedDataPath, zipFilePath);
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
                transmitData.SendMail(surveyData.UserId, threadData.progress, zipFilePath);
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
            surveyWizard.CleanUp();
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