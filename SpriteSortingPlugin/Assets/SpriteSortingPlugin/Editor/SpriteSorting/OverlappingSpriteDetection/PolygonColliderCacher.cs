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
using SpriteSortingPlugin.SpriteSorting.UI.Preview;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteSorting.OverlappingSpriteDetection
{
    public class PolygonColliderCacher
    {
        private static PolygonColliderCacher instance;

        private Dictionary<string, PolygonCollider2D[]> spriteColliderDataDictionary =
            new Dictionary<string, PolygonCollider2D[]>();

        private PolygonColliderCacher()
        {
        }

        public static PolygonColliderCacher GetInstance()
        {
            return instance ?? (instance = new PolygonColliderCacher());
        }

        public PolygonCollider2D GetCachedColliderOrCreateNewCollider(string assetGuid,
            SpriteDataItem spriteDataItem, Transform transform)
        {
            var containsColliderArray =
                spriteColliderDataDictionary.TryGetValue(assetGuid, out var polygonColliderArray);
            if (!containsColliderArray)
            {
                polygonColliderArray = new PolygonCollider2D[2];

                var polygonCollider = CreateNewPolygonColliderOnNewGameObject(spriteDataItem);
                SetColliderPointsToCollider(spriteDataItem, transform, ref polygonCollider);
                PreviewUtility.HideAndDontSaveGameObject(polygonCollider.gameObject);

                polygonColliderArray[0] = polygonCollider;
                spriteColliderDataDictionary[assetGuid] = polygonColliderArray;
                return polygonCollider;
            }

            for (var i = 0; i < polygonColliderArray.Length; i++)
            {
                var polygonCollider = polygonColliderArray[i];

                if (polygonCollider == null)
                {
                    polygonCollider = CreateNewPolygonColliderOnNewGameObject(spriteDataItem);
                    SetColliderPointsToCollider(spriteDataItem, transform, ref polygonCollider);
                    PreviewUtility.HideAndDontSaveGameObject(polygonCollider.gameObject);
                    polygonColliderArray[i] = polygonCollider;

                    spriteColliderDataDictionary[assetGuid] = polygonColliderArray;
                    return polygonCollider;
                }

                if (polygonCollider.enabled)
                {
                    continue;
                }

                SetColliderPointsToCollider(spriteDataItem, transform, ref polygonCollider);
                polygonCollider.enabled = true;
                return polygonCollider;
            }

            return null;
        }

        private static PolygonCollider2D CreateNewPolygonColliderOnNewGameObject(SpriteDataItem spriteDataItem)
        {
            var polyColliderGameObject =
                PreviewUtility.CreateGameObject(null, $"PolygonCollider {spriteDataItem.AssetName}", true);
            return polyColliderGameObject.AddComponent<PolygonCollider2D>();
        }

        private static void SetColliderPointsToCollider(SpriteDataItem spriteDataItem, Transform transform,
            ref PolygonCollider2D polygonCollider)
        {
            polygonCollider.transform.SetPositionAndRotation(transform.position, transform.rotation);
            polygonCollider.transform.localScale = transform.lossyScale;

            polygonCollider.points = spriteDataItem.outlinePoints;
        }

        public void DisableCachedCollider(string assetGuid, int polygonColliderInstanceId)
        {
            var containsColliderArray =
                spriteColliderDataDictionary.TryGetValue(assetGuid, out var polygonColliderArray);
            if (!containsColliderArray)
            {
                return;
            }

            foreach (var polygonCollider in polygonColliderArray)
            {
                if (polygonCollider == null)
                {
                    continue;
                }

                if (polygonCollider.GetInstanceID() != polygonColliderInstanceId)
                {
                    continue;
                }

                polygonCollider.enabled = false;
                break;
            }
        }

        public void CleanUp()
        {
            foreach (var polygonColliders in spriteColliderDataDictionary.Values)
            {
                if (polygonColliders == null)
                {
                    continue;
                }

                foreach (var polygonCollider in polygonColliders)
                {
                    if (polygonCollider == null)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(polygonCollider.gameObject);
                }
            }
        }
    }
}