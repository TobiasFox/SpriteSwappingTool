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

using SpriteSortingPlugin.SpriteSorting.AutoSorting.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public class ContainmentCriterionDataUIRepresentation : CriterionDataBaseUIRepresentation
    {
        private ContainmentSortingCriterionData ContainmentSortingCriterionData =>
            (ContainmentSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = "Containment";
            tooltip = UITooltipConstants.ContainmentTooltip;
        }

        protected override void OnInspectorGuiInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(CalculateIndentSpace);
                using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent("Sort contained Sprite in",
                            UITooltipConstants.ContainmentEncapsulatedSpriteInForegroundTooltip));
                        GUILayout.FlexibleSpace();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground = GUILayout.Toggle(
                            ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground, "Foreground",
                            Styling.ButtonStyle);

                        ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground = !GUILayout.Toggle(
                            !ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground, "Background",
                            Styling.ButtonStyle);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground))
            {
                ContainmentSortingCriterionData.isCheckingAlpha = EditorGUILayout.ToggleLeft(
                    new GUIContent("Is checking transparency of larger sprite",
                        UITooltipConstants.ContainmentCheckingAlphaTooltip),
                    ContainmentSortingCriterionData.isCheckingAlpha);

                using (new EditorGUI.DisabledScope(!ContainmentSortingCriterionData.isCheckingAlpha))
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        ContainmentSortingCriterionData.alphaThreshold =
                            EditorGUILayout.FloatField(
                                new GUIContent("Alpha threshold", UITooltipConstants.ContainmentAlphaThresholdTooltip),
                                ContainmentSortingCriterionData.alphaThreshold);
                        if (changeScope.changed)
                        {
                            ContainmentSortingCriterionData.alphaThreshold =
                                Mathf.Clamp01(ContainmentSortingCriterionData.alphaThreshold);
                        }
                    }
                }
            }
        }
    }
}