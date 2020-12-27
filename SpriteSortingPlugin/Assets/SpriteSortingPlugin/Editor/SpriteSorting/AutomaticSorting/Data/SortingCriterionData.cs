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

namespace SpriteSortingPlugin.SpriteSorting.AutomaticSorting.Data
{
    [Serializable]
    public class SortingCriterionData : ICloneable
    {
        public bool isActive;
        public float priority = 1;
        [HideInInspector] public bool isExpanded;
        [HideInInspector] public bool isAddedToEditorList;
        public SortingCriterionType sortingCriterionType;

        protected void CopyDataTo(SortingCriterionData copy)
        {
            copy.isActive = isActive;
            copy.priority = priority;
            copy.isExpanded = isExpanded;
            copy.isAddedToEditorList = isAddedToEditorList;
            copy.sortingCriterionType = sortingCriterionType;
        }

        public virtual object Clone()
        {
            var clone = new SortingCriterionData();
            CopyDataTo(clone);
            return clone;
        }
    }
}