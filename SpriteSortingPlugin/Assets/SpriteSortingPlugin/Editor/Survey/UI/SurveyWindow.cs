using System;
using System.IO;
using System.Threading;
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
            GeneralData.isLoggingActive = true;
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
                    var displayText = "Part " + (i + 1) + ": " + round + "% (" + tempGroupProgress +
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
                            NextSurveyStep();
                        }
                    }
                }
                else
                {
                    using (new EditorGUI.DisabledScope(!surveyWizard.HasNextStep()))
                    {
                        if (GUILayout.Button("Next -->", buttonStyle, heightLayout))
                        {
                            NextSurveyStep();
                        }
                    }
                }

                GUILayout.Space(10);
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
        }

        private void PrepareAndSendData(bool isResult = false)
        {
            surveyData.SurveyStepDataList = surveyWizard.GetData();
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

            Directory.CreateDirectory(directory);
            var pathAndName = Path.Combine(directory, (isResult ? "Result" : "") + "SurveyData.json");

            var json = surveyData.GenerateJson();
            File.WriteAllText(pathAndName, json);

            CopyCollectedFiles(directory);

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
                //TODO: exc after Skipped and therefore mail with data is not send
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
            GeneralData.isSurveyActive = false;
            GeneralData.isAutomaticSortingActive = true;
            GeneralData.isLoggingActive = false;
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