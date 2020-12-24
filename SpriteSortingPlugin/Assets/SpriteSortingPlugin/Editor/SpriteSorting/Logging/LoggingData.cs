using System;
using System.Collections.Generic;

namespace SpriteSortingPlugin.SpriteSorting.Logging
{
    [Serializable]
    public class LoggingData
    {
        public List<SortingSuggestionLoggingData> sortingSuggestionLoggingDataList =
            new List<SortingSuggestionLoggingData>();

        private int currentIndex = -1;
        private bool isCurrentLoggingDataActive;

        public bool IsCurrentLoggingDataActive => isCurrentLoggingDataActive;

        public SortingSuggestionLoggingData GetCurrentLoggingData()
        {
            if (currentIndex < 0)
            {
                return null;
            }

            if (!isCurrentLoggingDataActive)
            {
                return null;
            }

            return sortingSuggestionLoggingDataList[currentIndex];
        }

        public void ClearLastLoggingData()
        {
            if (currentIndex < 0)
            {
                return;
            }

            if (!isCurrentLoggingDataActive)
            {
                return;
            }

            sortingSuggestionLoggingDataList.RemoveAt(currentIndex);
            currentIndex--;
            isCurrentLoggingDataActive = false;
        }

        public void AddSortingOrderSuggestionLoggingData(SortingSuggestionLoggingData data)
        {
            data.question = GeneralData.questionNumberForLogging;
            currentIndex++;
            sortingSuggestionLoggingDataList.Add(data);
            isCurrentLoggingDataActive = true;
        }

        public void ConfirmSortingOrder()
        {
            isCurrentLoggingDataActive = false;
        }
    }
}