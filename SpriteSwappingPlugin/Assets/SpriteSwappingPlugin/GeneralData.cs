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

namespace SpriteSwappingPlugin
{
    public static class GeneralData
    {
        public const int MayorVersionNr = 1;
        public const int MinorVersionNr = 0;
        public const int FixVersionNr = 0;
        public const string DevelopedBy = "Tobias Fox";
        public const string DeveloperMailAddress = "tobiasfox@gmx.net";

        public const string UnityMenuMainCategory = "Tools";
        public const string Name = "Sprite Swapping";

        public const string DetectorName = "Detector";
        public const string DetectorShortcut = "%q";

        public const string DataAnalysisName = "Data Analysis";
        public const string DataAnalysisShortcut = "%e";

        public const string ClipperLibName = "Clipper";
        public const string ClipperLibVersion = "6.4.2";
        public const string ClipperLibLicense = "Boost Software License (BSL 1.0)";
        public const string ClipperLibLink = "http://www.angusj.com/delphi/clipper.php";

        public const bool IsValidatingUserInput = false;

        public static int questionNumberForLogging;
        public static string currentSurveyId;

        public static bool isSurveyActive;
        public static bool isAutomaticSortingActive;
        public static bool isLoggingActive;

        public static string FullDetectorName => Name + " " + DetectorName;
        public static string FullDataAnalysisName => Name + " " + DataAnalysisName;

        public static string GetFullVersionNumber()
        {
            return MayorVersionNr + "." + MinorVersionNr + "." + FixVersionNr;
        }
    }
}