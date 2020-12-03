using System;
using SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    [Serializable]
    public class SortingCriteriaPreset : ScriptableObject, ISerializationCallbackReceiver
    {
        public string[] jsonStrings;
        public string[] types;
        public SortingCriterionData[] SortingCriterionData { get; set; }

        public SortingCriteriaPreset Copy()
        {
            var clone = CreateInstance<SortingCriteriaPreset>();
            if (SortingCriterionData.Length <= 0)
            {
                return clone;
            }

            clone.SortingCriterionData = new SortingCriterionData[SortingCriterionData.Length];
            for (var i = 0; i < SortingCriterionData.Length; i++)
            {
                clone.SortingCriterionData[i] = SortingCriterionData[i].Copy();
            }

            return clone;
        }

        public void OnBeforeSerialize()
        {
            if (SortingCriterionData == null)
            {
                return;
            }

            var length = SortingCriterionData.Length;
            jsonStrings = new string[length];
            types = new string[length];
            for (var i = 0; i < length; i++)
            {
                var criterionData = SortingCriterionData[i];
                jsonStrings[i] = JsonUtility.ToJson(criterionData);
                types[i] = criterionData.GetType().FullName;
            }
        }

        public void OnAfterDeserialize()
        {
            SortingCriterionData = new SortingCriterionData[jsonStrings.Length];
            for (var i = 0; i < jsonStrings.Length; i++)
            {
                var data = CreateInstance(types[i]);
                JsonUtility.FromJsonOverwrite(jsonStrings[i], data);
                SortingCriterionData[i] = (SortingCriterionData) data;
            }
        }
    }
}