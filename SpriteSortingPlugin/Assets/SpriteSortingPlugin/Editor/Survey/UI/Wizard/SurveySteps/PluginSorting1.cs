﻿using System;
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

        private SortingTaskData sortingTaskData;

        public PluginSorting1(string name) : base(name)
        {
            sortingTaskData = new SortingTaskData(SceneName);
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

                var buttonLabel = (sortingTaskData.isTaskStarted ? "Restart" : "Start") + " and open scene";
                if (GUILayout.Button(buttonLabel))
                {
                    sortingTaskData.isTaskStarted = true;
                    sortingTaskData.TaskStartTime = DateTime.Now;
                    sortingTaskData.ResetTimeNeeded();
                    sortingTaskData.LoadedScene =
                        EditorSceneManager.OpenScene(sortingTaskData.FullScenePathAndName, OpenSceneMode.Single);
                }

                EditorGUILayout.Space(20);

                using (new EditorGUI.DisabledScope(!sortingTaskData.isTaskStarted))
                {
                    if (GUILayout.Button("Finish"))
                    {
                        sortingTaskData.isTaskStarted = false;
                        sortingTaskData.CalculateAndSetTimeNeeded();

                        var savePath = sortingTaskData.FullModifiedScenePath;

                        EditorSceneManager.SaveScene(sortingTaskData.LoadedScene, savePath, true);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}