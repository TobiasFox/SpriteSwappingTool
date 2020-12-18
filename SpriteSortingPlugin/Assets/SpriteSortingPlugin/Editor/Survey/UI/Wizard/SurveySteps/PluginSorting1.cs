using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class PluginSorting1 : SurveyStep
    {
        private const string SceneName = "PluginSortingExample1.unity";

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public PluginSorting1(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            var sortingTaskData = new SortingTaskData();
            sortingTaskData.SetSceneName(SceneName);
            SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
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
                    "3. Please find and solve all visual glitches in the given scene by using the " +
                    GeneralData.Name + " " + GeneralData.DetectorName + ".",
                    taskLabelStyle);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(
                    "You can optionally generate more accurate Sprite outlines by using a " + nameof(SpriteData) +
                    ". Such an asset can be created with the " + GeneralData.Name + " " +
                    GeneralData.DataAnalysisName + " window.", Styling.LabelWrapStyle);

                EditorGUILayout.Space(10);

                var currentSortingTaskData = SurveyStepSortingData.sortingTaskDataList[0];
                var buttonLabel = (currentSortingTaskData.isTaskStarted ? "Restart" : "Start") +
                                  " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    currentSortingTaskData.StartTask();
                    currentSortingTaskData.LoadedScene = EditorSceneManager.OpenScene(
                        currentSortingTaskData.FullScenePathAndName,
                        OpenSceneMode.Single);
                }

                EditorGUILayout.Space(20);

                using (new EditorGUI.DisabledScope(!currentSortingTaskData.isTaskStarted))
                {
                    if (GUILayout.Button("Finish"))
                    {
                        currentSortingTaskData.FinishTask();

                        var savePath = currentSortingTaskData.FullModifiedScenePath;

                        EditorSceneManager.SaveScene(currentSortingTaskData.LoadedScene, savePath, true);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}