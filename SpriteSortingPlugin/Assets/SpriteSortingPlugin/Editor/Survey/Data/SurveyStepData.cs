using System;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class SurveyStepData
    {
        public bool isFinished;
        public bool isStarted;

        [SerializeField] private string type;

        public string DataType => type;

        public SurveyStepData()
        {
            type = GetType().Name;
        }
    }
}