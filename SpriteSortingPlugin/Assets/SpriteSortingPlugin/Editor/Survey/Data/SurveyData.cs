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
        public List<SortingTaskData> sortingTaskDataList;

        public string SaveFolder => Path.Combine(userId, "progress" + currentProgress);

        public string ResultSaveFolder => Path.Combine(userId, "result");

        public Guid UserId { get; } = Guid.NewGuid();

        [SerializeField] private string userId;

        public SurveyData()
        {
            generalQuestionsData = new GeneralQuestionsData();
            usabilityData = new UsabilityData();
            userId = UserId.ToString();
        }
    }
}