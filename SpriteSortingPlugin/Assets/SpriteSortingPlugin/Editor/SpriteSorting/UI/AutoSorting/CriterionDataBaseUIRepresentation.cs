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

using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public abstract class CriterionDataBaseUIRepresentation<T> where T : SortingCriterionData
    {
        protected T sortingCriterionData;
        protected string title = "Criteria";
        protected string tooltip = "";

        private Rect headerRect;
        private bool isShowingInInspector;

        public string Title => title;

        protected float CalculateIndentSpace => (EditorGUI.indentLevel * EditorGUIUtility.singleLineHeight) * 0.8f;

        public void Initialize(T sortingCriterionData, bool isShowingInInspector = false)
        {
            this.isShowingInInspector = isShowingInInspector;
            this.sortingCriterionData = sortingCriterionData;
            InternalInitialize();
        }

        protected virtual void InternalInitialize()
        {
        }

        public void UpdateSortingCriterionData(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
        }

        public void OnInspectorGUI()
        {
            DrawHeader();

            if (!sortingCriterionData.isExpanded)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(!sortingCriterionData.isActive))
            {
                EditorGUI.indentLevel++;
                OnInspectorGuiInternal();
                EditorGUI.indentLevel--;
            }
        }

        protected abstract void OnInspectorGuiInternal();

        private void DrawHeader()
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 22);
            DrawHeader(backgroundRect);
        }

        private void DrawHeader(Rect backgroundRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                headerRect = new Rect(backgroundRect) {height = 22};
            }

            Rect removeButtonRect;
            if (isShowingInInspector)
            {
                removeButtonRect = Rect.zero;
            }
            else
            {
                removeButtonRect = headerRect;
                removeButtonRect.xMin = removeButtonRect.xMax - 31.5f;
                removeButtonRect.yMax -= 2;
                removeButtonRect.yMin += 2;
            }

            var priorityRect = headerRect;
            priorityRect.yMax -= 2;
            priorityRect.yMin += 2;
            if (isShowingInInspector)
            {
                if (sortingCriterionData is ContainmentSortingCriterionData)
                {
                    priorityRect.xMin += 175;
                    priorityRect.width = 175;
                }
                else
                {
                    priorityRect.xMin += 207.5f;
                    priorityRect.width = 100;
                }
            }
            else
            {
                if (sortingCriterionData is ContainmentSortingCriterionData)
                {
                    priorityRect.xMax = removeButtonRect.xMin;
                    priorityRect.xMin = priorityRect.xMax - 175;
                }
                else
                {
                    priorityRect.width = 100;
                    priorityRect.x = removeButtonRect.xMin - 140;
                }
            }

            var labelRect = headerRect;
            labelRect.xMin += 37f;
            labelRect.xMax = priorityRect.xMin;

            var foldoutRect = new Rect(headerRect.x, headerRect.y, 13, headerRect.height);
            var toggleRect = new Rect(headerRect.x + 16, headerRect.y, 13, headerRect.height);

            var headerColor = isShowingInInspector
                ? Styling.SortingCriteriaInspectorHeaderBackgroundColor
                : Styling.SortingCriteriaHeaderBackgroundColor;
            EditorGUI.DrawRect(headerRect, headerColor);

            using (new EditorGUI.DisabledScope(!sortingCriterionData.isActive))
            {
                if (sortingCriterionData is ContainmentSortingCriterionData)
                {
                    var labelStyle = new GUIStyle(EditorStyles.label);
                    GUI.Label(priorityRect, new GUIContent("Preferred over all other criteria",
                        UITooltipConstants.SortingCriteriaContainmentWeightTooltip), labelStyle);
                }
                else
                {
                    EditorGUIUtility.labelWidth = 45;

                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                    {
                        sortingCriterionData.priority = EditorGUI.FloatField(priorityRect,
                            new GUIContent("Weight", UITooltipConstants.SortingCriteriaWeightTooltip),
                            sortingCriterionData.priority);
                        if (changeCheckScope.changed)
                        {
                            sortingCriterionData.priority = Mathf.Max(0, sortingCriterionData.priority);
                        }
                    }

                    EditorGUIUtility.labelWidth = 0;
                }

                EditorGUI.LabelField(labelRect, new GUIContent(title, tooltip), EditorStyles.boldLabel);
            }

            if (!isShowingInInspector)
            {
                if (GUI.Button(removeButtonRect, Styling.RemoveIcon))
                {
                    sortingCriterionData.isAddedToEditorList = false;
                }
            }

            var isGUIEnabled = GUI.enabled;
            GUI.enabled = true;
            sortingCriterionData.isExpanded = GUI.Toggle(foldoutRect, sortingCriterionData.isExpanded, GUIContent.none,
                EditorStyles.foldout);
            GUI.enabled = isGUIEnabled;

            sortingCriterionData.isActive =
                GUI.Toggle(toggleRect, sortingCriterionData.isActive, GUIContent.none);

            //gui needs to be enabled to receive mouse events
            GUI.enabled = true;
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseDown)
            {
                GUI.enabled = isGUIEnabled;
                return;
            }

            if (labelRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.button == 0)
                {
                    sortingCriterionData.isExpanded = !sortingCriterionData.isExpanded;
                }

                currentEvent.Use();
            }

            GUI.enabled = isGUIEnabled;
        }
    }
}