using System;
using SpriteSortingPlugin.Survey.UI.Wizard;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class SurveyStepData
    {
        public SurveyFinishState finishState;
        public bool isFinished;
        public bool isStarted;

        [SerializeField] private string type;

        public string DataType => type;

        public SurveyStepData()
        {
            type = GetType().Name;
        }

        public virtual string GenerateJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}