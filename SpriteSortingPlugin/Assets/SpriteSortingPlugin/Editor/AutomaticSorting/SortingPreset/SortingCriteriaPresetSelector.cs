using UnityEditor.Presets;

namespace SpriteSortingPlugin.AutomaticSorting.SortingPreset
{
    public class SortingCriteriaPresetSelector : PresetSelectorReceiver
    {
        private SpriteSortingEditorWindow currentWindow;
        private Preset initialValues;
        private SortingCriteriaPreset currentPreset;

        public void Init(SpriteSortingEditorWindow window)
        {
            currentWindow = window;
        }

        public void ShowPresetSelector()
        {
            currentPreset = currentWindow.GenerateSortingCriteriaPreset();
            initialValues = new Preset(currentPreset);
            PresetSelector.ShowSelector(currentPreset, null, true, this);
        }

        public override void OnSelectionChanged(Preset selection)
        {
            if (selection != null)
            {
                selection.ApplyTo(currentPreset);
            }
            else
            {
                initialValues.ApplyTo(currentPreset);
            }

            currentWindow.UpdateSortingCriteriaFromPreset(currentPreset.Copy());
        }

        public override void OnSelectionClosed(Preset selection)
        {
            OnSelectionChanged(selection);
        }
    }
}