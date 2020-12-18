using System;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
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