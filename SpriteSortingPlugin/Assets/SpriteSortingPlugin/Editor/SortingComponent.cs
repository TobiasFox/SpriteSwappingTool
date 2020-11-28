using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SortingComponent
    {
        public readonly SpriteRenderer spriteRenderer;
        public readonly SortingGroup sortingGroup;

        private HashSet<int> overlappingSortingComponents;

        public SortingComponent(SpriteRenderer spriteRenderer, SortingGroup sortingGroup = null)
        {
            this.spriteRenderer = spriteRenderer;
            this.sortingGroup = sortingGroup;
        }

        public SortingComponent(SortingComponent otherSortingComponent)
        {
            this.spriteRenderer = otherSortingComponent.spriteRenderer;
            this.sortingGroup = otherSortingComponent.sortingGroup;

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
                if (sortingGroup != null)
                {
                    return sortingGroup.sortingOrder;
                }

                return spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
            }
        }

        public int OriginSortingLayer
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

        public int GetInstanceId()
        {
            return sortingGroup != null
                ? sortingGroup.GetInstanceID()
                : spriteRenderer.GetInstanceID();
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
            return spriteRenderer == other.spriteRenderer &&
                   sortingGroup == other.sortingGroup;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashcode = 397 * spriteRenderer.GetHashCode();
                if (sortingGroup != null)
                {
                    hashcode ^= sortingGroup.GetHashCode();
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
            return "SortingComponent[" + spriteRenderer.name + ", " + OriginSortingLayer + ", " +
                   OriginSortingOrder + ", SG:" + (sortingGroup != null) + "]";
        }
    }
}