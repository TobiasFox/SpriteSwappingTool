using System;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class GeneralQuestionsData
    {
        public bool isGameDevelopmentStudent;
        public bool isWorkingInGameDevelopment;
        public bool isGameDevelopmentHobbyist;
        public bool isNotDevelopingGames;
        public bool isGameDevelopmentRelationNoAnswer;
        public bool isGameDevelopmentRelationOther;
        public string gameDevelopmentRelationOther = "";

        public int mainFieldOfWork = -1;
        public bool isMainFieldOfWorkOther;
        public string mainFieldOfWorkOther = "";
        public bool isMainFieldOfWorkNoAnswer;

        public int developing2dGames = -1;

        public int numberOfDeveloped2dGames = -1;
        public bool isNumberOfDeveloped2dGamesNoAnswer;

        
        public int knowingVisualGlitches = -1;
        public string visualGlitchReasons = "";

        public int workingOnApplicationWithVisualGlitch = -1;
        public int numberOfApplicationsWithVisualGlitches = -1;
        public bool isNotKnowingNumberOfApplicationsWithVisualGlitches;
        public bool isNumberOfApplicationsWithVisualGlitchesNoAnswer;

        public int solvedVisualGlitches = -1;
        public string solvingVisualGlitchApproach = "";
    }
}