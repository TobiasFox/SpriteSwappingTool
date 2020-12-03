using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    [CustomEditor(typeof(SortingCriteriaPreset))]
    public class SortingCriteriaPresetEditor : Editor
    {
        private Editor[] sortingCriteriaEditors;

        private void Awake()
        {
            var preset = (SortingCriteriaPreset) target;

            sortingCriteriaEditors = new Editor[preset.SortingCriterionData.Length];
            for (var i = 0; i < preset.SortingCriterionData.Length; i++)
            {
                var sortingCriterionData = preset.SortingCriterionData[i];
                var specificEditor = CreateEditor(sortingCriterionData);
                var criterionDataBaseEditor = (CriterionDataBaseEditor<SortingCriterionData>) specificEditor;
                criterionDataBaseEditor.Initialize(sortingCriterionData, true);
                sortingCriteriaEditors[i] = criterionDataBaseEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            foreach (var criteriaEditor in sortingCriteriaEditors)
            {
                criteriaEditor.OnInspectorGUI();
            }
        }

        private void OnDestroy()
        {
            foreach (var sortingCriteriaEditor in sortingCriteriaEditors)
            {
                DestroyImmediate(sortingCriteriaEditor);
            }
        }
    }
}