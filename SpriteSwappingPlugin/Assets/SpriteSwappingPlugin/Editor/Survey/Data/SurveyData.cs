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
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpriteSwappingPlugin.Survey.Data
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

        public Guid UserId => ownGuid;

        private Guid ownGuid;
        [SerializeField] private string userId;

        public SurveyData()
        {
            generalQuestionsData = new GeneralQuestionsData();
            usabilityData = new UsabilityData();
        }

        public void LoadGuid()
        {
            ownGuid = string.IsNullOrEmpty(userId) ? Guid.NewGuid() : Guid.Parse(userId);
            userId = ownGuid.ToString();
        }
    }
}