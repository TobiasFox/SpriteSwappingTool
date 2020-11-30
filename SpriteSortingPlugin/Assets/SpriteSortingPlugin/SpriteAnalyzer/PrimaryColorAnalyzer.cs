using UnityEngine;

namespace SpriteSortingPlugin.SpriteAnalyzer
{
    public class PrimaryColorAnalyzer
    {
        public Color Analyze(Sprite sprite)
        {
            var averageColor = new Color();

            if (sprite == null)
            {
                return averageColor;
            }

            if (!sprite.texture.isReadable)
            {
                return averageColor;
            }

            var pixels = sprite.texture.GetPixels();

            var pixelCounter = 0;
            foreach (var pixel in pixels)
            {
                if (pixel.a <= 0)
                {
                    continue;
                }

                pixelCounter++;
                averageColor += pixel;
            }

            averageColor /= pixelCounter;

            averageColor.a = 1;

            return averageColor;
        }
    }
}