using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SortingComponent
    {
        private readonly SpriteRenderer originSpriteRenderer;
        private readonly SortingGroup outmostSortingGroup;

        protected HashSet<int> overlappingSortingComponents;

        public SpriteRenderer OriginSpriteRenderer => originSpriteRenderer;
        public SortingGroup OutmostSortingGroup => outmostSortingGroup;

        public SortingComponent(SpriteRenderer originSpriteRenderer, SortingGroup outmostSortingGroup = null)
        {
            this.originSpriteRenderer = originSpriteRenderer;
            this.outmostSortingGroup = outmostSortingGroup;
        }

        public SortingComponent(SortingComponent otherSortingComponent)
        {
            this.originSpriteRenderer = otherSortingComponent.originSpriteRenderer;
            this.outmostSortingGroup = otherSortingComponent.outmostSortingGroup;

            if (otherSortingComponent.overlappingSortingComponents != null)
            {
                overlappingSortingComponents = new HashSet<int>();
                foreach (var hashCode in otherSortingComponent.overlappingSortingComponents)
                {
                    overlappingSortingComponents.Add(hashCode);
                }
            }
        }

        public int OriginSortingOrder
        {
            get
            {
                if (OutmostSortingGroup != null)
                {
                    return OutmostSortingGroup.sortingOrder;
                }

                return OriginSpriteRenderer != null ? OriginSpriteRenderer.sortingOrder : 0;
            }
        }

        public int OriginSortingLayer
        {
            get
            {
                if (OutmostSortingGroup != null)
                {
                    return OutmostSortingGroup.sortingLayerID;
                }

                return OriginSpriteRenderer != null ? OriginSpriteRenderer.sortingLayerID : 0;
            }
        }

        public int GetInstanceId()
        {
            return OutmostSortingGroup != null
                ? OutmostSortingGroup.GetInstanceID()
                : OriginSpriteRenderer.GetInstanceID();
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
            return OriginSpriteRenderer == other.OriginSpriteRenderer &&
                   OutmostSortingGroup == other.OutmostSortingGroup;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashcode = 397 * originSpriteRenderer.GetHashCode();
                if (outmostSortingGroup != null)
                {
                    hashcode ^= outmostSortingGroup.GetHashCode();
                }

                return hashcode;
            }
        }

        public void AddOverlappingSortingComponent(SortingComponent sortingComponent)
        {
            if (overlappingSortingComponents == null)
            {
                overlappingSortingComponents = new HashSet<int>();
            }

            overlappingSortingComponents.Add(sortingComponent.GetHashCode());
        }

        public bool IsOverlapping(SortingComponent sortingComponent)
        {
            if (overlappingSortingComponents == null)
            {
                return false;
            }

            return overlappingSortingComponents.Contains(sortingComponent.GetHashCode());
        }

        public override string ToString()
        {
            return "SortingComponent[" + OriginSpriteRenderer.name + ", " + OriginSortingLayer + ", " +
                   OriginSortingOrder + ", SG:" + (OutmostSortingGroup != null) + "]";
        }
    }
}