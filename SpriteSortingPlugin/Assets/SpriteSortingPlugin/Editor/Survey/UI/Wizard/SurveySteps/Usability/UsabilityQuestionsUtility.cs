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

using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public static class UsabilityQuestionsUtility
    {
        private static readonly string[] RatingAnswers = new string[] {"1", "2", "3", "4", "5"};

        public static void DrawRatingHeader(float questionWidthPercentage, string label, string from, string to,
            bool isBold = true)
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false,
                EditorGUIUtility.singleLineHeight * 2));

            var questionLabelRect = rect;
            questionLabelRect.width *= questionWidthPercentage;
            var centeredStyleBold = isBold ? Styling.CenteredStyleBold : Styling.CenteredStyle;
            centeredStyleBold = new GUIStyle(centeredStyleBold) {wordWrap = true};
            GUI.Label(questionLabelRect, label, centeredStyleBold);

            rect.xMin = rect.x + questionLabelRect.xMax;

            var labelRect1 = rect;
            labelRect1.width /= 2f;
            var labelStyle = new GUIStyle(Styling.LabelWrapStyle) {alignment = TextAnchor.MiddleLeft};
            if (isBold)
            {
                labelStyle.fontStyle = FontStyle.Bold;
            }

            GUI.Label(labelRect1, from, labelStyle);

            var labelRect2 = rect;
            labelRect2.xMin = labelRect1.xMax;
            labelStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(labelRect2, to, labelStyle);
        }

        public static int DrawSingleRatingQuestion(int result, float questionWidthPercentage, int questionNumber,
            string questionText, bool isDisplayingTooltip = true)
        {
            EditorGUI.indentLevel++;
            var entireQuestionRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false,
                EditorGUIUtility.singleLineHeight * 3));
            EditorGUI.indentLevel--;

            var questionLabelRect = entireQuestionRect;
            questionLabelRect.width *= questionWidthPercentage;
            var labelWrapStyle = new GUIStyle(Styling.LabelWrapStyle) {alignment = TextAnchor.MiddleCenter};

            var questionNumberRect = questionLabelRect;
            questionNumberRect.yMax -= EditorGUIUtility.singleLineHeight * 2;
            GUI.Label(questionNumberRect, questionNumber + ".", labelWrapStyle);

            var questionRect = questionLabelRect;
            questionRect.yMin += EditorGUIUtility.singleLineHeight;
            GUI.Label(questionRect,
                new GUIContent(questionText, isDisplayingTooltip ? questionNumber + ". " + questionText : ""),
                labelWrapStyle);


            var selectionGrid = entireQuestionRect;
            selectionGrid.xMin = selectionGrid.x + questionLabelRect.width;
            result = GUI.SelectionGrid(selectionGrid, result, RatingAnswers, RatingAnswers.Length);

            return result;
        }
    }
}