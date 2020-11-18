using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.SortingPreset
{
    public class SortingCriteriaPreset : ScriptableObject
    {
        public SortingCriterionData[] sortingCriterionData;

        public SortingCriteriaPreset Copy()
        {
            var clone = CreateInstance<SortingCriteriaPreset>();
            if (sortingCriterionData.Length <= 0)
            {
                return clone;
            }

            clone.sortingCriterionData = new SortingCriterionData[sortingCriterionData.Length];
            for (var i = 0; i < sortingCriterionData.Length; i++)
            {
                clone.sortingCriterionData[i] = sortingCriterionData[i].Copy();
            }

            return clone;
        }
    }
}