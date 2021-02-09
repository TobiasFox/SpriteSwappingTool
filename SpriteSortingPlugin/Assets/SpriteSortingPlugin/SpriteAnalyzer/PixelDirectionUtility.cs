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

namespace SpriteSortingPlugin.SpriteAnalyzer
{
    public static class PixelDirectionUtility
    {
        public const int PixelDirections = 8;
        public static int spriteWidth;

        public static int GetIndexOfPixelDirection(int currentPixelIndex, PixelDirection pixelDirection)
        {
            switch (pixelDirection)
            {
                case PixelDirection.South:
                    currentPixelIndex -= spriteWidth;
                    break;
                case PixelDirection.Southeast:
                    currentPixelIndex = currentPixelIndex - spriteWidth + 1;
                    break;
                case PixelDirection.East:
                    currentPixelIndex++;
                    break;
                case PixelDirection.Northeast:
                    currentPixelIndex = currentPixelIndex + spriteWidth + 1;
                    break;
                case PixelDirection.North:
                    currentPixelIndex += spriteWidth;
                    break;
                case PixelDirection.Northwest:
                    currentPixelIndex = currentPixelIndex + spriteWidth - 1;
                    break;
                case PixelDirection.West:
                    currentPixelIndex--;
                    break;
                case PixelDirection.Southwest:
                    currentPixelIndex = currentPixelIndex - spriteWidth - 1;
                    break;
            }

            return currentPixelIndex;
        }

        public static PixelDirection GetNextPixelDirectionClockWise(PixelDirection currentPixelDirection)
        {
            return GetDirectionWithOffset(currentPixelDirection, 7);
        }

        public static PixelDirection GetBacktracedPixelDirectionClockWise(PixelDirection currentPixelDirection,
            PixelDirection startPixelDirection)
        {
            var directionOffset = 0;
            switch (startPixelDirection)
            {
                case PixelDirection.North:
                    directionOffset = 2;
                    break;
                case PixelDirection.West:
                    directionOffset = 4;
                    break;
                case PixelDirection.South:
                    directionOffset = 6;
                    break;
            }

            var counter = 0;

            var tempDirection = (PixelDirection) ((int) startPixelDirection);
            for (int i = 0; i < PixelDirections; i++)
            {
                if (tempDirection == currentPixelDirection)
                {
                    break;
                }

                tempDirection = GetNextPixelDirectionClockWise(tempDirection);
                counter++;
            }

            switch (counter)
            {
                case 0:
                    return GetOppositePixelDirection(currentPixelDirection);
                case 1:
                case 8:
                    return GetDirectionWithOffset(PixelDirection.South, directionOffset);
                case 2:
                case 3:
                    return GetDirectionWithOffset(PixelDirection.West, directionOffset);
                case 4:
                case 5:
                    return GetDirectionWithOffset(PixelDirection.North, directionOffset);
                case 6:
                case 7:
                    return GetDirectionWithOffset(PixelDirection.East, directionOffset);
            }

            return currentPixelDirection;
        }

        private static PixelDirection GetDirectionWithOffset(PixelDirection currentPixelDirection, int offset)
        {
            return (PixelDirection) (((int) currentPixelDirection + offset) % PixelDirections);
        }

        public static PixelDirection GetOppositePixelDirection(PixelDirection currentPixelDirection)
        {
            return GetDirectionWithOffset(currentPixelDirection, 4);
        }
    }
}