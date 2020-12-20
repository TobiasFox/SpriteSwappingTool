using System;

namespace SpriteSortingPlugin.Survey.Data
{
    [Serializable]
    public class GeneralQuestionsData
    {
        public static readonly string[] MainFieldsOfWork = new string[]
        {
            "Design", "Programming", "Game Design", "3D Modelling", "Audio", "Texture Artist", "VFX Artist",
            "Animator", "Testing/QA", "Marketing"
        };

        public int understandingEnglish = -1;

        public bool isGameDevelopmentStudent;
        public bool isWorkingInGameDevelopment;
        public bool isGameDevelopmentHobbyist;
        public bool isNotDevelopingGames;
        public bool isGameDevelopmentRelationNoAnswer;
        public bool isGameDevelopmentRelationOther;
        public string gameDevelopmentRelationOther = "";

        public bool[] mainFieldOfWork = new bool[MainFieldsOfWork.Length];
        public bool isMainFieldOfWorkOther;
        public string mainFieldOfWorkOther = "";
        public bool isMainFieldOfWorkNoAnswer;

        //criterion of exclusion
        public int developing2dGames = -1;

        public int numberOfDeveloped2dGames = -1;
        public bool isNumberOfDeveloped2dGamesNoAnswer;

        public int workingOnApplicationWithVisualGlitch = -1;

        public bool IsAnyMainFieldOfWork()
        {
            if (mainFieldOfWork == null)
            {
                return false;
            }

            foreach (var isMainFieldActive in mainFieldOfWork)
            {
                if (isMainFieldActive)
                {
                    return true;
                }
            }

            return false;
        }

        public void ResetMainFieldOfWork()
        {
            if (mainFieldOfWork == null)
            {
                return;
            }

            for (var i = 0; i < mainFieldOfWork.Length; i++)
            {
                mainFieldOfWork[i] = false;
            }
        }
    }
}