using System;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
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