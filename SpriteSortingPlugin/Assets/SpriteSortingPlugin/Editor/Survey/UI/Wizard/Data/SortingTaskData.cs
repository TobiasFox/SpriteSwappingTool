using System;
using UnityEngine.SceneManagement;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class SortingTaskData
    {
        [NonSerialized] public string scenePathAndName;
        public bool isTaskStarted;
        public bool isTaskFinished;
        public double timeNeeded = -1;

        public DateTime TaskStartTime { get; set; }
        public Scene LoadedScene { get; set; }

        public SortingTaskData(string scenePathAndName)
        {
            this.scenePathAndName = scenePathAndName;
        }

        public void CalculateAndSetTimeNeeded()
        {
            timeNeeded = (DateTime.Now - TaskStartTime).TotalMilliseconds;
        }

        public void ResetTimeNeeded()
        {
            timeNeeded = -1;
        }
    }
}