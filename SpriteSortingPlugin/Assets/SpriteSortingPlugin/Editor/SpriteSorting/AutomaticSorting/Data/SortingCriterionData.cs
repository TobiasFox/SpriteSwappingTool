using System;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    [Serializable]
    public class SortingCriterionData : ICloneable
    {
        public bool isActive;
        public float priority = 1;
        [HideInInspector] public bool isExpanded;
        [HideInInspector] public bool isAddedToEditorList;
        public SortingCriterionType sortingCriterionType;

        protected void CopyDataTo(SortingCriterionData copy)
        {
            copy.isActive = isActive;
            copy.priority = priority;
            copy.isExpanded = isExpanded;
            copy.isAddedToEditorList = isAddedToEditorList;
            copy.sortingCriterionType = sortingCriterionType;
        }

        public virtual object Clone()
        {
            var clone = new SortingCriterionData();
            CopyDataTo(clone);
            return clone;
        }
    }
}