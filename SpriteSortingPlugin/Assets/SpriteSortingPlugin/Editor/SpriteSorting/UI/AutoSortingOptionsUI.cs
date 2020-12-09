﻿using System;
using System.Collections.Generic;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Criteria;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.SpriteSorting.UI.AutoSorting;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteSorting.UI
{
    [Serializable]
    public class AutoSortingOptionsUI
    {
        private bool isApplyingAutoSorting;

        //TODO list will be reset, when scripts are recompiling. Add serialization of it?
        private List<SortingCriteriaComponent> sortingCriteriaComponents;
        private SortingCriteriaPresetSelector sortingCriteriaPresetSelector;

        public bool IsApplyingAutoSorting => isApplyingAutoSorting;

        public void Init()
        {
            sortingCriteriaPresetSelector = ScriptableObject.CreateInstance<SortingCriteriaPresetSelector>();
            sortingCriteriaPresetSelector.Init(this);
            InitializeSortingCriteriaDataAndEditors();
        }

        public void DrawAutoSortingOptions(bool wasAnalyzeButtonClicked)
        {
            GUILayout.Label("Automatic Sorting");

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                var labelContent = new GUIContent("Apply auto sorting?",
                    UITooltipConstants.SortingEditorUsingAutoSortingTooltip);
                isApplyingAutoSorting = UIUtil.DrawFoldoutBoolContent(isApplyingAutoSorting, labelContent);

                if (!isApplyingAutoSorting)
                {
                    return;
                }

                EditorGUILayout.Space();

                if (sortingCriteriaComponents == null)
                {
                    InitializeSortingCriteriaDataAndEditors();
                }

                using (var headerScope = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.DrawRect(headerScope.rect, Styling.SortingCriteriaHeaderBackgroundColor);
                    GUILayout.Label("Sorting Criteria");
                    GUILayout.FlexibleSpace();

                    var isGUIEnabled = GUI.enabled;
                    GUI.enabled = true;

                    if (GUILayout.Button(
                        wasAnalyzeButtonClicked ? "Save Criteria Preset" : "Save / Load Criteria Preset",
                        GUILayout.Width(wasAnalyzeButtonClicked ? 135 : 170)))
                    {
                        if (!wasAnalyzeButtonClicked)
                        {
                            sortingCriteriaPresetSelector.ShowPresetSelector();
                        }
                        else
                        {
                            var pathWithinProject = EditorUtility.SaveFilePanelInProject(
                                "Save " + nameof(SortingCriteriaPreset),
                                nameof(SortingCriteriaPreset),
                                "preset",
                                "Please enter a file name to save the " +
                                ObjectNames.NicifyVariableName(nameof(SortingCriteriaPreset)));

                            if (pathWithinProject.Length != 0)
                            {
                                var preset = GenerateSortingCriteriaPreset();

                                AssetDatabase.CreateAsset(preset, pathWithinProject);
                                AssetDatabase.ForceReserializeAssets(new string[] {pathWithinProject});
                                AssetDatabase.Refresh();
                                Debug.Log("Preset saved to " + pathWithinProject);
                            }
                        }
                    }

                    GUI.enabled = isGUIEnabled;
                }

                UIUtil.DrawHorizontalLine(true);
                var isMinOneSortingCriterionEditorDrawn = false;

                for (var i = 0; i < sortingCriteriaComponents.Count; i++)
                {
                    var sortingCriteriaComponent = sortingCriteriaComponents[i];
                    if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                    {
                        continue;
                    }

                    sortingCriteriaComponent.criterionDataBaseUIRepresentation.OnInspectorGUI();
                    isMinOneSortingCriterionEditorDrawn = true;
                    if (sortingCriteriaComponents.Count > 0 && i < sortingCriteriaComponents.Count - 1)
                    {
                        UIUtil.DrawHorizontalLine();
                    }
                }

                if (isMinOneSortingCriterionEditorDrawn)
                {
                    UIUtil.DrawHorizontalLine(true);
                }

                var isEverySortingCriteriaIsUsed = IsEverySortingCriteriaIsUsed();
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(isEverySortingCriteriaIsUsed))
                    {
                        if (GUILayout.Button("Add Criterion", GUILayout.Width(103)))
                        {
                            DrawSortingCriteriaMenu();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(isEverySortingCriteriaIsUsed))
                    {
                        if (GUILayout.Button("All", GUILayout.Width(50)))
                        {
                            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
                            {
                                sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;
                                sortingCriteriaComponent.sortingCriterionData.isActive = true;
                            }
                        }
                    }

                    var isMinOneSortingCriteriaIsUsed = IsMinOneSortingCriteriaIsUsed();
                    using (new EditorGUI.DisabledScope(!isMinOneSortingCriteriaIsUsed))
                    {
                        if (GUILayout.Button("None", GUILayout.Width(50)))
                        {
                            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
                            {
                                sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = false;
                                sortingCriteriaComponent.sortingCriterionData.isActive = false;
                            }
                        }
                    }
                }
            }
        }

        public bool HasActiveAutoSortingCriteria()
        {
            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    continue;
                }

                if (!sortingCriteriaComponent.sortingCriterionData.isActive)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public bool IsCameraRequired(out string usedBy)
        {
            usedBy = "";
            if (!isApplyingAutoSorting || sortingCriteriaComponents == null)
            {
                return false;
            }

            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!(sortingCriteriaComponent.sortingCriterionData is DefaultSortingCriterionData
                    defaultSortingCriterionData))
                {
                    continue;
                }

                if (defaultSortingCriterionData.sortingCriterionType != SortingCriterionType.CameraDistance)
                {
                    continue;
                }

                var isRequiredForCameraDistanceCriterion = defaultSortingCriterionData.isAddedToEditorList &&
                                                           defaultSortingCriterionData.isActive;

                if (isRequiredForCameraDistanceCriterion)
                {
                    usedBy += "generate sorting order suggestions (camera distance difference)";
                }

                return isRequiredForCameraDistanceCriterion;
            }

            return false;
        }

        public bool IsSpriteDataRequired(out string usedBy)
        {
            usedBy = "";

            if (!isApplyingAutoSorting || sortingCriteriaComponents == null)
            {
                return false;
            }

            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList ||
                    !sortingCriteriaComponent.sortingCriterionData.isActive)
                {
                    continue;
                }

                if (sortingCriteriaComponent.sortingCriterion != null &&
                    sortingCriteriaComponent.sortingCriterion.IsUsingSpriteData())
                {
                    usedBy = "Automatic Sorting";
                    return true;
                }
            }

            return false;
        }

        public List<SortingCriterion<SortingCriterionData>> GetActiveSortingCriteria()
        {
            var list = new List<SortingCriterion<SortingCriterionData>>();

            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    continue;
                }

                if (!sortingCriteriaComponent.sortingCriterionData.isActive)
                {
                    continue;
                }

                list.Add(sortingCriteriaComponent.sortingCriterion);
            }

            return list;
        }

        public SortingCriteriaPreset GenerateSortingCriteriaPreset()
        {
            var preset = ScriptableObject.CreateInstance<SortingCriteriaPreset>();
            preset.sortingCriterionData = new SortingCriterionData[sortingCriteriaComponents.Count];

            for (var i = 0; i < sortingCriteriaComponents.Count; i++)
            {
                var sortingCriteriaComponent = sortingCriteriaComponents[i];
                preset.sortingCriterionData[i] =
                    (SortingCriterionData) sortingCriteriaComponent.sortingCriterionData.Clone();
            }

            return preset;
        }

        public void UpdateSortingCriteriaFromPreset(SortingCriteriaPreset preset)
        {
            for (var i = 0; i < preset.sortingCriterionData.Length; i++)
            {
                var sortingCriteriaComponent = sortingCriteriaComponents[i];
                sortingCriteriaComponent.sortingCriterionData = preset.sortingCriterionData[i];
                sortingCriteriaComponent.criterionDataBaseUIRepresentation.UpdateSortingCriterionData(
                    sortingCriteriaComponent
                        .sortingCriterionData);
                sortingCriteriaComponents[i] = sortingCriteriaComponent;
            }
        }

        public void Cleanup()
        {
            Object.DestroyImmediate(sortingCriteriaPresetSelector);
        }

        private void InitializeSortingCriteriaDataAndEditors()
        {
            sortingCriteriaComponents = new List<SortingCriteriaComponent>();

            foreach (SortingCriterionType sortingCriterionType in Enum.GetValues(typeof(SortingCriterionType)))
            {
                var sortingCriteriaComponent =
                    SortingCriteriaComponentFactory.CreateSortingCriteriaComponent(sortingCriterionType);
                sortingCriteriaComponents.Add(sortingCriteriaComponent);

                //default value
                if (sortingCriterionType == SortingCriterionType.Containment)
                {
                    sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;
                    sortingCriteriaComponent.sortingCriterionData.isActive = true;
                }
            }
        }

        private void DrawSortingCriteriaMenu()
        {
            var menu = new GenericMenu();

            for (var i = 0; i < sortingCriteriaComponents.Count; i++)
            {
                var sortingCriteriaComponent = sortingCriteriaComponents[i];
                if (sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    continue;
                }

                var content =
                    new GUIContent(sortingCriteriaComponent.criterionDataBaseUIRepresentation.Title);
                menu.AddItem(content, false, AddSortingCriteria, i);
            }

            menu.ShowAsContext();
        }

        private bool IsEverySortingCriteriaIsUsed()
        {
            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (!sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsMinOneSortingCriteriaIsUsed()
        {
            foreach (var sortingCriteriaComponent in sortingCriteriaComponents)
            {
                if (sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddSortingCriteria(object userdata)
        {
            var index = (int) userdata;
            var sortingCriteriaComponent = sortingCriteriaComponents[index];
            sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;
            sortingCriteriaComponent.sortingCriterionData.isActive = true;
            sortingCriteriaComponents[index] = sortingCriteriaComponent;
        }
    }
}