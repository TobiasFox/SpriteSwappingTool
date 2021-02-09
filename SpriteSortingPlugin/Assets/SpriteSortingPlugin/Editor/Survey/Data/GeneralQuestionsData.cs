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
    public class GeneralQuestionsData
    {
        public static readonly string[] MainFieldsOfWork = new string[]
        {
            "Design", "Programming", "Game Design", "3D Modelling", "Audio", "Texture Artist", "VFX Artist",
            "Animator", "Testing/QA", "Marketing"
        };

        public bool isStudent;
        public bool isProfessional;
        public bool isHobbyist;
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