﻿using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(ContainmentSortingCriterionData))]
    public class ContainmentCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
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
            ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent("Is contained Sprite in foreground",
                    UITooltipConstants.ContainmentEncapsulatedSpriteInForegroundTooltip),
                ContainmentSortingCriterionData.isSortingEnclosedSpriteInForeground);

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