#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteSwappingPlugin.SpriteSwappingDetector
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

        public int GetInstanceId()
        {
            return SortingGroup != null
                ? SortingGroup.GetInstanceID()
                : SpriteRenderer.GetInstanceID();
        }

        public bool Equals(SortingComponent other)
        {
            return SpriteRenderer == other.SpriteRenderer &&
                   SortingGroup == other.SortingGroup;
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is SortingComponent otherSortingComponent))
            {
                return false;
            }

            return Equals(otherSortingComponent);
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

        public override string ToString()
        {
            return "SortingComponent[" + spriteRenderer.name + ", " + OriginSortingLayer + ", " +
                   OriginSortingOrder + ", SG:" + (sortingGroup != null) + "]";
        }

        public void OnBeforeSerialize()
        {
            if (overlappingSortingComponents == null)
            {
                return;
            }

            serializedOverlappingSortingComponents = new List<int>(overlappingSortingComponents);
        }

        public void OnAfterDeserialize()
        {
            if (serializedOverlappingSortingComponents == null)
            {
                return;
            }

            overlappingSortingComponents = new HashSet<int>(serializedOverlappingSortingComponents);
        }
    }
}