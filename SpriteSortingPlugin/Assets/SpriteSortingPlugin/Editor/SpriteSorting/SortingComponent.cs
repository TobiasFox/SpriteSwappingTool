using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin.SpriteSorting
{
    [Serializable]
    public class SortingComponent : ISerializationCallbackReceiver
    {
        private SpriteRenderer spriteRenderer;
        private SortingGroup sortingGroup;
        private HashSet<int> overlappingSortingComponents;
        private List<int> serializedOverlappingSortingComponents;

        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public SortingGroup SortingGroup => sortingGroup;

        public SortingComponent(SpriteRenderer spriteRenderer, SortingGroup sortingGroup = null)
        {
            this.spriteRenderer = spriteRenderer;
            this.sortingGroup = sortingGroup;
        }

        public SortingComponent(SortingComponent otherSortingComponent) : this(otherSortingComponent.SpriteRenderer,
            otherSortingComponent.sortingGroup)
        {
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
                if (SortingGroup != null)
                {
                    return SortingGroup.sortingOrder;
                }

                return SpriteRenderer != null ? SpriteRenderer.sortingOrder : 0;
            }
        }

        public int OriginSortingLayer
        {
            get
            {
                if (SortingGroup != null)
                {
                    return SortingGroup.sortingLayerID;
                }

                return SpriteRenderer != null ? SpriteRenderer.sortingLayerID : 0;
            }
        }

        public int GetInstanceId()
        {
            return SortingGroup != null
                ? SortingGroup.GetInstanceID()
                : SpriteRenderer.GetInstanceID();
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
            return SpriteRenderer == other.SpriteRenderer &&
                   SortingGroup == other.SortingGroup;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashcode = 397 * SpriteRenderer.GetHashCode();
                if (SortingGroup != null)
                {
                    hashcode ^= SortingGroup.GetHashCode();
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

        public void OnBeforeSerialize()
        {
            serializedOverlappingSortingComponents = new List<int>(overlappingSortingComponents);
        }

        public void OnAfterDeserialize()
        {
            overlappingSortingComponents = new HashSet<int>(serializedOverlappingSortingComponents);
        }
    }
}