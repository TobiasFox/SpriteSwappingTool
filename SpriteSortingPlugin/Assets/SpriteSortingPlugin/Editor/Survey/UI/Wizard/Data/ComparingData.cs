using System;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class ComparingData
    {
        public double pluginUsageNeededTime;
        public double manualSortingNeededTime;

        public double[] sortingSuggestionNeededTimeArray= new double[2];

        // [SerializeField] private List<double> manualSortingNeededTimeList = new List<double>();
        // [SerializeField] private List<double> pluginUsageNeededTimeList = new List<double>();
        // [SerializeField] private List<string> manualSortingSavedSceneList = new List<string>();
        // [SerializeField] private List<string> pluginUsageSavedSceneList = new List<string>();
        //
        // public void AddMeasuredManualSortingTry(double neededTime, string savedScene)
        // {
        //     manualSortingNeededTimeList.Add(neededTime);
        //     manualSortingSavedSceneList.Add(savedScene);
        // }
        //
        // public void AddMeasuredPluginUsageSortingTry(double neededTime, string savedScene)
        // {
        //     pluginUsageNeededTimeList.Add(neededTime);
        //     pluginUsageSavedSceneList.Add(savedScene);
        // }
    }
}