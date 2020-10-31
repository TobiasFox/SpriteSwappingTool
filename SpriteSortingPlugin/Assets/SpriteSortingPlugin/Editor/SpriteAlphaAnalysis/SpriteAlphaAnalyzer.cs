using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    public class SpriteAlphaAnalyzer
    {
        private int[] borders;

        private int totalProgress;
        private int currentProgress;
        private SpriteOutlineAnalyzer outlineAnalyzer;

        public int CurrentProgress => currentProgress;

        public void AddAlphaShapeToSpriteAlphaData(ref SpriteAlphaData spriteAlphaData,
            OutlineType outlineType)
        {
            if (outlineType == OutlineType.Nothing)
            {
                return;
            }

            var assetGuidList = new List<string>(spriteAlphaData.spriteDataDictionary.Keys);
            foreach (var assetGuid in assetGuidList)
            {
                var spriteDataItem = spriteAlphaData.spriteDataDictionary[assetGuid];

                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                var spriteIsReadable = sprite.texture.isReadable;

                if (!spriteIsReadable)
                {
                    var isResetReadableFlagSuccessful = SetSpriteReadable(sprite.texture, true);
                    if (isResetReadableFlagSuccessful)
                    {
                        // currentProgress+=2; or +1 error
                        continue;
                    }
                }

                if (outlineType.HasFlag(OutlineType.OOBB))
                {
                    var oobb = GenerateOOBB(sprite.texture, sprite.pixelsPerUnit);
                    spriteDataItem.objectOrientedBoundingBox = oobb;
                }

                if (outlineType.HasFlag(OutlineType.Outline))
                {
                    var colliderPoints = GenerateAlphaOutline(sprite);
                    spriteDataItem.outlinePoints = colliderPoints;
                }

                // switch (outlineType)
                // {
                //     case OutlineType.OOBB:
                //         var oobb = GenerateOOBB(sprite.texture, sprite.pixelsPerUnit);
                //         spriteDataItem.objectOrientedBoundingBox = oobb;
                //         // currentProgress++;
                //         break;
                //     case OutlineType.Outline:
                //         var colliderPoints = GenerateAlphaOutline(sprite);
                //         spriteDataItem.outlinePoints = colliderPoints;
                //         // currentProgress++;
                //         break;
                //     case OutlineType.Both:
                //         var oobb2 = GenerateOOBB(sprite.texture, sprite.pixelsPerUnit);
                //         spriteDataItem.objectOrientedBoundingBox = oobb2;
                //
                //         var colliderPoints2 = GenerateAlphaOutline(sprite);
                //         spriteDataItem.outlinePoints = colliderPoints2;
                //         // currentProgress+=2;
                //         break;
                // }

                spriteAlphaData.spriteDataDictionary[assetGuid] = spriteDataItem;

                // currentProgress++;

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

            try
            {
                AssetDatabase.StartAssetEditing();

                textureImporter.isReadable = isReadable;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
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

            return true;
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

        private ObjectOrientedBoundingBox GenerateOOBB(Texture2D spriteTexture, float pixelsPerUnit)
        {
            var startTime = EditorApplication.timeSinceStartup;

            var pixelArray = spriteTexture.GetPixels();
            borders = new int[] {spriteTexture.height, spriteTexture.width, 0, 0};
            var counter = 0;

            for (int y = 0; y < spriteTexture.height; y++)
            {
                for (int x = 0; x < spriteTexture.width; x++)
                {
                    var color = pixelArray[counter];

                    AnalyzeOutmostAlpha(x, y, color.a);
                    counter++;
                }
            }

            //more performant around 0,124s
            // for (int i = 0; i < pixelArray.Length; i++)
            // {
            //     var color = pixelArray[i];
            //
            //     if (color.a == 0)
            //     {
            //         continue;
            //     }
            //
            //     var tempHeight = i % spriteTexture.width;
            //     var row = i / spriteTexture.width;
            //
            //     AnalyzeOutmostAlpha(tempHeight, row, color.a);
            //     counter++;
            // }

            //offset of about 1 pixel in all directions
            borders[0] = Math.Max(borders[0] - 1, 0);
            borders[1] = Math.Max(borders[1] - 1, 0);
            borders[2] = Math.Min(borders[2] + 1, spriteTexture.height);
            borders[3] = Math.Min(borders[3] + 1, spriteTexture.width);

            var alphaRectangleBorder = new AlphaRectangleBorder
            {
                topBorder = borders[0],
                leftBorder = borders[1],
                bottomBorder = spriteTexture.height - borders[2],
                rightBorder = spriteTexture.width - borders[3],
                spriteHeight = spriteTexture.height,
                spriteWidth = spriteTexture.width,
                pixelPerUnit = pixelsPerUnit
            };

            var oobb = new ObjectOrientedBoundingBox(alphaRectangleBorder, Vector2.zero, 0);
            return oobb;
        }

        private void AnalyzeOutmostAlpha(int x, int y, float alpha)
        {
            if (alpha == 0)
            {
                return;
            }

            //top
            if (y < borders[0])
            {
                borders[0] = y;
            }

            //left
            if (x < borders[1])
            {
                borders[1] = x;
            }

            //bottom
            if (y > borders[2])
            {
                borders[2] = y;
            }

            //right
            if (x > borders[3])
            {
                borders[3] = x;
            }
        }
    }
}