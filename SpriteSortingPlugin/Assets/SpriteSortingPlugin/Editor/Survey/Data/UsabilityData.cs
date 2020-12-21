using System;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class UsabilityData
    {
        public int[] susAnswers = Array.ConvertAll(new int[10], i => -1);

        public string highlights = "";
        public string lowlights = "";

        //TODO float or int?
        public float[] ratingAnswers = Array.ConvertAll(new float[3], i => 50f);

        public string missingCriteriaText = "";

        public bool isOccuringError;
        public string occuringErrorsText = "";

        public bool isMiscellaneous;
        public string miscellaneous = "";
    }
}