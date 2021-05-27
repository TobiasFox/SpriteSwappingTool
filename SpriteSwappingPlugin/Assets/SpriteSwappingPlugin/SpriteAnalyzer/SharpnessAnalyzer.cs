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

using System;
using UnityEngine;

namespace SpriteSwappingPlugin.SpriteAnalyzer
{
    public class SharpnessAnalyzer
    {
        private const int OwnPixelFactor = -4;

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
            spriteHeight = spriteTexture.height;
            spriteWidth = spriteTexture.width;
            PixelDirectionUtility.spriteWidth = spriteWidth;
            
            var pixels = spriteTexture.GetPixels();
            var modifiedPixelArray = ConvertToGreyScale(pixels);

            modifiedPixelArray = ApplyLaplacian(modifiedPixelArray);

            var standardDeviation = CalculateStandardDeviation(modifiedPixelArray);
            var variance = standardDeviation * standardDeviation;
            return variance;
        }

        private double[] ApplyLaplacian(double[] modifiedPixelArray)
        {
            var returnArray = new double[modifiedPixelArray.Length];
            for (var i = 0; i < spriteHeight; i++)
            {
                for (var j = 0; j < spriteWidth; j++)
                {
                    var index = i * spriteWidth + j;
                    var sharpness = modifiedPixelArray[index] * OwnPixelFactor;

                    foreach (var kernelItem in kernelItems)
                    {
                        var neighbourPixelIndex =
                            PixelDirectionUtility.GetIndexOfPixelDirection(index, kernelItem.direction);

                        if (neighbourPixelIndex < 0 || neighbourPixelIndex >= modifiedPixelArray.Length - 1)
                        {
                            continue;
                        }

                        //outside of left image side
                        if (index % spriteWidth == 0 && neighbourPixelIndex == index - 1)
                        {
                            continue;
                        }

                        //outside of right image side
                        if (neighbourPixelIndex % spriteWidth == 0 && neighbourPixelIndex == index + 1)
                        {
                            continue;
                        }

                        sharpness += modifiedPixelArray[neighbourPixelIndex] * kernelItem.factor;
                    }

                    returnArray[index] = sharpness;
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
                    greyScalePixels[i] = pixel.grayscale * 255;
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
                var temp = value - average;
                derivationSum += (temp * temp);
            }

            return Math.Sqrt(derivationSum / (values.Length - 1));
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