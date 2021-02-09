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
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.AutoSorting.Data
{
    [Serializable]
    public class PrimaryColorSortingCriterionData : SortingCriterionData
    {
        public bool[] activeChannels = new bool[] {true, true, true};
        public Color backgroundColor;
        public Color foregroundColor;

        public PrimaryColorSortingCriterionData()
        {
            sortingCriterionType = SortingCriterionType.PrimaryColor;
        }

        public override object Clone()
        {
            var clone = new PrimaryColorSortingCriterionData();
            CopyDataTo(clone);
            clone.activeChannels = new bool[3];
            for (var i = 0; i < activeChannels.Length; i++)
            {
                clone.activeChannels[i] = activeChannels[i];
            }

            clone.backgroundColor =
                new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);
            clone.foregroundColor =
                new Color(foregroundColor.r, foregroundColor.g, foregroundColor.b, foregroundColor.a);

            return clone;
        }
    }
}