using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalyzer
{
    public class AverageAlphaAnalyzer
    {
        public float Analyze(Sprite sprite)
        {
            var pixels = sprite.texture.GetPixels();
            float averageAlpha = 0;

            var pixelCounter = 0;
            foreach (var pixel in pixels)
            {
                if (pixel.a <= 0)
                {
                    continue;
                }

                pixelCounter++;
                averageAlpha += pixel.a;
            }

            averageAlpha /= pixelCounter;

            return averageAlpha;
        }
    }
}