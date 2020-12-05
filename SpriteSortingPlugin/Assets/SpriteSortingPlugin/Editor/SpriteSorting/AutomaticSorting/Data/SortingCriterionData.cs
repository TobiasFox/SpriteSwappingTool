using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    public abstract class SortingCriterionData : ScriptableObject
    {
        public bool isActive;
        public float priority = 1;
        [HideInInspector] public bool isExpanded;
        [HideInInspector] public bool isAddedToEditorList;

        protected void CopyDataTo(SortingCriterionData copy)
        {
            copy.isActive = isActive;
            copy.priority = priority;
            copy.isExpanded = isExpanded;
            copy.isAddedToEditorList = isAddedToEditorList;
        }

        public abstract SortingCriterionData Copy();
    }
}