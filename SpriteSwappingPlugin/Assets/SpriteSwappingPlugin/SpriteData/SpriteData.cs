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

namespace SpriteSwappingPlugin
{
    // [CreateAssetMenu(fileName = "Data")]
    public class SpriteData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SpriteDataItem> spriteDataList = new List<SpriteDataItem>();

        [NonSerialized]
        public Dictionary<string, SpriteDataItem> spriteDataDictionary = new Dictionary<string, SpriteDataItem>();

        public void OnBeforeSerialize()
        {
            if (spriteDataDictionary != null)
            {
                spriteDataList = new List<SpriteDataItem>(spriteDataDictionary.Values);
            }
        }

        public void OnAfterDeserialize()
        {
            if (spriteDataList != null)
            {
                spriteDataDictionary.Clear();

                foreach (var spriteData in spriteDataList)
                {
                    spriteDataDictionary.Add(spriteData.AssetGuid, spriteData);
                }
            }
        }

        public bool SpriteDataDictionaryContainsAllGuids(List<string> guidList)
        {
            if (spriteDataDictionary == null || guidList == null)
            {
                return false;
            }

            foreach (var guid in guidList)
            {
                if (!spriteDataDictionary.ContainsKey(guid))
                {
                    return false;
                }
            }

            return true;
        }
    }
}