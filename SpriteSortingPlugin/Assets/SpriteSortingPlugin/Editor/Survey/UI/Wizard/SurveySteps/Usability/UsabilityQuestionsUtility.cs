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
            GUI.Label(questionLabelRect, label, isBold ? Styling.CenteredStyleBold : Styling.CenteredStyle);

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