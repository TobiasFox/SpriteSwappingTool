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
    public class PrimaryColorCriterionDataUIRepresentation : CriterionDataBaseUIRepresentation
    {
        private static readonly string[] ChannelNames = new string[] {"Red", "Green", "Blue"};

        private PrimaryColorSortingCriterionData PrimaryColorSortingCriterionData =>
            (PrimaryColorSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = "Primary Color";
            tooltip = UITooltipConstants.PrimaryColorTooltip;
        }

        protected override void OnInspectorGuiInternal()
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent("Channels", UITooltipConstants.PrimaryColorChannelsTooltip));

                if (GUILayout.Button("All",
                    GUILayout.Width(25f), GUILayout.ExpandWidth(false)))
                {
                    for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
                    {
                        PrimaryColorSortingCriterionData.activeChannels[i] = true;
                    }
                }

                if (GUILayout.Button("None", GUILayout.Width(40f), GUILayout.ExpandWidth(false)))
                {
                    for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
                    {
                        PrimaryColorSortingCriterionData.activeChannels[i] = false;
                    }
                }

                for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
                {
                    PrimaryColorSortingCriterionData.activeChannels[i] = GUILayout.Toggle(
                        PrimaryColorSortingCriterionData.activeChannels[i], ChannelNames[i], Styling.ButtonStyle,
                        GUILayout.ExpandWidth(true));
                }
            }

            PrimaryColorSortingCriterionData.foregroundColor = EditorGUILayout.ColorField(
                new GUIContent("Foreground Color", UITooltipConstants.PrimaryColorForegroundColorTooltip),
                PrimaryColorSortingCriterionData.foregroundColor);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("\u2191 Switch \u2193", GUILayout.Width(152)))
                {
                    var tempColour = PrimaryColorSortingCriterionData.backgroundColor;
                    PrimaryColorSortingCriterionData.backgroundColor = PrimaryColorSortingCriterionData.foregroundColor;
                    PrimaryColorSortingCriterionData.foregroundColor = tempColour;
                }

                GUILayout.FlexibleSpace();
            }

            PrimaryColorSortingCriterionData.backgroundColor = EditorGUILayout.ColorField(
                new GUIContent("Background Color", UITooltipConstants.PrimaryColorBackgroundColorTooltip),
                PrimaryColorSortingCriterionData.backgroundColor);
        }
    }
}