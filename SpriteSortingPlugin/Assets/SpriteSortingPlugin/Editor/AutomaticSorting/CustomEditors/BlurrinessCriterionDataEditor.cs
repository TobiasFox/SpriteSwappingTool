using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(BlurrinessSortingCriterionData))]
    public class BlurrinessCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private BlurrinessSortingCriterionData BlurrinessSortingCriterionData =>
            (BlurrinessSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            BlurrinessSortingCriterionData.isMoreBlurrySpriteInForeground = EditorGUILayout.ToggleLeft(
                "is more blurry sprite in foreground", BlurrinessSortingCriterionData.isMoreBlurrySpriteInForeground);
        }

        public override string GetTitleName()
        {
            return "Sprite Blurriness";
        }
    }
}