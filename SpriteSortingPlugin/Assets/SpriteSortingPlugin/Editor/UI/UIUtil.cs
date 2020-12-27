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

using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.UI
{
    public static class UIUtil
    {
        public static bool DrawFoldoutBoolContent(bool isActive, GUIContent content)
        {
            var foldoutBoolContentRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);

            var labelRect = foldoutBoolContentRect;
            labelRect.xMin += 15f;
            labelRect.width = 136;

            var foldoutRect = foldoutBoolContentRect;
            foldoutRect.xMax = 13;

            var toggleRect = foldoutBoolContentRect;
            toggleRect.xMin = labelRect.xMax + 2;

            isActive = GUI.Toggle(foldoutRect, isActive, GUIContent.none, EditorStyles.foldout);

            EditorGUI.LabelField(labelRect, content);

            isActive = GUI.Toggle(toggleRect, isActive, GUIContent.none);

            return isActive;
        }

        public static void DrawHorizontalLine(bool isBig = false)
        {
            var rect = GUILayoutUtility.GetRect(1f, isBig ? 1.5f : 1.25f);

            // Splitter rect should be full-width
            // rect.xMin = 0f;
            // rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.DrawRect(rect, Styling.HorizontalLineColor);
        }
    }
}