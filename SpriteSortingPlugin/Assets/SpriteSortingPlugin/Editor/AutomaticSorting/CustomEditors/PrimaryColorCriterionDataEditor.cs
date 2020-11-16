using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(PrimaryColorSortingCriterionData))]
    public class PrimaryColorCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private PrimaryColorSortingCriterionData PrimaryColorSortingCriterionData =>
            (PrimaryColorSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            PrimaryColorSortingCriterionData.backgroundColor = EditorGUILayout.ColorField("Background Color",
                PrimaryColorSortingCriterionData.backgroundColor);
            PrimaryColorSortingCriterionData.foregroundColor = EditorGUILayout.ColorField("Foreground Color",
                PrimaryColorSortingCriterionData.foregroundColor);
            
            //switch button
        }

        protected override string GetTitleName()
        {
            return "Primary Color";
        }
    }
}