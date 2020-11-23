using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSortingPlugin
{
    [Serializable]
    public class SortingComponent
    {
        private SpriteRenderer originSpriteRenderer;
        private SortingGroup outmostSortingGroup;

        public SpriteRenderer OriginSpriteRenderer => originSpriteRenderer;
        public SortingGroup OutmostSortingGroup => outmostSortingGroup;

        public SortingComponent(SpriteRenderer originSpriteRenderer, SortingGroup outmostSortingGroup = null)
        {
            this.originSpriteRenderer = originSpriteRenderer;
            this.outmostSortingGroup = outmostSortingGroup;
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
            var hashcode = OriginSpriteRenderer.GetHashCode();
            if (OutmostSortingGroup != null)
            {
                hashcode ^= OutmostSortingGroup.GetHashCode();
            }

            return hashcode;
        }

        public override string ToString()
        {
            return "SortingComponent[" + OriginSpriteRenderer.name + ", " + OriginSortingLayer + ", " +
                   OriginSortingOrder +
                   ", SG:" + (OutmostSortingGroup != null) + "]";
        }
    }
}