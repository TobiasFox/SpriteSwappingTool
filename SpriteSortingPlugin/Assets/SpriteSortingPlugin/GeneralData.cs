namespace SpriteSortingPlugin
{
    public static class GeneralData
    {
        //TODO set to true
        public const bool IsValidatingUserInput = false;

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

        public const string ClipperLibName = "clipper";
        public const string ClipperLibVersion = "6.4.2";
        public const string ClipperLibLicense = "BSD license";
        public const string ClipperLibLink = "http://www.angusj.com/delphi/clipper.php";

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