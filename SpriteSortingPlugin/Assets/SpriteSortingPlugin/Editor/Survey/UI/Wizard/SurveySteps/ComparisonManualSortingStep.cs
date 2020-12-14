using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class ComparisonManualSortingStep : SurveyStep
    {
        private static int questionCounterStart = 1;
        private const string ScenePathAndName = "Assets/SpriteSortingPlugin/Editor/Survey/Scenes/ManualSorting.unity";

        private bool isTaskStarted;
        private DateTime taskStartTime;
        private double neededTimeToFinishTask = -1;
        private int questionCounter;
        private Scene currentScene;
        private ComparingData data;

        public ComparisonManualSortingStep(string name, ComparingData data) : base(name)
        {
            this.data = data;
            questionCounterStart++;
        }

        public override void DrawContent()
        {
            questionCounter = questionCounterStart;
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

                var buttonLabel = (isTaskStarted ? "Restart" : "Start") + " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    isTaskStarted = true;
                    taskStartTime = DateTime.Now;
                    neededTimeToFinishTask = -1;
                    //TODO open Scene and may discard everything before
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

                        var path = ScenePathAndName.Split(char.Parse("/"));
                        path[path.Length - 1] = path[path.Length - 1] + "_modified";

                        EditorSceneManager.SaveScene(currentScene, string.Join("/", path), true);
                        data.manualSortingNeededTime = neededTimeToFinishTask;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}