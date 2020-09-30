using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct SortingComponent
    {
        public readonly SpriteRenderer spriteRenderer;
        public readonly SortingGroup sortingGroup;

        public SortingComponent(SpriteRenderer spriteRenderer, SortingGroup sortingGroup = null)
        {
            this.spriteRenderer = spriteRenderer;
            this.sortingGroup = sortingGroup;
        }

        public int CurrentSortingOrder
        {
            get
            {
                if (sortingGroup != null)
                {
                    return sortingGroup.sortingOrder;
                }

                return spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
            }
        }

        public int CurrentSortingLayer
        {
            get
            {
                if (sortingGroup != null)
                {
                    return sortingGroup.sortingLayerID;
                }

                return spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SortingComponent otherSortingComponent))
            {
                return false;
            }

            return Equals(otherSortingComponent);
        }

        public bool Equals(SortingComponent other)
        {
            return spriteRenderer == other.spriteRenderer && sortingGroup == other.sortingGroup;
        }

        public override int GetHashCode()
        {
            var hashcode = spriteRenderer.GetHashCode();
            if (sortingGroup != null)
            {
                hashcode ^= sortingGroup.GetHashCode();
            }

            return hashcode;
        }

        public override string ToString()
        {
            return "SortingComponent[" + spriteRenderer.name + ", " + CurrentSortingLayer + ", " + CurrentSortingOrder +
                   ", SG:" + (sortingGroup != null) + "]";
        }
    }
}