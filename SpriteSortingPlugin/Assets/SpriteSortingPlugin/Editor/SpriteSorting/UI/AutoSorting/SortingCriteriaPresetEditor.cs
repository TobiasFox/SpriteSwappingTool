using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    [CustomEditor(typeof(SortingCriteriaPreset))]
    public class SortingCriteriaPresetEditor : Editor
    {
        private CriterionDataBaseUIRepresentation<SortingCriterionData>[] criterionDataBaseUIRepresentations;

        private void Awake()
        {
            var preset = (SortingCriteriaPreset) target;

            criterionDataBaseUIRepresentations =
                new CriterionDataBaseUIRepresentation<SortingCriterionData>[preset.sortingCriterionData.Length];
            for (var i = 0; i < preset.sortingCriterionData.Length; i++)
            {
                var sortingCriterionData = preset.sortingCriterionData[i];
                var criterionDataBaseEditor =
                    SortingCriterionDataUIRepresentationFactory.CreateUIRepresentation(sortingCriterionData, true);
                criterionDataBaseEditor.Initialize(sortingCriterionData, true);
                criterionDataBaseUIRepresentations[i] = criterionDataBaseEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            foreach (var criteriaEditor in criterionDataBaseUIRepresentations)
            {
                criteriaEditor.OnInspectorGUI();
            }
        }
    }
}