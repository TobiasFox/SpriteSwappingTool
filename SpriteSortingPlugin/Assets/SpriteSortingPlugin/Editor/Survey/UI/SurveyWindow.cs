using System;
using SpriteSortingPlugin.SpriteSorting.UI;
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

        private SurveyStep introStep;
        private float currentProgress;

        [MenuItem(GeneralData.UnityMenuMainCategory + "/" + GeneralData.Name + "/Survey %g", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<SurveyWindow>();
            window.Show();
        }

        private void Awake()
        {
            surveyId = Guid.NewGuid();
            titleContent = new GUIContent("Sprite Swapping Survey");
            introStep = new IntroSurveyStep("Intro");
        }

        private void OnGUI()
        {
            GUILayout.Label("Sprite Swapping Survey", Styling.CenteredStyleBold);
            UIUtil.DrawHorizontalLine();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawProgressBars();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawHeader();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawSurveyStepContent();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawNavigationButtons();

            EditorGUILayout.Space(30);

            DrawFooter();

            // GUILayout.Label(UITextConstants.SurveyIntro);

            // EditorGUILayout.Space();
            //
            // if (GUILayout.Button("Generate and send "))
            // {
            //     var thread = new Thread(GenerateAndSendDataThreadFunction);
            //     thread.Start(new ThreadData() {progress = progress, tempPath = Application.temporaryCachePath});
            // }
            //
            // progress++;
        }

        private void DrawHeader()
        {
            GUILayout.Label(introStep.Name, Styling.CenteredStyle);
        }

        private void DrawProgressBars()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label("Progress", Styling.CenteredStyle);

                var overallProgressBarRect =
                    EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                        GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(overallProgressBarRect, currentProgress, currentProgress + "/");


                var groupProgressBarsRect =
                    EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                        GUILayout.ExpandWidth(true));

                int groupCount = 4;
                var summedSpace = (groupCount - 1) * SpaceBetweenGroupProgressBars;
                groupProgressBarsRect.width = (groupProgressBarsRect.width - summedSpace) / (float) groupCount;

                for (var i = 0; i < groupCount; i++)
                {
                    EditorGUI.ProgressBar(groupProgressBarsRect, currentProgress,
                        "Part " + (i + 1) + ": " + currentProgress + "/");
                    groupProgressBarsRect.x += groupProgressBarsRect.width + SpaceBetweenGroupProgressBars;
                }
            }
        }

        private void DrawSurveyStepContent()
        {
            using (new GUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                introStep.DrawContent();
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
                if (GUILayout.Button("<-- Back", buttonStyle, heightLayout))
                {
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Next -->", buttonStyle, heightLayout))
                {
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
    }

    internal struct ThreadData
    {
        public int progress;
        public string tempPath;
    }
}