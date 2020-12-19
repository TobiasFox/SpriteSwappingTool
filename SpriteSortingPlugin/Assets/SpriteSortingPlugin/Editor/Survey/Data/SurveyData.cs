using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class SurveyData
    {
        public GeneralQuestionsData generalQuestionsData;
        public UsabilityData usabilityData;

        public int currentProgress;
        public int totalProgress;

        public List<SurveyStepData> SurveyStepDataList { get; set; }

        public string SaveFolder => Path.Combine(userId, "progress" + currentProgress);
        // UserId.ToString() + Path.DirectorySeparatorChar + "progress" + currentProgress;

        public string ResultSaveFolder => Path.Combine(userId, "result");
        // UserId.ToString() + Path.DirectorySeparatorChar + "result";

        public Guid UserId { get; } = Guid.NewGuid();

        [SerializeField] private string userId;

        public SurveyData()
        {
            generalQuestionsData = new GeneralQuestionsData();
            usabilityData = new UsabilityData();
            SurveyStepDataList = new List<SurveyStepData>();
            userId = UserId.ToString();
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