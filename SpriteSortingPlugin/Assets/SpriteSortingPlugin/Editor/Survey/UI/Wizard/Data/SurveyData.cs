using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class SurveyData
    {
        public UserData userData;
        public GeneralQuestionsData generalQuestionsData;
        public UsabilityData usabilityData;

        public int currentProgress;
        public int totalProgress;

        public List<SurveyStepData> SurveyStepDataList { get; set; }

        public string SaveFolder =>
            userData.id.ToString() + Path.DirectorySeparatorChar + "progress" + currentProgress;

        public SurveyData()
        {
            userData = new UserData();
            generalQuestionsData = new GeneralQuestionsData();
            usabilityData = new UsabilityData();
            SurveyStepDataList = new List<SurveyStepData>();
        }

        public string GenerateJson()
        {
            var json = JsonUtility.ToJson(this);
            var surveyStepDataBuilder =
                SurveyStepDataUtil.GenerateSurveyStepsDataListJson(SurveyStepDataList, nameof(SurveyStepDataList));

            json = json.Insert(json.Length - 1, surveyStepDataBuilder);
            return json;
        }
    }
}