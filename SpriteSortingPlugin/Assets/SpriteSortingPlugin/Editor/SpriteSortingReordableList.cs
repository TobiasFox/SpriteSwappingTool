using System.Collections.Generic;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class SpriteSortingReordableList : ScriptableObject
    {
        public List<OverlappingItem> reordableSpriteSortingItems = new List<OverlappingItem>();
    }
}