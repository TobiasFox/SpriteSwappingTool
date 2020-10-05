using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SortingComponent
    {
        public readonly SpriteRenderer spriteRenderer;
        public readonly SortingGroup outmostSortingGroup;

        public SortingComponent(SpriteRenderer spriteRenderer, SortingGroup outmostSortingGroup = null)
        {
            this.spriteRenderer = spriteRenderer;
            this.outmostSortingGroup = outmostSortingGroup;
        }

        public int CurrentSortingOrder
        {
            get
            {
                if (outmostSortingGroup != null)
                {
                    return outmostSortingGroup.sortingOrder;
                }

                return spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
            }
        }

        public int CurrentSortingLayer
        {
            get
            {
                if (outmostSortingGroup != null)
                {
                    return outmostSortingGroup.sortingLayerID;
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
            return spriteRenderer == other.spriteRenderer && outmostSortingGroup == other.outmostSortingGroup;
        }

        public override int GetHashCode()
        {
            var hashcode = spriteRenderer.GetHashCode();
            if (outmostSortingGroup != null)
            {
                hashcode ^= outmostSortingGroup.GetHashCode();
            }

            return hashcode;
        }

        public override string ToString()
        {
            return "SortingComponent[" + spriteRenderer.name + ", " + CurrentSortingLayer + ", " + CurrentSortingOrder +
                   ", SG:" + (outmostSortingGroup != null) + "]";
        }
    }
}