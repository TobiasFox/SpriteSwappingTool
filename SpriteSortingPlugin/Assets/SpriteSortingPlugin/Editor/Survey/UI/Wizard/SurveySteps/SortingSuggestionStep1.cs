using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStep1 : SurveyStep
    {
        private const string SceneName = "SortingSuggestionExample1.unity";

        private static readonly string[] QuestionLabels = new string[]
        {
            "1. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName + ".",
            "2. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName +
            " with the sorting suggestion functionality."
        };

        private SortingTaskData[] taskDataArray;

        public SortingSuggestionStep1(string name) : base(name)
        {
            taskDataArray = new[] {new SortingTaskData(""), new SortingTaskData(SceneName)};
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Okay, very good!", Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Now, it is time to use another functionality of the plugin.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "It is a functionality, which generates sorting order suggestions after SpriteRenderers are being identified by the " +
                GeneralData.Name + " " + GeneralData.DetectorName + ".",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "These suggestions are based on several criteria which you can select and modify. More information about each criterion is displayed when hovering over one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "Some criteria need a Sprite Data asset. If you have not created such an asset yet, you might need to create one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(25);
            EditorGUILayout.LabelField(
                "A two-step process is used for the evaluation of this functionality. However, 1. is already done, because the data from the last step can be reused :)",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            for (var i = 0; i < taskDataArray.Length; i++)
            {
                var currentTaskData = taskDataArray[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUI.DisabledScope(i == 0))
                    {
                        var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                        EditorGUILayout.LabelField(QuestionLabels[i],
                            taskLabelStyle);

                        if (i == 0)
                        {
                            EditorGUILayout.LabelField(
                                "This is already done, because the data of the last step can be reused:)",
                                Styling.LabelWrapStyle);
                        }
                        else if (i == 1)
                        {
                            EditorGUILayout.Space(5);

                            EditorGUILayout.LabelField("The same SpriteRenderer setup as in 1. is used.",
                                Styling.LabelWrapStyle);
                        }

                        EditorGUILayout.Space(10);

                        var buttonLabel = (currentTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                        if (GUILayout.Button(buttonLabel))
                        {
                            currentTaskData.isTaskStarted = true;
                            currentTaskData.isTaskFinished = false;
                            currentTaskData.TaskStartTime = DateTime.Now;
                            currentTaskData.ResetTimeNeeded();
                            currentTaskData.LoadedScene =
                                EditorSceneManager.OpenScene(currentTaskData.FullScenePathAndName,
                                    OpenSceneMode.Single);
                        }

                        EditorGUILayout.Space(10);

                        using (new EditorGUI.DisabledScope(!currentTaskData.isTaskStarted))
                        {
                            if (GUILayout.Button("Finish"))
                            {
                                currentTaskData.isTaskStarted = false;
                                currentTaskData.isTaskFinished = true;
                                currentTaskData.CalculateAndSetTimeNeeded();

                                var savePath = currentTaskData.FullModifiedScenePath;

                                EditorSceneManager.SaveScene(currentTaskData.LoadedScene, savePath, true);
                            }
                        }
                    }
                }

                EditorGUILayout.Space(20);
            }

            EditorGUI.indentLevel--;
        }
    }
}