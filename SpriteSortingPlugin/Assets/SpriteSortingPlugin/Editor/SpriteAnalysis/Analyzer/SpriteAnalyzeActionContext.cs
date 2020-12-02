using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteAnalysis.Analyzer
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
                    spriteDataAnalyzer.Analyse(ref spriteDataItem, sprite, spriteAnalyzeInputData);
                }

                spriteAnalyzeInputData.spriteData.spriteDataDictionary[tempAssetGuid] = spriteDataItem;

                if (!spriteIsReadable)
                {
                    SetSpriteReadable(sprite.texture, false);
                }

                // currentProgress++;
            }

            var returnSpriteData = spriteAnalyzeInputData.spriteData;
            spriteAnalyzeInputData = EmptyInputData;

            return returnSpriteData;
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