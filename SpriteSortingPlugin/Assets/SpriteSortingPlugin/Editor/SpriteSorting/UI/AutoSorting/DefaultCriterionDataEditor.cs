using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    [CustomEditor(typeof(DefaultSortingCriterionData))]
    public class DefaultCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            title = DefaultSortingCriterionData.criterionName;
            tooltip = DefaultSortingCriterionData.criterionTooltip;
        }

        protected override void OnInspectorGuiInternal()
        {
            DefaultSortingCriterionData.isSortingInForeground = EditorGUILayout.ToggleLeft(
                new GUIContent(DefaultSortingCriterionData.foregroundSortingName,
                    DefaultSortingCriterionData.foregroundSortingTooltip),
                DefaultSortingCriterionData.isSortingInForeground);
        }
    }
}