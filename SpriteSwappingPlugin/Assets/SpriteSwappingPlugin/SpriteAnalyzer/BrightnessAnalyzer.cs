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

namespace SpriteSwappingPlugin.SpriteAnalyzer
{
    /**
     * analysis is based on https://stackoverflow.com/a/56678483
     */
    public class BrightnessAnalyzer
    {
        private const float UnitScaleFactor = 100f;

        public float Analyze(Sprite sprite)
        {
            if (sprite == null)
            {
                return 0;
            }

            if (!sprite.texture.isReadable)
            {
                return 0;
            }

            var pixels = sprite.texture.GetPixels();

            var averagePerceivedLightness = 0f;
            var pixelCounter = 0;
            foreach (var pixel in pixels)
            {
                if (pixel.a <= 0)
                {
                    continue;
                }

                pixelCounter++;

                var pixelLightness = CalculatePerceivedLightness(pixel);
                averagePerceivedLightness += pixelLightness;
            }

            averagePerceivedLightness /= pixelCounter;

            return averagePerceivedLightness;
        }

        public float Analyze(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return 0;
            }

            return CalculatePerceivedLightness(spriteRenderer.color);
        }

        public float ApplySpriteRendererColor(float lightness, Color spriteRendererColor)
        {
            if (spriteRendererColor.r.Equals(1) && spriteRendererColor.g.Equals(1) && spriteRendererColor.b.Equals(1))
            {
                return lightness;
            }

            var spriteRendererLightness = CalculatePerceivedLightness(spriteRendererColor);

            var combinedLightness = lightness / UnitScaleFactor;
            combinedLightness *= spriteRendererLightness / UnitScaleFactor;

            return combinedLightness * UnitScaleFactor;
        }

        public float Analyze2(SpriteRenderer spriteRenderer)
        {
            var color = spriteRenderer.color;
            var redColor = color.r * color.r * 0.299f;
            var blueColor = color.g * color.g * 0.587f;
            var greenColor = color.b * color.b * 0.114f;

            return Mathf.Sqrt(redColor + blueColor + greenColor);
        }

        private float CalculatePerceivedLightness(Color color)
        {
            var luminance = CalculateLuminance(color);
            var convertedLuminance = LuminanceToCIELAB(luminance);

            return convertedLuminance;
        }

        private static float CalculateLuminance(Color color)
        {
            var linearColor = color.linear;
            return (0.2126f * linearColor.r + 0.7152f * linearColor.g + 0.0722f * linearColor.b);
        }

        /**
         * return perceptual lightness in L*
         * <param name="luminance"> current luminance value between 0.0 and 1.0</param>
         * <returns>value between 0 and 100, 0 means dark, 100 means light</returns>
         */
        private static float LuminanceToCIELAB(float luminance)
        {
            if (luminance < 0)
            {
                return 0;
            }

            if (luminance > 1.0001f)
            {
                return 1f;
            }

            // CIE standard states 0.008856, calculate number (0.008856451679036) on the fly 
            if (luminance <= (216f / 24389f))
            {
                // CIE standard states 903.3, calculate number (903.296296296296296) on the fly
                return luminance * (24389f / 27f);
            }

            return Mathf.Pow(luminance, (1f / 3f)) * 116f - 16f;
        }
    }
}