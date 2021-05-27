using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using SpriteSwappingPlugin.SortingGeneration;
using SpriteSwappingPlugin.SpriteSwappingDetector.Logging;
using SpriteSwappingPlugin.Survey.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSwappingPlugin.Survey.Analyzing
{
    public class DataConverter
    {
        private static readonly string[] UsabilityHeaders = new string[]
        {
            "Easiness Detector", "Easiness Data Analysis", "Helpfulness Generation Of Sorting Order Suggestions"
        };

        private List<string> headerList;
        private List<string> valueList;

        private string outputPathAndName;

        private SurveyData surveyData;
        private List<LoggingData> loggingDataList;

        public void AnalyzeMultiResultZips(string zipFileFolderPath, string outputFolderPath)
        {
            headerList = new List<string>();
            valueList = new List<string>();

            var dirInfo = new DirectoryInfo(zipFileFolderPath);

            var extractedFolderPath = Path.Combine(zipFileFolderPath, "extractedZips");
            Directory.CreateDirectory(extractedFolderPath);

            foreach (var fileInfo in dirInfo.GetFiles())
            {
                if (!fileInfo.Extension.Equals(".zip"))
                {
                    continue;
                }

                var destinationDirectoryName = Path.Combine(extractedFolderPath,
                    Path.GetFileNameWithoutExtension(fileInfo.Name));
                try
                {
                    ZipFile.ExtractToDirectory(fileInfo.FullName, destinationDirectoryName);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    continue;
                }

                ConvertAndSave(destinationDirectoryName, outputFolderPath);
            }

            Directory.Delete(extractedFolderPath, true);
        }

        private void ConvertAndSave(string resultFolderPath, string outputFolderPath)
        {
            headerList = new List<string>();
            valueList = new List<string>();

            LoadDataFromFolder(resultFolderPath);

            AddEntry("SurveyId", surveyData?.UserId.ToString() ?? "");

            ExtractGeneralData();
            ExtractUsabilityData();
            ExtractSortingTaskData();

            ExtractLogData();

            SaveCSVFile(Path.Combine(outputFolderPath, $"Result_{surveyData.UserId.ToString()}.csv"));

            headerList.Clear();
            valueList.Clear();
        }

        private void LoadDataFromFolder(string resultFolderPath)
        {
            var surveyDataPath = Path.Combine(resultFolderPath, "ResultSurveyData.json");
            if (File.Exists(surveyDataPath))
            {
                try
                {
                    var surveyDataContent = File.ReadAllText(surveyDataPath);
                    surveyData = JsonUtility.FromJson<SurveyData>(surveyDataContent);
                    surveyData.LoadGuid();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    surveyData = null;
                }
            }


            var loggingDataFolderPath = Path.Combine(resultFolderPath, "LogFiles");
            if (!Directory.Exists(loggingDataFolderPath))
            {
                loggingDataList = null;
            }
            else
            {
                loggingDataList = new List<LoggingData>();

                try
                {
                    var dirInfo = new DirectoryInfo(loggingDataFolderPath);

                    foreach (var fileInfo in dirInfo.GetFiles())
                    {
                        if (!fileInfo.Extension.Equals(".json"))
                        {
                            continue;
                        }

                        var loggingDataContent = File.ReadAllText(fileInfo.FullName);
                        var loggingData = JsonUtility.FromJson<LoggingData>(loggingDataContent);
                        loggingDataList.Add(loggingData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    loggingDataList = null;
                }
            }
        }

        public void ConvertAndSave(string surveyDataPath, string loggingDataPath, string outputPathAndName)
        {
            this.outputPathAndName = outputPathAndName;

            headerList = new List<string>();
            valueList = new List<string>();

            LoadData(surveyDataPath, loggingDataPath);

            ExtractGeneralData();
            ExtractUsabilityData();
            ExtractSortingTaskData();

            ExtractLogData();

            SaveCSVFile();

            headerList.Clear();
            valueList.Clear();
        }

        private void AddEntry(string header, string value)
        {
            headerList.Add(header);
            valueList.Add(value);
        }

        private void AddEntry(string header, bool value)
        {
            headerList.Add(header);
            valueList.Add((value ? 1 : 0).ToString());
        }

        private void AddEntry(string header, int value)
        {
            headerList.Add(header);
            valueList.Add(value.ToString());
        }

        private void ExtractLogData()
        {
            var sortingSuggestionPrefix = "SortingSuggestion_";

            AddEntry($"{sortingSuggestionPrefix}Created logical sorting order", "");

            var totalFoundGlitches = 0;
            var totalClearedGlitches = 0;
            var totalConfirmedGlitches = 0;

            var isLoggingDataExisting = loggingDataList != null && loggingDataList.Count > 0;
            AddEntry($"{sortingSuggestionPrefix}Used Sorting order suggestion generation", isLoggingDataExisting);

            if (isLoggingDataExisting)
            {
                foreach (var loggingData in loggingDataList)
                {
                    var currentGlitchStatistic = loggingData.CurrentFoundGlitchStatistic;
                    totalFoundGlitches += currentGlitchStatistic.totalFoundGlitches;
                    totalClearedGlitches += currentGlitchStatistic.totalClearedGlitches;
                    totalConfirmedGlitches += currentGlitchStatistic.totalConfirmedGlitches;
                }
            }

            AddEntry($"{sortingSuggestionPrefix}Total Found Glitches", totalFoundGlitches);
            AddEntry($"{sortingSuggestionPrefix}Total Cleared Glitches", totalClearedGlitches);
            AddEntry($"{sortingSuggestionPrefix}Total Confirmed Glitches", totalConfirmedGlitches);

            var totalModifications = 0;
            if (isLoggingDataExisting)
            {
                foreach (var loggingData in loggingDataList)
                {
                    foreach (var sortingSuggestionLoggingData in loggingData.sortingSuggestionLoggingDataList)
                    {
                        if (sortingSuggestionLoggingData.modifications == null)
                        {
                            continue;
                        }

                        totalModifications += sortingSuggestionLoggingData.modifications.Count;
                    }
                }
            }

            AddEntry($"{sortingSuggestionPrefix}Total Modifications", totalModifications);


            var criteriaDictionary = new Dictionary<SortingCriterionType, int>();
            foreach (SortingCriterionType criterionType in Enum.GetValues(typeof(SortingCriterionType)))
            {
                criteriaDictionary.Add(criterionType, 0);
            }

            if (isLoggingDataExisting)
            {
                foreach (var loggingData in loggingDataList)
                {
                    foreach (var sortingSuggestionLoggingData in loggingData.sortingSuggestionLoggingDataList)
                    {
                        if (sortingSuggestionLoggingData.containmentCriterionDataList != null &&
                            sortingSuggestionLoggingData.containmentCriterionDataList.Count == 1)
                        {
                            criteriaDictionary[SortingCriterionType.Containment]++;
                        }

                        if (sortingSuggestionLoggingData.primaryColorCriterionDataList != null &&
                            sortingSuggestionLoggingData.primaryColorCriterionDataList.Count == 1)
                        {
                            criteriaDictionary[SortingCriterionType.PrimaryColor]++;
                        }

                        if (sortingSuggestionLoggingData.defaultCriterionDataList != null)
                        {
                            foreach (var defaultSortingCriterionData in sortingSuggestionLoggingData
                                .defaultCriterionDataList)
                            {
                                criteriaDictionary[defaultSortingCriterionData.sortingCriterionType]++;
                            }
                        }
                    }
                }
            }

            foreach (var criterionPair in criteriaDictionary)
            {
                var header =
                    $"{sortingSuggestionPrefix}Total used criterion {ObjectNames.NicifyVariableName(criterionPair.Key.ToString())}";
                AddEntry(header, criterionPair.Value);
            }
        }

        private void LoadData(string surveyDataPath, string loggingDataPath)
        {
            var surveyDataContent = File.ReadAllText(surveyDataPath);
            surveyData = JsonUtility.FromJson<SurveyData>(surveyDataContent);

            loggingDataList = new List<LoggingData>();
            var loggingDataContent = File.ReadAllText(loggingDataPath);
            var loggingData = JsonUtility.FromJson<LoggingData>(loggingDataContent);
            loggingDataList.Add(loggingData);
        }

        private void SaveCSVFile()
        {
            var headerLine = string.Join(";", headerList);
            var valueLine = string.Join(";", valueList);

            File.WriteAllText(outputPathAndName, $"{headerLine}\n{valueLine}");
        }

        private void SaveCSVFile(string csvName)
        {
            var headerLine = string.Join(";", headerList);
            var valueLine = string.Join(";", valueList);

            File.WriteAllText(csvName, $"{headerLine}\n{valueLine}");
        }

        private void ExtractSortingTaskData()
        {
            foreach (var sortingTaskData in surveyData.sortingTaskDataList)
            {
                var taskName = sortingTaskData.sceneName.Replace(".unity", "");

                var neededMs = Math.Round(sortingTaskData.timeNeeded, 2).ToString(CultureInfo.CurrentCulture);
                var neededS = Math.Round(sortingTaskData.timeNeeded / 1000.0f, 2).ToString(CultureInfo.CurrentCulture);
                var neededMin = Math.Round(sortingTaskData.timeNeeded / 1000f / 60f, 2)
                    .ToString(CultureInfo.CurrentCulture);

                AddEntry($"{taskName}_Time Needed in ms", neededMs);
                AddEntry($"{taskName}_Time Needed in s", neededS);
                AddEntry($"{taskName}_Time Needed in min", neededMin);
                AddEntry($"{taskName}_Error Rate", "");
                AddEntry($"{taskName}_Modified Positions", 0);
            }
        }

        private void ExtractUsabilityData()
        {
            var usabilityData = surveyData.usabilityData;

            AddEntry("System Usability Score", CalculateSystemUsabilityScore().ToString(CultureInfo.CurrentCulture));

            for (var i = 0; i < usabilityData.ratingAnswers.Length; i++)
            {
                var ratingValue = "";
                if (usabilityData.ratingAnswersChanged[i])
                {
                    var roundedValue = Math.Round(usabilityData.ratingAnswers[i]);
                    ratingValue = roundedValue.ToString(CultureInfo.CurrentCulture);
                }

                AddEntry(UsabilityHeaders[i], ratingValue);
            }

            var occuringErrorValue = "";
            if (usabilityData.occuringError >= 0)
            {
                occuringErrorValue = (usabilityData.occuringError == 0 ? 1 : 0).ToString();
            }

            AddEntry("Missing Criteria Text", RemoveLineBreaks(usabilityData.missingCriteriaText));

            AddEntry("Occurring Error", occuringErrorValue);
            AddEntry("Occurring Error Text", RemoveLineBreaks(usabilityData.occuringErrorsText));

            AddEntry("Miscellaneous", usabilityData.isMiscellaneous);
            AddEntry("Miscellaneous Text", RemoveLineBreaks(usabilityData.miscellaneous));
        }

        private float CalculateSystemUsabilityScore()
        {
            var usabilityData = surveyData.usabilityData;

            var sus = 0f;
            for (var i = 0; i < usabilityData.susAnswers.Length; i++)
            {
                var susAnswer = usabilityData.susAnswers[i];
                if (i % 2 == 0)
                {
                    sus += susAnswer - 1;
                }
                else
                {
                    sus += 5 - susAnswer;
                }
            }

            sus *= 2.5f;

            return sus;
        }

        private void ExtractGeneralData()
        {
            var generalData = surveyData.generalQuestionsData;

            var developed2DGamesValue = "";
            if (generalData.developing2dGames >= 0)
            {
                developed2DGamesValue =
                    (generalData.developing2dGames == 0 ? 1 : 0).ToString();
            }

            AddEntry("Developing 2D Games", developed2DGamesValue);

            ExtractProfession();
            ExtractMainFields();

            var numberOfDeveloped2dGames = generalData.numberOfDeveloped2dGames < 0
                ? ""
                : generalData.numberOfDeveloped2dGames.ToString();
            AddEntry("Number Of Developed 2D Games", numberOfDeveloped2dGames);

            AddEntry("Number Of Developed 2D Games No Answer", generalData.isNumberOfDeveloped2dGamesNoAnswer);


            var workingOnApplicationWithVisualGlitchValue = "";
            if (generalData.workingOnApplicationWithVisualGlitch >= 0)
            {
                workingOnApplicationWithVisualGlitchValue =
                    (generalData.workingOnApplicationWithVisualGlitch == 0 ? 1 : 0).ToString();
            }

            AddEntry("Working On Application With Visual Glitch", workingOnApplicationWithVisualGlitchValue);
        }

        private void ExtractProfession()
        {
            var generalData = surveyData.generalQuestionsData;

            AddEntry("Student", generalData.isStudent);
            AddEntry("Professional", generalData.isProfessional);
            AddEntry("Hobbyist", generalData.isHobbyist);
            AddEntry("Not Developing Games", generalData.isNotDevelopingGames);
            AddEntry("Game Dev Relation Other", generalData.isGameDevelopmentRelationOther);
            AddEntry("Game Dev Relation Other Text", RemoveLineBreaks(generalData.gameDevelopmentRelationOther));
            AddEntry("Game Dev Relation No Answer", generalData.isGameDevelopmentRelationNoAnswer);
        }

        private void ExtractMainFields()
        {
            var generalData = surveyData.generalQuestionsData;

            for (var i = 0; i < GeneralQuestionsData.MainFieldsOfWork.Length; i++)
            {
                AddEntry($"MainField_{GeneralQuestionsData.MainFieldsOfWork[i]}", generalData.mainFieldOfWork[i]);
            }

            AddEntry("MainField_Other", generalData.isMainFieldOfWorkOther);
            AddEntry("MainField_Other Text", RemoveLineBreaks(generalData.mainFieldOfWorkOther));
            AddEntry("MainField_No Answer", generalData.isMainFieldOfWorkNoAnswer);
        }

        private string RemoveLineBreaks(string text)
        {
            return text.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
        }
    }
}