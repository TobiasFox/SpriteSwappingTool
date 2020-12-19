﻿using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class SortingTaskData
    {
        private static readonly string[] SceneFolderPath = new string[]
        {
            "Assets", "SpriteSortingPlugin", "Editor", "Survey", "Scenes"
        };

        public static readonly string[] ModifiedSceneFolderPath = new string[]
        {
            "Assets", "SpriteSortingPlugin", "Editor", "Survey", "Scenes", "Modified"
        };

        public string sceneName;
        public bool isTaskStarted;
        public bool isTaskFinished;
        public double timeNeeded = -1;

        public DateTime TaskStartTime { get; set; }
        public Scene LoadedScene { get; set; }
        public string FullScenePathAndName { get; private set; }

        public string FullModifiedScenePath
        {
            get
            {
                var directory = Path.Combine(ModifiedSceneFolderPath);
                Directory.CreateDirectory(directory);

                return Path.Combine(directory, "Modified_" + sceneName);
            }
        }

        public void SetSceneName(string sceneName)
        {
            this.sceneName = sceneName;
            FullScenePathAndName = Path.Combine(Path.Combine(SceneFolderPath), sceneName);
        }

        public void StartTask()
        {
            isTaskStarted = true;
            isTaskFinished = false;

            timeNeeded = -1;
            TaskStartTime = DateTime.Now;
        }

        public void FinishTask()
        {
            timeNeeded = (DateTime.Now - TaskStartTime).TotalMilliseconds;

            isTaskStarted = false;
            isTaskFinished = true;
        }

        public void CancelTask()
        {
            isTaskStarted = false;
            isTaskFinished = false;
        }
    }
}