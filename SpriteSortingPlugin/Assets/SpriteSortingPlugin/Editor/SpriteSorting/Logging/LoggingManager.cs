namespace SpriteSortingPlugin.SpriteSorting.Logging
{
    public class LoggingManager
    {
        public LoggingData loggingData;

        private static LoggingManager instance;

        public static LoggingManager GetInstance()
        {
            if (instance == null)
            {
                instance = new LoggingManager();
            }

            return instance;
        }

        private LoggingManager()
        {
            loggingData = new LoggingData();
        }

        public void Clear()
        {
            instance = null;
            
        }
    }
}