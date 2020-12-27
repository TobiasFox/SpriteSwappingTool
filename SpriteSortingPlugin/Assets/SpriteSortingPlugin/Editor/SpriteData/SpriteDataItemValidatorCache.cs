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

namespace SpriteSortingPlugin
{
    public class SpriteDataItemValidatorCache
    {
        private static SpriteDataItemValidatorCache instance;

        private readonly Dictionary<int, SpriteDataItemValidator> validationDictionary =
            new Dictionary<int, SpriteDataItemValidator>();

        private SpriteData spriteData;

        private SpriteDataItemValidatorCache()
        {
        }

        public static SpriteDataItemValidatorCache GetInstance()
        {
            return instance ?? (instance = new SpriteDataItemValidatorCache());
        }

        public void UpdateSpriteData(SpriteData spriteData)
        {
            this.spriteData = spriteData;
        }

        public SpriteDataItemValidator GetOrCreateValidator(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return null;
            }

            var containsValidator =
                validationDictionary.TryGetValue(spriteRenderer.sprite.GetInstanceID(), out var validator);

            if (containsValidator)
            {
                return validator;
            }

            validator = new SpriteDataItemValidator();
            validator.Validate(spriteRenderer, spriteData);
            validationDictionary.Add(spriteRenderer.sprite.GetInstanceID(), validator);
            return validator;
        }

        public void Clear()
        {
            validationDictionary.Clear();
        }
    }
}