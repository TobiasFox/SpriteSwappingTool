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
        private const string ScenePathAndName1 =
            "Assets/SpriteSortingPlugin/Editor/Survey/Scenes/SortingSuggestion1.unity";

        private const string ScenePathAndName2 =
            "Assets/SpriteSortingPlugin/Editor/Survey/Scenes/SortingSuggestion2.unity";

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
            taskDataArray = new[] {new SortingTaskData(ScenePathAndName1), new SortingTaskData(ScenePathAndName2)};
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
                "After SpriteRenderers are being identified by the " + GeneralData.Name + " " +
                GeneralData.DetectorName +
                " you can optionally use the automatic sorting functionality to generate a sorting suggestion of these SpriteRenderers.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "The suggestion is based on several criteria which you can select and modify. More information about each criterion is displayed when hovering over one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "Some criteria need a Sprite Data asset. If you have not created such an asset yet, you might need to create one.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(30);
            EditorGUILayout.LabelField(
                "A two-step process is used for the evaluation of this functionality; however your input really helps me a lot.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            for (var i = 0; i < taskDataArray.Length; i++)
            {
                var currentTaskData = taskDataArray[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                    EditorGUILayout.LabelField(QuestionLabels[i],
                        taskLabelStyle);

                    if (i == 1)
                    {
                        EditorGUILayout.Space(5);

                        EditorGUILayout.LabelField("The same SpriteRenderer setup is used.", Styling.LabelWrapStyle);
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
                            EditorSceneManager.OpenScene(currentTaskData.scenePathAndName, OpenSceneMode.Single);

                    }

                    EditorGUILayout.Space(10);

                    using (new EditorGUI.DisabledScope(!currentTaskData.isTaskStarted))
                    {
                        if (GUILayout.Button("Finish"))
                        {
                            currentTaskData.isTaskStarted = false;
                            currentTaskData.isTaskFinished = true;
                            currentTaskData.CalculateAndSetTimeNeeded();

                            var path = currentTaskData.scenePathAndName.Split(char.Parse("/"));
                            path[path.Length - 1] = "modified_" + path[path.Length - 1];

                            EditorSceneManager.SaveScene(currentTaskData.LoadedScene, string.Join("/", path), true);
                        }
                    }
                }

                EditorGUILayout.Space(20);
            }

            EditorGUI.indentLevel--;
        }
    }
}