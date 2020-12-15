using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class ComparingPluginUsage : SurveyStep
    {
        private const string ScenePathAndName =
            "Assets/SpriteSortingPlugin/Editor/Survey/Scenes/SortingWithPluginUsage.unity";

        private int questionCounter = 3;
        private SortingTaskData sortingTaskData;

        public ComparingPluginUsage(string name) : base(name)
        {
            sortingTaskData = new SortingTaskData(ScenePathAndName);
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Well done :)", Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("After the manual approach were used, it's now time to use the plugin.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "The " + GeneralData.Name + " " + GeneralData.DetectorName +
                " automatically identifies overlapping and unsorted SpritesRenderers and helps to sort them.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "You can find the " + GeneralData.Name + " " + GeneralData.DetectorName + " here:\n" +
                GeneralData.UnityMenuMainCategory + " -> " + GeneralData.Name + " -> " + GeneralData.DetectorName,
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    "2. Please find and solve all visual glitches in the given scene by using the " +
                    GeneralData.Name + " " + GeneralData.DetectorName + ".",
                    taskLabelStyle);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(
                    "You can optionally generate more accurate Sprite outlines by using a " + nameof(SpriteData) +
                    ". Such an asset can be created with the " + GeneralData.Name + " " +
                    GeneralData.DataAnalysisName + " window.", Styling.LabelWrapStyle);

                EditorGUILayout.Space(10);

                var buttonLabel = (sortingTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    sortingTaskData.isTaskStarted = true;
                    sortingTaskData.TaskStartTime = DateTime.Now;
                    sortingTaskData.ResetTimeNeeded();
                    sortingTaskData.LoadedScene = EditorSceneManager.OpenScene(ScenePathAndName, OpenSceneMode.Single);
                }

                EditorGUILayout.Space(20);

                using (new EditorGUI.DisabledScope(!sortingTaskData.isTaskStarted))
                {
                    if (GUILayout.Button("Finish"))
                    {
                        sortingTaskData.isTaskStarted = false;
                        sortingTaskData.CalculateAndSetTimeNeeded();

                        var path = ScenePathAndName.Split(char.Parse("/"));
                        path[path.Length - 1] = "modified_" + path[path.Length - 1];

                        EditorSceneManager.SaveScene(sortingTaskData.LoadedScene, string.Join("/", path), true);
                    }
                }
            }


            EditorGUI.indentLevel--;
        }
    }
}