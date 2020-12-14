using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class ComparingPluginUsage : SurveyStep
    {
        private const string ScenePathAndName =
            "Assets/SpriteSortingPlugin/Editor/Survey/Scenes/SortingWithPluginUsage.unity";

        private int questionCounter = 3;
        private bool isTaskStarted;
        private DateTime taskStartTime;
        private double neededTimeToFinishTask = -1;
        private Scene currentScene;
        private ComparingData data;

        public ComparingPluginUsage(string name, ComparingData data) : base(name)
        {
            this.data = data;
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

                var buttonLabel = (isTaskStarted ? "Restart" : "Start") + " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    isTaskStarted = true;
                    taskStartTime = DateTime.Now;
                    neededTimeToFinishTask = -1;
                    currentScene = EditorSceneManager.OpenScene(ScenePathAndName, OpenSceneMode.Single);
                    
                    data.manualSortingNeededTime = neededTimeToFinishTask;
                }

                if (isTaskStarted)
                {
                    EditorGUILayout.Space(20);
                    if (GUILayout.Button("Finish"))
                    {
                        isTaskStarted = false;
                        neededTimeToFinishTask = (DateTime.Now - taskStartTime).TotalMilliseconds;
                        Debug.Log(neededTimeToFinishTask);

                        var path = ScenePathAndName.Split(char.Parse("/"));
                        path[path.Length - 1] = path[path.Length - 1] + "_modified";

                        EditorSceneManager.SaveScene(currentScene, string.Join("/", path), true);
                        
                        data.manualSortingNeededTime = neededTimeToFinishTask;
                    }
                }
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField(
                "You can optionally generate more accurate Sprite outlines by using a " + nameof(SpriteData) +
                ". Such an asset can be created with the " + GeneralData.Name + " " +
                GeneralData.DataAnalysisName + " window.", Styling.LabelWrapStyle);


            EditorGUI.indentLevel--;
        }
    }
}