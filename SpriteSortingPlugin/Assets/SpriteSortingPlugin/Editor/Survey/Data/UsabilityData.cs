using System;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class UsabilityData
    {
        public int[] susAnswers = Array.ConvertAll(new int[10], i => -1);

        public float[] ratingAnswers = Array.ConvertAll(new float[3], i => 50f);
        public bool[] ratingAnswersChanged = new bool[3];

        public string missingCriteriaText = "";

        public int occuringError = -1;
        public string occuringErrorsText = "";

        public bool isMiscellaneous;
        public string miscellaneous = "";
    }
}