using UnityEditor.Presets;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public class SortingCriteriaPresetSelector : PresetSelectorReceiver
    {
        private AutoSortingOptionsUI currentWindow;
        private Preset initialValues;
        private SortingCriteriaPreset currentPreset;

        public void Init(AutoSortingOptionsUI autoSortingOptionsUI)
        {
            currentWindow = autoSortingOptionsUI;
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

            var presetClone = (SortingCriteriaPreset) currentPreset.Clone();
            currentWindow.UpdateSortingCriteriaFromPreset(presetClone);
        }
    }
}