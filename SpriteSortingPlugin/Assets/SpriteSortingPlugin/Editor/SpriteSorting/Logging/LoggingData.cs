﻿#region license

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
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.Logging
{
    [Serializable]
    public class LoggingData
    {
        public List<SortingSuggestionLoggingData> sortingSuggestionLoggingDataList =
            new List<SortingSuggestionLoggingData>();

        private int currentIndex = -1;
        private bool isCurrentLoggingDataActive;
        public bool isFirstSortingQuestion = true;

        [SerializeField] private FoundGlitchStatistic[] foundGlitchStatistics = new FoundGlitchStatistic[]
        {
            new FoundGlitchStatistic(),
            new FoundGlitchStatistic()
        };

        public FoundGlitchStatistic CurrentFoundGlitchStatistic =>
            foundGlitchStatistics[isFirstSortingQuestion ? 0 : 1];

        private string guid = Guid.NewGuid().ToString();
        public string UniqueFileName => $"LoggingData_{guid}.json";

        public bool IsCurrentLoggingDataActive => isCurrentLoggingDataActive;

        public SortingSuggestionLoggingData GetCurrentSuggestionLoggingData()
        {
            if (currentIndex < 0)
            {
                return null;
            }

            if (!isCurrentLoggingDataActive)
            {
                return null;
            }

            return sortingSuggestionLoggingDataList[currentIndex];
        }

        public void ClearLastLoggingData()
        {
            if (currentIndex < 0)
            {
                return;
            }

            if (!isCurrentLoggingDataActive)
            {
                return;
            }

            sortingSuggestionLoggingDataList.RemoveAt(currentIndex);
            currentIndex--;
            isCurrentLoggingDataActive = false;
        }

        public void AddSortingOrderSuggestionLoggingData(SortingSuggestionLoggingData data)
        {
            data.question = GeneralData.questionNumberForLogging;
            currentIndex++;
            sortingSuggestionLoggingDataList.Add(data);
            isCurrentLoggingDataActive = true;
        }

        public void ConfirmSortingOrder()
        {
            isCurrentLoggingDataActive = false;
        }
    }

    [Serializable]
    public class FoundGlitchStatistic
    {
        public int totalFoundGlitches;
        public int totalClearedGlitches;
        public int totalConfirmedGlitches;
        public int question;
    }
}