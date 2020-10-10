﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
                    SetSpriteReadable();
                }

                // currentProgress++;

                var oobb = GenerateOOBB(sprite.texture, sprite.pixelsPerUnit);
                oobb.assetGuid = spritePair.Key;
                list.Add(oobb);

                // currentProgress++;

                if (!spriteIsReadable)
                {
                    SetSpriteNonReadable();
                }

                // currentProgress++;
            }


            return list;
        }

        private void SetSpriteReadable()
        {
        }

        private void SetSpriteNonReadable()
        {
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

            var adjustedBorder = new float[4];
            for (var i = 0; i < borders.Length; i++)
            {
                adjustedBorder[i] = borders[i] / pixelsPerUnit;
            }

            var width = adjustedBorder[3] - adjustedBorder[1];
            var height = adjustedBorder[2] - adjustedBorder[0];
            var oobb = new ObjectOrientedBoundingBox(new Bounds(Vector3.zero, new Vector2(width, height)), 0);

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