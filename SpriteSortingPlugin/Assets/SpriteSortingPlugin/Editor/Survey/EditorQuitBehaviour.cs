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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    [InitializeOnLoad]
    public class EditorQuitBehaviour
    {
        private static readonly string[] SurveyDataOutputPath = new string[]
        {
            "SurveyData"
        };

        static EditorQuitBehaviour()
        {
            EditorApplication.quitting += Quit;
        }

        private static void Quit()
        {
            DeleteCache();
        }

        private static void DeleteCache()
        {
            var surveyDataPath = Path.Combine(Application.temporaryCachePath, Path.Combine(SurveyDataOutputPath));

            try
            {
                Directory.Delete(surveyDataPath, true);
            }
            catch
            {
                // ignored
            }
        }
    }
}