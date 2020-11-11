using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting
{
    [CustomEditor(typeof(SizeSortingCriterionData))]
    public class SizeCriterionDataEditor : CriterionDataBaseEditor<SizeSortingCriterionData>
    {
        protected override void OnInspectorGuiInternal()
        {
            sortingCriterionData.isLargeSpritesInForeground = EditorGUILayout.ToggleLeft(
                "is Large Sprites In Foreground", sortingCriterionData.isLargeSpritesInForeground);
        }
    }
}