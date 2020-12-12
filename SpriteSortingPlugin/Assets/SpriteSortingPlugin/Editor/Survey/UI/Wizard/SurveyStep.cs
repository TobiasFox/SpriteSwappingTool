namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public abstract class SurveyStep
    {
        protected bool isCompleted;
        protected bool isSkipped;
        protected string jsonData;
        protected string name;

        public bool IsCompleted => isCompleted;
        public bool IsSkipped => isSkipped;
        public string JsonData => jsonData;
        public string Name => name;

        public SurveyStep(string name)
        {
            this.name = name;
        }

        public abstract void Commit();

        public abstract void Rollback();

        public abstract void DrawContent();
    }
}