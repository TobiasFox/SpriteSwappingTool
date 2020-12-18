using System.Collections.Generic;
using System.Text;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    public static class SurveyStepDataUtil
    {
        public static string GenerateSurveyStepsDataListJson(List<SurveyStepData> list, string listName)
        {
            if (list == null)
            {
                return "";
            }

            var surveyStepDataBuilder = new StringBuilder(",\"").Append(listName).Append("\":[");

            for (var i = 0; i < list.Count; i++)
            {
                var surveyStepDataJson = list[i].GenerateJson();
                surveyStepDataBuilder.Append(surveyStepDataJson);

                if (i < list.Count - 1)
                {
                    surveyStepDataBuilder.Append(",");
                }
            }

            surveyStepDataBuilder.Append("]");
            return surveyStepDataBuilder.ToString();
        }
    }
}