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
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteAnalysis.AnalyzeActions
{
    public class SpriteDataAnalyzerContext
    {
        private static readonly SpriteAnalyzeInputData EmptyInputData = new SpriteAnalyzeInputData();

        protected int totalProgress;
        protected int currentProgress;

        public int CurrentProgress => currentProgress;

        private readonly HashSet<ISpriteDataAnalyzer> spriteDataAnalyzerSet = new HashSet<ISpriteDataAnalyzer>();

        private readonly Dictionary<SpriteAnalyzerType, ISpriteDataAnalyzer> spriteDataAnalyzers =
            new Dictionary<SpriteAnalyzerType, ISpriteDataAnalyzer>();

        private SpriteAnalyzeInputData spriteAnalyzeInputData;

        public void AddSpriteDataAnalyzer(SpriteAnalyzerType spriteAnalyzerType)
        {
            var isContained = spriteDataAnalyzers.TryGetValue(spriteAnalyzerType, out var spriteDataAnalyzer);
            if (!isContained)
            {
                switch (spriteAnalyzerType)
                {
                    case SpriteAnalyzerType.Outline:
                        spriteDataAnalyzer = new SpriteOutlineAnalyzeAction();
                        break;
                    case SpriteAnalyzerType.Sharpness:
                        spriteDataAnalyzer = new SpriteSharpnessAnalyzer();
                        break;
                    case SpriteAnalyzerType.Lightness:
                        spriteDataAnalyzer = new SpriteBrightnessAnalyzeAction();
                        break;
                    case SpriteAnalyzerType.PrimaryColor:
                        spriteDataAnalyzer = new SpritePrimaryColorAnalyzer();
                        break;
                    case SpriteAnalyzerType.AverageAlpha:
                        spriteDataAnalyzer = new SpriteAverageAlphaAnalyzer();
                        break;
                }

                spriteDataAnalyzers.Add(spriteAnalyzerType, spriteDataAnalyzer);
            }

            spriteDataAnalyzerSet.Add(spriteDataAnalyzer);
        }

        public void ClearSpriteDataAnalyzers()
        {
            spriteDataAnalyzerSet.Clear();
        }

        public SpriteData Analyze(SpriteAnalyzeInputData inputData)
        {
            var startTime = EditorApplication.timeSinceStartup;
            spriteAnalyzeInputData = inputData;
            InitSpriteData();

            var assetGuidList = new List<string>();
            if (spriteAnalyzeInputData.assetGuid != null)
            {
                assetGuidList.Add(spriteAnalyzeInputData.assetGuid);
            }
            else
            {
                assetGuidList.AddRange(spriteAnalyzeInputData.spriteData.spriteDataDictionary.Keys);
            }

            foreach (var tempAssetGuid in assetGuidList)
            {
                var spriteDataItem = spriteAnalyzeInputData.spriteData.spriteDataDictionary[tempAssetGuid];

                var assetPath = AssetDatabase.GUIDToAssetPath(tempAssetGuid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                var isSpriteTextureReadable = sprite.texture.isReadable;

                if (!isSpriteTextureReadable)
                {
                    try
                    {
                        var readableSpriteTexture = CreateReadableSpriteTexture2D(sprite.texture);
                        sprite = Sprite.Create(readableSpriteTexture, sprite.textureRect, sprite.pivot,
                            sprite.pixelsPerUnit);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Sprite {sprite.name} is skipped due to an error occured.");
                        Debug.LogException(e);
                        continue;
                    }
                }

                foreach (var spriteDataAnalyzer in spriteDataAnalyzerSet)
                {
                    spriteDataAnalyzer.Analyse(ref spriteDataItem, sprite, spriteAnalyzeInputData);
                }

                spriteAnalyzeInputData.spriteData.spriteDataDictionary[tempAssetGuid] = spriteDataItem;

                if (!isSpriteTextureReadable)
                {
                    Object.DestroyImmediate(sprite);
                }

                // currentProgress++;
            }

            var returnSpriteData = spriteAnalyzeInputData.spriteData;
            spriteAnalyzeInputData = EmptyInputData;
            var timeDif = EditorApplication.timeSinceStartup - startTime;
            Debug.LogFormat("analyzed {0} sprites in {1} seconds", assetGuidList.Count, Math.Round(timeDif, 2));
            return returnSpriteData;
        }

        private Texture2D CreateReadableSpriteTexture2D(Texture2D spriteTexture)
        {
            var tmp = RenderTexture.GetTemporary(spriteTexture.width, spriteTexture.height);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(spriteTexture, tmp);

            // Backup the currently set RenderTexture
            var previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            var myTexture2D = new Texture2D(spriteTexture.width, spriteTexture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            return myTexture2D;
        }

        private void InitSpriteData()
        {
            if (spriteAnalyzeInputData.spriteData == null)
            {
                spriteAnalyzeInputData.spriteData = ScriptableObject.CreateInstance<SpriteData>();
            }

            if (spriteAnalyzeInputData.sprite != null)
            {
                AddSpriteDataItemToDictionary(spriteAnalyzeInputData.sprite);

                spriteAnalyzeInputData.assetGuid = AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(spriteAnalyzeInputData.sprite.GetInstanceID()));
                return;
            }

            var spriteRenderers = Object.FindObjectsOfType<SpriteRenderer>();
            foreach (var currentSpriteRenderer in spriteRenderers)
            {
                if (!currentSpriteRenderer.enabled || !currentSpriteRenderer.gameObject.activeInHierarchy ||
                    currentSpriteRenderer.sprite == null)
                {
                    continue;
                }

                AddSpriteDataItemToDictionary(currentSpriteRenderer.sprite);
            }
        }

        private void AddSpriteDataItemToDictionary(Sprite currentSprite)
        {
            var path = AssetDatabase.GetAssetPath(currentSprite.GetInstanceID());
            var guid = AssetDatabase.AssetPathToGUID(path);

            if (spriteAnalyzeInputData.spriteData.spriteDataDictionary.ContainsKey(guid))
            {
                return;
            }

            var spriteDataItem = new SpriteDataItem(guid, currentSprite.name);
            spriteAnalyzeInputData.spriteData.spriteDataDictionary.Add(guid, spriteDataItem);
        }
    }
}