using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(SizeSortingCriterionData))]
    public class SizeCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private SizeSortingCriterionData SizeSortingCriterionData => (SizeSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            SizeSortingCriterionData.isLargeSpriteInForeground = EditorGUILayout.ToggleLeft(
                "is large sprite in foreground", SizeSortingCriterionData.isLargeSpriteInForeground);
        }

        public override string GetTitleName()
        {
            return "Size";
        }
    }
}