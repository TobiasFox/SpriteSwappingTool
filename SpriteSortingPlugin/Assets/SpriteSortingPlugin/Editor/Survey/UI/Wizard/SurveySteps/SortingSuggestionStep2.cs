using System;
using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SortingSuggestionStep2 : SurveyStep
    {
        private static readonly string[] SceneNames = new string[]
        {
            "PluginSortingExample3.unity",
            "SortingSuggestionExample2.unity"
        };

        private static readonly string[] QuestionLabels = new string[]
        {
            "3. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName + ".",
            "4. Please find and solve all visual glitches in the given scene by using the " +
            GeneralData.Name + " " + GeneralData.DetectorName +
            " with the sorting suggestion functionality."
        };

        private bool isDescriptionVisible;

        private SurveyStepSortingData SurveyStepSortingData => (SurveyStepSortingData) surveyStepData;

        public SortingSuggestionStep2(string name) : base(name)
        {
            surveyStepData = new SurveyStepSortingData();

            foreach (var sceneName in SceneNames)
            {
                var sortingTaskData = new SortingTaskData();
                sortingTaskData.SetSceneName(sceneName);
                SurveyStepSortingData.sortingTaskDataList.Add(sortingTaskData);
            }
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Nice!", Styling.LabelWrapStyle);

            EditorGUILayout.Space(5);

            isDescriptionVisible = EditorGUILayout.Foldout(isDescriptionVisible,
                "Information about the sorting order suggestion functionality", true);

            if (isDescriptionVisible)
            {
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
            }

            EditorGUILayout.Space(25);
            EditorGUILayout.LabelField(
                "This time a two-step process is used with a scene setup with slightly more SpriteRenderers.",
                Styling.LabelWrapStyle);

            EditorGUILayout.Space(10);

            for (var i = 0; i < SurveyStepSortingData.sortingTaskDataList.Count; i++)
            {
                var currentTaskData = SurveyStepSortingData.sortingTaskDataList[i];
                using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
                    EditorGUILayout.LabelField(QuestionLabels[i],
                        taskLabelStyle);

                    if (i == 1)
                    {
                        EditorGUILayout.Space(5);

                        EditorGUILayout.LabelField("The same SpriteRenderer setup as in 3. is used.",
                            Styling.LabelWrapStyle);
                    }

                    EditorGUILayout.Space(10);

                    var buttonLabel = (currentTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                    if (GUILayout.Button(buttonLabel))
                    {
                        currentTaskData.StartTask();
                        currentTaskData.LoadedScene = EditorSceneManager.OpenScene(currentTaskData.FullScenePathAndName,
                            OpenSceneMode.Single);
                    }

                    EditorGUILayout.Space(10);

                    using (new EditorGUI.DisabledScope(!currentTaskData.isTaskStarted))
                    {
                        if (GUILayout.Button("Finish"))
                        {
                            currentTaskData.FinishTask();

                            var savePath = currentTaskData.FullModifiedScenePath;
                            EditorSceneManager.SaveScene(currentTaskData.LoadedScene, savePath, true);
                        }
                    }
                }

                EditorGUILayout.Space(20);
            }

            EditorGUI.indentLevel--;
        }
    }
}