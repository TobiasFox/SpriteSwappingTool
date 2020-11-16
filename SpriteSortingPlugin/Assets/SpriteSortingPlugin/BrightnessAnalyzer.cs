using UnityEngine;

namespace SpriteSortingPlugin
{
    public class BrightnessAnalyzer
    {
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

            var averageBrightness = 0f;
            var pixelCounter = 0;
            foreach (var pixel in pixels)
            {
                if (pixel.a <= 0)
                {
                    continue;
                }

                pixelCounter++;

                var luminance = CalculateLuminance(pixel);
                var convertedLuminance = LuminanceToLStar(luminance);

                averageBrightness += convertedLuminance;
            }

            averageBrightness /= pixelCounter;


            return averageBrightness;
        }

        public float Analyze(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return 0;
            }

            var spriteRendererColor = spriteRenderer.color;
            var luminance = CalculateLuminance(spriteRendererColor);
            var convertedLuminance = LuminanceToLStar(luminance);

            return convertedLuminance;
        }

        /**
         * <param name="colorChannel">sRGB gamma encoded color value between 0.0 and 1.0</param>
         * <returns>linearized value</returns>
         */
        private float ConvertsRGBtoLinear(float colorChannel)
        {
            if (colorChannel <= 0.04045)
            {
                return colorChannel / 12.92f;
            }

            return Mathf.Pow(((colorChannel + 0.055f) / 1.055f), 2.4f);
        }

        private float CalculateLuminance(Color color)
        {
            return (0.2126f * ConvertsRGBtoLinear(color.r) + 0.7152f * ConvertsRGBtoLinear(color.g) +
                    0.0722f * ConvertsRGBtoLinear(color.b));
        }

        /**
         * return perceptual lightness in L*
         * <param name="luminance"> current luminance calue between 0.0 and 1.0</param>
         * <returns>value between 0 and 100, 0 means dark, 100 means light</returns>
         */
        private float LuminanceToLStar(float luminance)
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