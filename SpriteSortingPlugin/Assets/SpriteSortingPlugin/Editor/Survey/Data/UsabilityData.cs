using System;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class UsabilityData
    {
        public int[] susAnswers = Array.ConvertAll(new int[10], i => -1);

        public string highlights = "";
        public string lowlights = "";

        public int[] ratingAnswers = Array.ConvertAll(new int[3], i => 50);

        public string missingCriteriaText = "";

        public bool isOccuringError;
        public string occuringErrorsText = "";

        public bool isMiscellaneous;
        public string miscellaneous = "";
    }
}