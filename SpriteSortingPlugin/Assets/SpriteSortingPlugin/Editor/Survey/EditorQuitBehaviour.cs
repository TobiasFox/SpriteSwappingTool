using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    [InitializeOnLoad]
    public class EditorQuitBehaviour
    {
        private static readonly string[] SurveyDataOutputPath = new string[]
        {
            "SurveyData"
        };

        static EditorQuitBehaviour()
        {
            EditorApplication.quitting += Quit;
        }

        private static void Quit()
        {
            DeleteCache();
        }

        private static void DeleteCache()
        {
            var surveyDataPath = Path.Combine(Application.temporaryCachePath, Path.Combine(SurveyDataOutputPath));

            try
            {
                Directory.Delete(surveyDataPath, true);
            }
            catch
            {
                // ignored
            }
        }
    }
}