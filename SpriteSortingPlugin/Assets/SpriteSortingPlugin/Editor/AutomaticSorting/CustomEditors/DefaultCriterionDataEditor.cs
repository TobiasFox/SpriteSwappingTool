using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(DefaultSortingCriterionData))]
    public class DefaultCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private DefaultSortingCriterionData DefaultSortingCriterionData =>
            (DefaultSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            DefaultSortingCriterionData.isSortingInForeground = EditorGUILayout.ToggleLeft(
                DefaultSortingCriterionData.foregroundSortingName, DefaultSortingCriterionData.isSortingInForeground);
        }

        public override string GetTitleName()
        {
            return DefaultSortingCriterionData.criterionName;
        }
    }
}