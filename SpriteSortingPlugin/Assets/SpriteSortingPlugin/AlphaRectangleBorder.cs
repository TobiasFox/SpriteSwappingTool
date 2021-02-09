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

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct AlphaRectangleBorder : ICloneable
    {
        public int topBorder;
        public int leftBorder;
        public int bottomBorder;
        public int rightBorder;

        public int spriteWidth;
        public int spriteHeight;
        public float pixelPerUnit;

        public object Clone()
        {
            return new AlphaRectangleBorder
            {
                topBorder = this.topBorder,
                leftBorder = this.leftBorder,
                bottomBorder = this.bottomBorder,
                rightBorder = this.rightBorder,
                spriteHeight = this.spriteHeight,
                spriteWidth = this.spriteWidth,
                pixelPerUnit = this.pixelPerUnit
            };
        }
    }
}