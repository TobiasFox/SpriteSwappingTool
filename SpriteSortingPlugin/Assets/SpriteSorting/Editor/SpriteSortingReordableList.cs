using System.Collections.Generic;
using UnityEngine;

namespace SpriteSorting
{
    public class SpriteSortingReordableList : ScriptableObject
    {
        public List<OverlappingItem> reordableSpriteSortingItems = new List<OverlappingItem>();
    }
}