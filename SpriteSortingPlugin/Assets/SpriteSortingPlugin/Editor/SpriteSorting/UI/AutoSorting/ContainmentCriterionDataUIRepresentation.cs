﻿using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public class ContainmentCriterionDataUIRepresentation : CriterionDataBaseUIRepresentation<SortingCriterionData>
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
                    EditorGUI.BeginChangeCheck();
                    ContainmentSortingCriterionData.alphaThreshold =
                        EditorGUILayout.FloatField(
                            new GUIContent("Alpha threshold", UITooltipConstants.ContainmentAlphaThresholdTooltip),
                            ContainmentSortingCriterionData.alphaThreshold);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ContainmentSortingCriterionData.alphaThreshold =
                            Mathf.Clamp01(ContainmentSortingCriterionData.alphaThreshold);
                    }
                }
            }
        }
    }
}