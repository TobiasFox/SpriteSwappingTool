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

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class UsabilityData
    {
        public int[] susAnswers = Array.ConvertAll(new int[10], i => -1);

        public float[] ratingAnswers = Array.ConvertAll(new float[3], i => 50f);
        public bool[] ratingAnswersChanged = new bool[3];

        public string missingCriteriaText = "";

        public int occuringError = -1;
        public string occuringErrorsText = "";

        public bool isMiscellaneous;
        public string miscellaneous = "";
    }
}