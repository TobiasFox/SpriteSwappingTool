﻿namespace SpriteSortingPlugin
{
    public static class GeneralData
    {
        public const int MayorVersionNr = 1;
        public const int MinorVersionNr = 1;
        public const int FixVersionNr = 1;
        public const string DevelopedBy = "Tobias Fox";
        public const string DeveloperMailAddress = "TobiasFox@GMX.net";

        public const string UnityMenuMainCategory = "Tools";
        public const string Name = "Sprite Swapping";
        
        public const string DetectorName = "Detector";
        public const string DetectorShortcut = "%q";
        
        public const string DataAnalysisName = "Data Analysis";
        public const string DataAnalysisShortcut = "%e";

        public const string ClipperLibName = "clipper";
        public const string ClipperLibVersion = "6.4.2";
        public const string ClipperLibLicense = "BSD license";
        public const string ClipperLibLink = "http://www.angusj.com/delphi/clipper.php";
        
        public static string GetFullVersionNumber()
        {
            return MayorVersionNr + "." + MinorVersionNr + "." + FixVersionNr;
        }
    }
}