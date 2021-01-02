using System.IO;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.Analyzing
{
    public class ConverterWindow : EditorWindow
    {
        private string surveyDataPath;
        private string loggingDataPath;
        private string outputPathAndName;
        private string outputPath;
        private string zipFolderPath;
        private string resultFolderPath;
        private string outputName = "result";
        private DataConverter dataConverter;

        [MenuItem(GeneralData.Name + "/Data Converter %h", false, 6)]
        public static void ShowWindow()
        {
            var window = GetWindow<ConverterWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            GUILayout.Label("Extract single data", Styling.CenteredStyleBold);
            ExtractSingleData();

            EditorGUILayout.Space(10);
            UIUtil.DrawHorizontalLine(true);
            EditorGUILayout.Space(10);

            GUILayout.Label("Extract multi data from zip folder", Styling.CenteredStyleBold);
            ExtractMultiZips();
        }

        private void ExtractMultiZips()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        zipFolderPath = EditorGUILayout.TextField("zip Folder Path", zipFolderPath);
                    }

                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                    {
                        var path = EditorUtility.OpenFolderPanel("Choose a zip Folder",
                            Path.Combine(Application.dataPath),
                            "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            zipFolderPath = path;
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        resultFolderPath = EditorGUILayout.TextField("Result Folder Path", resultFolderPath);
                    }

                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                    {
                        var path = EditorUtility.OpenFolderPanel("Choose a Result Folder",
                            Path.Combine(Application.dataPath), "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            resultFolderPath = path;
                        }
                    }
                }

                if (GUILayout.Button("Convert"))
                {
                    if (dataConverter == null)
                    {
                        dataConverter = new DataConverter();
                    }

                    dataConverter.AnalyzeMultiResultZips(zipFolderPath, resultFolderPath);
                }
            }
        }

        private void ExtractSingleData()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        surveyDataPath = EditorGUILayout.TextField("Survey Data Path", surveyDataPath);
                    }

                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load existing SurveyData", "", "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            surveyDataPath = path;
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        loggingDataPath = EditorGUILayout.TextField("Logging Data Path", loggingDataPath);
                    }

                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                    {
                        var path = EditorUtility.OpenFilePanel("Load existing LoggingData", "", "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            loggingDataPath = path;
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        outputPath = EditorGUILayout.TextField("Output path and Name", outputPath);
                    }

                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                    {
                        var path = EditorUtility.OpenFolderPanel("Choose an output file", "", "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            outputPath = path;
                            outputPathAndName = Path.Combine(outputPath, $"{outputName}.csv");
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        outputName = EditorGUILayout.TextField("Output Name", outputName, GUILayout.ExpandWidth(true));
                        if (changeScope.changed)
                        {
                            outputPathAndName = Path.Combine(outputPath, $"{outputName}.csv");
                        }
                    }

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField(".csv", GUILayout.ExpandWidth(false));
                    }
                }

                if (GUILayout.Button("Convert"))
                {
                    if (dataConverter == null)
                    {
                        dataConverter = new DataConverter();
                    }

                    dataConverter.ConvertAndSave(surveyDataPath, loggingDataPath, outputPathAndName);
                }
            }
        }
    }
}