using System.Text;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public abstract class SurveyStep
    {
        protected SurveyFinishState finishState;
        protected bool isStarted;
        protected bool isFinished;
        protected string jsonData;
        protected string name;

        public SurveyFinishState FinishState => finishState;
        public string JsonData => jsonData;
        public string Name => name;
        public bool IsFinished => isFinished;
        public bool IsStarted => isStarted;

        public SurveyStep(string name)
        {
            this.name = name;
        }

        public virtual void Start()
        {
            isStarted = true;
            finishState = SurveyFinishState.None;
            isFinished = false;
        }

        protected void Finish(SurveyFinishState finishState)
        {
            isFinished = true;
            this.finishState = finishState;
        }

        public virtual void Commit()
        {
            isFinished = true;
        }

        public virtual void Rollback()
        {
            isStarted = false;
            finishState = SurveyFinishState.None;
        }

        public abstract void DrawContent();

        public virtual void CleanUp()
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("SurveyStep[").Append(name).Append(", ").Append(nameof(isStarted)).Append(":")
                .Append(isStarted).Append(", ").Append(nameof(isFinished)).Append(":").Append(isFinished).Append(", ")
                .Append(nameof(finishState)).Append(":").Append(finishState).Append("]");

            return builder.ToString();
        }
    }
}