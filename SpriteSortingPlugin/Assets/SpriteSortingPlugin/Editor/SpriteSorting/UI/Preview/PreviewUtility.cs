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

using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.UI.Preview
{
    public static class PreviewUtility
    {
        //TODO: convert to a pool?
        public static GameObject CreateGameObject(Transform parent, string name, bool isDontSave)
        {
            var previewItem = new GameObject(name)
            {
                hideFlags = isDontSave ? HideFlags.DontSave : HideFlags.None
            };

            previewItem.transform.SetParent(parent);

            return previewItem;
        }

        public static void HideAndDontSaveGameObject(GameObject gameObject)
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}