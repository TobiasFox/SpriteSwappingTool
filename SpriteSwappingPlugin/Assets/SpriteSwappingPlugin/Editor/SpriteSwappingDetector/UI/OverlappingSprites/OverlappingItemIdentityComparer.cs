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

using System.Collections.Generic;
using UnityEngine;

namespace SpriteSwappingPlugin.SpriteSwappingDetector.UI.OverlappingSprites
{
    public class OverlappingItemIdentityComparer : Comparer<OverlappingItem>
    {
        public override int Compare(OverlappingItem x, OverlappingItem y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var sortingLayerComparison = SortingLayer.GetLayerValueFromName(y.sortingLayerName)
                .CompareTo(SortingLayer.GetLayerValueFromName(x.sortingLayerName));

            if (sortingLayerComparison != 0)
            {
                return sortingLayerComparison;
            }

            return y.sortingOrder.CompareTo(x.sortingOrder);
        }
    }
}