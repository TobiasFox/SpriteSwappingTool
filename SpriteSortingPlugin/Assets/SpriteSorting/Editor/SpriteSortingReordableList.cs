using System.Collections.Generic;
using UnityEngine;

namespace SpriteSorting
{
    public class SpriteSortingReordableList : ScriptableObject
    {
        public List<ReordableSpriteSortingItem> reordableSpriteSortingItems = new List<ReordableSpriteSortingItem>();
    }
}