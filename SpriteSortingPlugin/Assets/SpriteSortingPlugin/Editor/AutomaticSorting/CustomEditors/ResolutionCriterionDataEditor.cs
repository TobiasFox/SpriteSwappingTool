using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(ResolutionSortingCriterionData))]
    public class ResolutionCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private ResolutionSortingCriterionData ResolutionSortingCriterionData => (ResolutionSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            ResolutionSortingCriterionData.isSpriteWithHigherResolutionInForeground = EditorGUILayout.ToggleLeft(
                "is sprite with higher pixel density in foreground", ResolutionSortingCriterionData.isSpriteWithHigherResolutionInForeground);
        }

        protected override string GetTitleName()
        {
            return "Sprite Resolution";
        }
    }
}