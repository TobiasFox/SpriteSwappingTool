using System;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class BlurrinessAnalyzer
    {
        private const int OwnPixelFactor = -4;
        private const float PerceivedRedColor = 0.299f;
        private const float PerceivedGreenColor = 0.587f;
        private const float PerceivedBlueColor = 0.114f;

        private static readonly KernelItem[] kernelItems =
        {
            new KernelItem {direction = PixelDirection.North, factor = 1},
            new KernelItem {direction = PixelDirection.East, factor = 1},
            new KernelItem {direction = PixelDirection.South, factor = 1},
            new KernelItem {direction = PixelDirection.West, factor = 1}
        };

        private int spriteHeight;
        private int spriteWidth;

        public double Analyze(Sprite sprite)
        {
            var spriteTexture = sprite.texture;
            var pixels = spriteTexture.GetPixels();
            spriteHeight = spriteTexture.height;
            spriteWidth = spriteTexture.width;

            var modifiedPixelArray = ConvertToGreyScale(pixels);
            modifiedPixelArray = ApplyLaplacian(modifiedPixelArray);

            var standardDeviation = CalculateStandardDeviation(modifiedPixelArray);
            return standardDeviation;
        }

        private double[] ApplyLaplacian(double[] modifiedPixelArray)
        {
            var returnArray = new double[modifiedPixelArray.Length];
            for (var i = 1; i < spriteHeight - 1; i++)
            {
                for (var j = 1; j < spriteWidth - 1; j++)
                {
                    var index = i * spriteWidth + j;
                    var blurriness = modifiedPixelArray[index] * OwnPixelFactor;

                    foreach (var kernelItem in kernelItems)
                    {
                        var neighbourPixelIndex =
                            PixelDirectionUtility.GetIndexOfPixelDirection(index, kernelItem.direction);
                        blurriness += modifiedPixelArray[neighbourPixelIndex] * kernelItem.factor;
                    }

                    returnArray[index] = blurriness;
                }
            }

            return returnArray;
        }

        private double[] ConvertToGreyScale(Color[] pixels)
        {
            var greyScalePixels = new double[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                if (pixel.a <= 0)
                {
                    greyScalePixels[i] = 0;
                }
                else
                {
                    greyScalePixels[i] = PerceivedRedColor * pixel.r + PerceivedGreenColor * pixel.g +
                                         PerceivedBlueColor * pixel.b;
                }
            }

            return greyScalePixels;
        }


        private double CalculateStandardDeviation(double[] values)
        {
            var average = CalculateAverage(values);
            var derivationSum = 0d;
            foreach (var value in values)
            {
                derivationSum += value * value;
            }

            var derivationAverageSum = derivationSum / (values.Length - 1);
            var squaredAverage = average * average;
            return Math.Sqrt(derivationAverageSum - squaredAverage);
        }

        private double CalculateAverage(double[] values)
        {
            var average = 0d;
            foreach (var value in values)
            {
                average += value;
            }

            return average / (values.Length - 1);
        }

        private struct KernelItem
        {
            public PixelDirection direction;
            public int factor;
        }
    }
}