#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System;
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
        public double timeNeeded = -1;
        public TaskState taskState;
        public string question;
        public int surveyPart;

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
            taskState = TaskState.Started;

            timeNeeded = -1;
            TaskStartTime = DateTime.Now;
        }

        public void FinishTask()
        {
            timeNeeded = (DateTime.Now - TaskStartTime).TotalMilliseconds;

            taskState = TaskState.Finished;
        }
    }
}