using System;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class UsabilityData
    {
        public int[] susAnswers = Array.ConvertAll(new int[10], i => -1);

        public string highlights = "";
        public string lowlights = "";
        
        public int[] ratingAnswers= Array.ConvertAll(new int[3], i => -1);

        public string missingCriteria = "";
        public string missingFunctionality = "";
        public string occuringErrors = "";
        public string miscellaneous = "";
    }
}