﻿using System;
using SpriteSortingPlugin.Survey.UI.Wizard;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI
{
    public class SurveyWindow : EditorWindow
    {
        private const float SpaceBetweenGroupProgressBars = 15;

        private Guid surveyId;
        private int progress;

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
            surveyId = Guid.NewGuid();
            titleContent = new GUIContent(GeneralData.Name + " Survey");

            surveyStepGenerator = new SurveyStepGenerator();

            surveyWizard = new SurveyWizard();
            surveyWizard.SetSurveySteps(surveyStepGenerator.GenerateSurveySteps());
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

            // if (GUILayout.Button("Generate and send "))
            // {
            //     var thread = new Thread(GenerateAndSendDataThreadFunction);
            //     thread.Start(new ThreadData() {progress = progress, tempPath = Application.temporaryCachePath});
            // }
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
                var progressPercentage = tempCurrentProgress / (float) surveyWizard.OverallProgress;

                EditorGUI.ProgressBar(overallProgressBarRect, progressPercentage,
                    (Math.Round(progressPercentage * 100, 2)) + "% (" + tempCurrentProgress + "/" +
                    surveyWizard.OverallProgress +
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
                    var surveyGroupCurrentProgress = tempGroupProgress / (float) surveyGroup.OverallProgress;

                    var round = Math.Round(surveyGroupCurrentProgress * 100, 2);
                    var displayText = "Part " + (i + 1) /*+ ": " + surveyGroup.Name + ", "*/ +": "+
                                      round + "% (" + tempGroupProgress +
                                      "/" + surveyGroup.OverallProgress + ")";

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
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("<-- Back", buttonStyle, heightLayout))
                    {
                        surveyWizard.Backward();
                        currentStep = surveyWizard.GetCurrent();
                    }

                    GUILayout.Space(10);
                    using (new EditorGUI.DisabledScope(!surveyWizard.HasNextStep()))
                    {
                        if (GUILayout.Button("Next -->", buttonStyle, heightLayout))
                        {
                            surveyWizard.Forward();
                            currentStep = surveyWizard.GetCurrent();
                        }
                    }
                }

                GUILayout.Space(10);
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

            try
            {
                Debug.Log("start thread");
                var collectedDataPath = threadData.tempPath + "/data/progress0" /* + threadData.progress*/;
                var zipFilePath = threadData.tempPath + "/data/data" + threadData.progress + ".zip";

                Debug.Log("start zipping file");
                var fileZipper = new FileZipper();
                var isSucceededZippingFiles = fileZipper.GenerateZip(collectedDataPath, zipFilePath);
                Debug.Log("zipping succeeded: " + isSucceededZippingFiles);

                if (isSucceededZippingFiles)
                {
                    Debug.Log("start sending mail");
                    var transmitData = new TransmitData();
                    transmitData.SendMail(surveyId, threadData.progress, zipFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDestroy()
        {
            surveyWizard.CleanUp();
        }
    }

    internal struct ThreadData
    {
        public int progress;
        public string tempPath;
    }
}