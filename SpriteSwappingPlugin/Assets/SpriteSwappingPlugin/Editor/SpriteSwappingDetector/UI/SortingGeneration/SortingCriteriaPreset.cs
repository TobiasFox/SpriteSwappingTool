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
using SpriteSwappingPlugin.SortingGeneration;
using SpriteSwappingPlugin.SortingGeneration.Data;
using UnityEngine;

namespace SpriteSwappingPlugin.SpriteSwappingDetector.UI.SortingGeneration
{
    [Serializable]
    public class SortingCriteriaPreset : ScriptableObject, ISerializationCallbackReceiver, ICloneable
    {
        [HideInInspector] public string[] jsonData;
        public SortingCriterionData[] sortingCriterionData;

        public object Clone()
        {
            var clone = CreateInstance<SortingCriteriaPreset>();
            if (sortingCriterionData.Length <= 0)
            {
                return clone;
            }

            clone.sortingCriterionData = new SortingCriterionData[sortingCriterionData.Length];
            for (var i = 0; i < sortingCriterionData.Length; i++)
            {
                clone.sortingCriterionData[i] = (SortingCriterionData) sortingCriterionData[i].Clone();
            }

            return clone;
        }

        public void OnBeforeSerialize()
        {
            SaveData();
        }

        public void OnAfterDeserialize()
        {
            LoadData();
        }

        private void SaveData()
        {
            var isValidForSaving = IsSortingCriteriaDataValidForSaving();
            if (!isValidForSaving)
            {
                return;
            }

            jsonData = new string[sortingCriterionData.Length];
            for (var i = 0; i < sortingCriterionData.Length; i++)
            {
                var criterionData = sortingCriterionData[i];
                jsonData[i] = JsonUtility.ToJson(criterionData);
            }
        }

        private void LoadData()
        {
            var isValidForLoading = IsSavedDataValidForLoading();
            if (!isValidForLoading)
            {
                return;
            }

            for (var i = 0; i < sortingCriterionData.Length; i++)
            {
                var data = CreateAppropriateCriterionData(sortingCriterionData[i].sortingCriterionType);
                JsonUtility.FromJsonOverwrite(jsonData[i], data);
                sortingCriterionData[i] = data;
            }
        }

        private SortingCriterionData CreateAppropriateCriterionData(SortingCriterionType sortingCriterionType)
        {
            switch (sortingCriterionType)
            {
                case SortingCriterionType.Containment:
                    return new ContainmentSortingCriterionData();
                case SortingCriterionType.PrimaryColor:
                    return new PrimaryColorSortingCriterionData();
                default:
                    return new DefaultSortingCriterionData();
            }
        }

        private bool IsSortingCriteriaDataValidForSaving()
        {
            if (sortingCriterionData == null)
            {
                return false;
            }

            foreach (var currentSortingCriterionData in sortingCriterionData)
            {
                if (currentSortingCriterionData == null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSavedDataValidForLoading()
        {
            if (jsonData == null || sortingCriterionData == null)
            {
                return false;
            }

            for (var i = 0; i < jsonData.Length; i++)
            {
                var json = jsonData[i];
                if (string.IsNullOrEmpty(json) || sortingCriterionData[i] == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}