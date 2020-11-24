using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
{
    public class SpriteDataAnalyzerContext
    {
        protected int totalProgress;
        protected int currentProgress;

        public int CurrentProgress => currentProgress;

        private readonly HashSet<ISpriteDataAnalyzer> spriteDataAnalyzerSet = new HashSet<ISpriteDataAnalyzer>();

        private readonly Dictionary<SpriteAnalyzerType, ISpriteDataAnalyzer> spriteDataAnalyzers =
            new Dictionary<SpriteAnalyzerType, ISpriteDataAnalyzer>();

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
                    case SpriteAnalyzerType.Blurriness:
                        spriteDataAnalyzer = new SpriteBlurrinessAnalyzer();
                        break;
                    case SpriteAnalyzerType.Lightness:
                        spriteDataAnalyzer = new SpriteBrightnessAnalyzeAction();
                        break;
                    case SpriteAnalyzerType.PrimaryColor:
                        spriteDataAnalyzer = new SpritePrimaryColorAnalyzer();
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

        public void Analyze(ref SpriteData spriteData, SpriteAnalyzeInputData inputData)
        {
            var assetGuidList = new List<string>();

            if (inputData.assetGuid != null)
            {
                assetGuidList.Add(inputData.assetGuid);
            }
            else
            {
                assetGuidList.AddRange(spriteData.spriteDataDictionary.Keys);
            }

            foreach (var tempAssetGuid in assetGuidList)
            {
                var spriteDataItem = spriteData.spriteDataDictionary[tempAssetGuid];

                var assetPath = AssetDatabase.GUIDToAssetPath(tempAssetGuid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                var spriteIsReadable = sprite.texture.isReadable;

                if (!spriteIsReadable)
                {
                    var isResetReadableFlagSuccessful = SetSpriteReadable(sprite.texture, true);
                    if (!isResetReadableFlagSuccessful)
                    {
                        // currentProgress+=2; or +1 error
                        continue;
                    }
                }

                foreach (var spriteDataAnalyzer in spriteDataAnalyzerSet)
                {
                    spriteDataAnalyzer.Analyse(ref spriteDataItem, sprite, inputData);
                }

                spriteData.spriteDataDictionary[tempAssetGuid] = spriteDataItem;

                if (!spriteIsReadable)
                {
                    SetSpriteReadable(sprite.texture, false);
                }

                // currentProgress++;
            }
        }

        private bool SetSpriteReadable(Texture2D spriteTexture, bool isReadable)
        {
            var path = AssetDatabase.GetAssetPath(spriteTexture.GetInstanceID());
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter == null)
            {
                return false;
            }

            var isSuccessfullyChangeReadableFlag = false;

            try
            {
                AssetDatabase.StartAssetEditing();

                textureImporter.isReadable = isReadable;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                isSuccessfullyChangeReadableFlag = true;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("could not set readable flag to {0} of sprite {1}", isReadable, path);
                Debug.LogWarning(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return isSuccessfullyChangeReadableFlag;
        }
    }
}