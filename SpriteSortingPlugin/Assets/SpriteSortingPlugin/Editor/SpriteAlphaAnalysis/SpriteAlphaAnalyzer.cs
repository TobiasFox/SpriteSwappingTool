using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    public class SpriteAlphaAnalyzer
    {
        private int totalProgress;
        private int currentProgress;
        private SpriteOutlineAnalyzer outlineAnalyzer;
        private OOBBGenerator oOBBGenerator;

        public int CurrentProgress => currentProgress;

        public void AddAlphaShapeToSpriteAlphaData(ref SpriteData spriteData,
            OutlineAnalysisType outlineType)
        {
            if (outlineType == OutlineAnalysisType.Nothing)
            {
                return;
            }

            var assetGuidList = new List<string>(spriteData.spriteDataDictionary.Keys);
            foreach (var assetGuid in assetGuidList)
            {
                var spriteDataItem = spriteData.spriteDataDictionary[assetGuid];

                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
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

                if (outlineType.HasFlag(OutlineAnalysisType.ObjectOrientedBoundingBox))
                {
                    var oobb = GenerateOOBB(sprite);
                    spriteDataItem.objectOrientedBoundingBox = oobb;
                    // currentProgress++;
                }

                if (outlineType.HasFlag(OutlineAnalysisType.PixelPerfect))
                {
                    var colliderPoints = GenerateAlphaOutline(sprite);
                    spriteDataItem.outlinePoints = colliderPoints;
                    // currentProgress++;
                }

                spriteData.spriteDataDictionary[assetGuid] = spriteDataItem;

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

        private List<Vector2> GenerateAlphaOutline(Sprite sprite)
        {
            if (outlineAnalyzer == null)
            {
                outlineAnalyzer = new SpriteOutlineAnalyzer();
            }

            var points = outlineAnalyzer.Analyze(sprite);
            return points;
        }

        private ObjectOrientedBoundingBox GenerateOOBB(Sprite sprite)
        {
            if (oOBBGenerator == null)
            {
                oOBBGenerator = new OOBBGenerator();
            }

            var oobb = oOBBGenerator.Generate(sprite);
            return oobb;
        }
    }
}