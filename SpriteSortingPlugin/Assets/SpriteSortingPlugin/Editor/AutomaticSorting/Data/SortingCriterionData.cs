using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.Data
{
    public class SortingCriterionData : ScriptableObject
    {
        public bool isActive;
        public bool isExpanded;
        public int priority = 1;
    }
}