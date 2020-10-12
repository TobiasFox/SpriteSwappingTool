using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    public class SpriteAlphaAnalyzer
    {
        private int[] borders;
        private Dictionary<string, Sprite> spriteDictionary;

        private int totalProgress;
        private int currentProgress;

        public int CurrentProgress => currentProgress;

        public void Initialize()
        {
            var spriteRenderers = Object.FindObjectsOfType<SpriteRenderer>();
            spriteDictionary = new Dictionary<string, Sprite>();

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (!spriteRenderer.enabled || !spriteRenderer.gameObject.activeInHierarchy ||
                    spriteRenderer.sprite == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID());
                var guid = AssetDatabase.AssetPathToGUID(path);

                if (!spriteDictionary.ContainsKey(guid))
                {
                    spriteDictionary.Add(guid, spriteRenderer.sprite);
                }
            }

            totalProgress = spriteDictionary.Count;
        }

        public List<ObjectOrientedBoundingBox> GenerateOOBBs()
        {
            var list = new List<ObjectOrientedBoundingBox>();

            foreach (var spritePair in spriteDictionary)
            {
                var sprite = spritePair.Value;
                var spriteIsReadable = sprite.texture.isReadable;

                if (!spriteIsReadable)
                {
                    SetSpriteReadable(sprite.texture);
                }

                // currentProgress++;

                var oobb = GenerateOOBB(sprite.texture, sprite.pixelsPerUnit);
                oobb.assetGuid = spritePair.Key;
                oobb.assetName = sprite.texture.name;
                list.Add(oobb);

                // currentProgress++;

                if (!spriteIsReadable)
                {
                    SetSpriteNonReadable(sprite.texture);
                }

                // currentProgress++;
            }


            return list;
        }

        private void SetSpriteReadable(Texture2D spriteTexture)
        {
            var path = AssetDatabase.GetAssetPath(spriteTexture.GetInstanceID());
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            AssetDatabase.StartAssetEditing();

            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            AssetDatabase.StopAssetEditing();
        }

        private void SetSpriteNonReadable(Texture2D spriteTexture)
        {
            var path = AssetDatabase.GetAssetPath(spriteTexture.GetInstanceID());
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            AssetDatabase.StartAssetEditing();

            textureImporter.isReadable = false;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            AssetDatabase.StopAssetEditing();
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

            var adjustedBorder = new float[4];
            for (var i = 0; i < borders.Length; i++)
            {
                adjustedBorder[i] = borders[i] / pixelsPerUnit;
            }

            var width = adjustedBorder[3] - adjustedBorder[1];
            var height = adjustedBorder[2] - adjustedBorder[0];
            var oobb = new ObjectOrientedBoundingBox(new Bounds(Vector3.zero, new Vector2(width, height)), 0)
            {
                AlphaRectangleBorder = alphaRectangleBorder
            };

            // Debug.Log("analyzed within " + (EditorApplication.timeSinceStartup - startTime));
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