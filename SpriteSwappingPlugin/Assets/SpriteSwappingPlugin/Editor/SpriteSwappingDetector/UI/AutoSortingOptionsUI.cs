#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using SpriteSwappingPlugin.SortingGeneration;
using SpriteSwappingPlugin.SortingGeneration.Criteria;
using SpriteSwappingPlugin.SortingGeneration.Data;
using SpriteSwappingPlugin.SpriteSwappingDetector.UI.SortingGeneration;
using SpriteSwappingPlugin.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSwappingPlugin.SpriteSwappingDetector.UI
{
    [Serializable]
    public class AutoSortingOptionsUI
    {
        private bool isApplyingAutoSorting;

        //TODO list will be reset, when scripts are recompiling. Add serialization of it?
        private List<SortingCriteriaComponent> sortingCriteriaComponents;
        private SortingCriteriaPresetSelector sortingCriteriaPresetSelector;
        private bool isAutoSortingOptionExpanded;

        public bool IsApplyingAutoSorting
        {
            get
            {
                if (!isApplyingAutoSorting)
                {
                    return false;
                }

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
        }

        public void Init()
        {
            sortingCriteriaPresetSelector = ScriptableObject.CreateInstance<SortingCriteriaPresetSelector>();
            sortingCriteriaPresetSelector.Init(this);
            InitializeSortingCriteriaDataAndEditors();
        }

        public void ActivateAutoSorting()
        {
            isAutoSortingOptionExpanded = true;
            isApplyingAutoSorting = true;
        }

        public void DrawAutoSortingOptions(bool wasAnalyzeButtonClicked)
        {
            var isDisable = false;

            if (GeneralData.isSurveyActive)
            {
                isDisable = !GeneralData.isAutomaticSortingActive;
            }

            using (new EditorGUI.DisabledScope(isDisable))
            {
                var labelText = "Generation of Sorting order suggestion" +
                                (isDisable ? " - enabled in Part 3 of the survey" : "");
                // GUILayout.Label(labelText);

                isAutoSortingOptionExpanded = EditorGUILayout.Foldout(isAutoSortingOptionExpanded, labelText, true);
                // GUILayout.Label("Sorting Options");
                if (!isAutoSortingOptionExpanded)
                {
                    return;
                }

                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    var labelContent = new GUIContent("Generate sorting order?",
                        UITooltipConstants.SortingEditorUsingAutoSortingTooltip);
                    isApplyingAutoSorting = UIUtil.DrawFoldoutBoolContent(isApplyingAutoSorting, labelContent);

                    DrawSortingCriteriaHeader(wasAnalyzeButtonClicked);

                    if (!isApplyingAutoSorting)
                    {
                        return;
                    }

                    EditorGUILayout.Space();

                    if (sortingCriteriaComponents == null)
                    {
                        InitializeSortingCriteriaDataAndEditors();
                    }

                    DrawSortingCriteria();

                    DrawFooter();
                }
            }
        }

        private void DrawSortingCriteriaHeader(bool wasAnalyzeButtonClicked)
        {
            using (new EditorGUI.DisabledScope(!isApplyingAutoSorting))
            {
                using (var headerScope = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.DrawRect(headerScope.rect, Styling.SortingCriteriaHeaderBackgroundColor);
                    GUILayout.Label(new GUIContent("Sorting Criteria",
                        UITooltipConstants.SortingEditorSortingCriteriaListTooltip));
                    GUILayout.FlexibleSpace();

                    var isGUIEnabled = GUI.enabled;
                    if (isApplyingAutoSorting)
                    {
                        GUI.enabled = true;
                    }

                    var saveButtonText = wasAnalyzeButtonClicked
                        ? "Save Criteria Preset"
                        : "Save / Load Criteria Preset";
                    if (GUILayout.Button(saveButtonText, GUILayout.Width(wasAnalyzeButtonClicked ? 135 : 170)))
                    {
                        SaveCriteriaPreset(wasAnalyzeButtonClicked);
                    }

                    if (isApplyingAutoSorting)
                    {
                        GUI.enabled = isGUIEnabled;
                    }
                }

                UIUtil.DrawHorizontalLine(true);
            }
        }

        private void SaveCriteriaPreset(bool wasAnalyzeButtonClicked)
        {
            if (!wasAnalyzeButtonClicked)
            {
                sortingCriteriaPresetSelector.ShowPresetSelector();
                return;
            }

            var pathWithinProject = EditorUtility.SaveFilePanelInProject(
                "Save " + nameof(SortingCriteriaPreset),
                nameof(SortingCriteriaPreset),
                "preset",
                "Please enter a file name to save the " +
                ObjectNames.NicifyVariableName(nameof(SortingCriteriaPreset)));

            if (pathWithinProject.Length == 0)
            {
                return;
            }

            var preset = GenerateSortingCriteriaPreset();

            AssetDatabase.CreateAsset(preset, pathWithinProject);
            AssetDatabase.ForceReserializeAssets(new string[] {pathWithinProject});
            AssetDatabase.Refresh();
            Debug.Log("Preset saved to " + pathWithinProject);
        }

        private void DrawSortingCriteria()
        {
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
        }

        private void DrawFooter()
        {
            var isEverySortingCriteriaIsUsed = IsEverySortingCriteriaIsUsed();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(isEverySortingCriteriaIsUsed))
                {
                    if (GUILayout.Button("+ Add Criterion", GUILayout.Width(103)))
                    {
                        DrawSortingCriteriaMenu();
                    }
                }

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
                    usedBy = "Sorting order suggestion";
                    return true;
                }
            }

            return false;
        }

        public List<SortingCriterion> GetActiveSortingCriteria()
        {
            var list = new List<SortingCriterion>();

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

        public SortingCriterionData[] GenerateSortingCriteriaDataArray()
        {
            var sortingCriterionDataList = new List<SortingCriterionData>();

            for (var i = 0; i < sortingCriteriaComponents.Count; i++)
            {
                var criterionData = sortingCriteriaComponents[i].sortingCriterionData;

                if (!criterionData.isAddedToEditorList || !criterionData.isActive)
                {
                    continue;
                }

                sortingCriterionDataList.Add((SortingCriterionData) criterionData.Clone());
            }

            return sortingCriterionDataList.ToArray();
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

                sortingCriteriaComponent.sortingCriterionData.isAddedToEditorList = true;

                //default value
                if (sortingCriterionType == SortingCriterionType.Containment)
                {
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