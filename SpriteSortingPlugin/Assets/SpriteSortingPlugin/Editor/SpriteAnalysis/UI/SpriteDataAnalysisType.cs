﻿#region license

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

namespace SpriteSortingPlugin.SpriteAnalysis.UI
{
    [Flags]
    public enum SpriteDataAnalysisType
    {
        Nothing = 0,
        Outline = 1 << 0,
        Sharpness = 1 << 1,
        Lightness = 1 << 2,
        PrimaryColor = 1 << 3,
        AverageAlpha = 1 << 4,
        All = ~0
    }
}