using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class ComparisonManualSortingStep : SurveyStep
    {
        private const string ScenePathAndName = "Assets/SpriteSortingPlugin/Editor/Survey/Scenes/ManualSorting.unity";

        private SortingTaskData sortingTaskData;

        public ComparisonManualSortingStep(string name) : base(name)
        {
            sortingTaskData = new SortingTaskData(ScenePathAndName);
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(
                "This part compares the manual approach to the approach used by the Sprite Swapping Detector. For this comparison, two short tasks are given.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Short Description about what causes a visual glitch:", Styling.LabelWrapStyle);
            var visualGlitchDescription =
                "Depending on the position of the camera which renders a scene, a visual glitch happens, when SpriteRenderers overlap and have identical sorting options.";
            EditorGUILayout.LabelField(visualGlitchDescription, Styling.LabelWrapStyle);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "To detect potential SpriteRenderer the manual method can be used, by moving around the Unity SceneCamera and watching out for Sprite swaps.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(20);

            using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                EditorGUILayout.LabelField(
                    "1. Please find and solve all visual glitches in the given scene by using the manual approach.",
                    taskLabelStyle);

                EditorGUILayout.Space();
                var largeLabel = new GUIStyle(EditorStyles.largeLabel) {wordWrap = true};
                EditorGUILayout.LabelField("Please don't start the play mode.",
                    largeLabel);
                EditorGUILayout.LabelField("Instead, use the editor mode and move the SceneCamera.",
                    largeLabel);

                var buttonLabel = (sortingTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    sortingTaskData.isTaskStarted = true;
                    sortingTaskData.TaskStartTime = DateTime.Now;
                    sortingTaskData.ResetTimeNeeded();
                    
                    //TODO open Scene and may discard everything before
                    sortingTaskData.LoadedScene = EditorSceneManager.OpenScene(ScenePathAndName, OpenSceneMode.Single);
                }

                using (new EditorGUI.DisabledScope(!sortingTaskData.isTaskStarted))
                {
                    EditorGUILayout.Space(20);
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