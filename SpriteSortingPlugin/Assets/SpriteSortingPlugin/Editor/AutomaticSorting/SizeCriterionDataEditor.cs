using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting
{
    [CustomEditor(typeof(SizeSortingCriterionData))]
    public class SizeCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private SizeSortingCriterionData SizeSortingCriterionData => (SizeSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            SizeSortingCriterionData.isLargeSpritesInForeground = EditorGUILayout.ToggleLeft(
                "is Large Sprites In Foreground", SizeSortingCriterionData.isLargeSpritesInForeground);
        }

        protected override string GetTitleName()
        {
            return "Size";
        }
    }
}