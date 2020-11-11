using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public class SortingCriterionData : ScriptableObject
    {
        public bool isActive;
        public bool isExpanded;
        public int priority;
    }
}