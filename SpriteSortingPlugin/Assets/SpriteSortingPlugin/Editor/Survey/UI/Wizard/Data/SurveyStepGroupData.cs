using System;
using System.Collections.Generic;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class SurveyStepGroupData : SurveyStepData
    {
        public List<SurveyStepData> SurveyStepsData { get; set; }

        public override string GenerateJson()
        {
            var baseJson = base.GenerateJson();
            var surveyStepDataJson =
                SurveyStepDataUtil.GenerateSurveyStepsDataListJson(SurveyStepsData, nameof(SurveyStepsData));

            var json = baseJson.Insert(baseJson.Length - 1, surveyStepDataJson);
            return json;
        }
    }
}