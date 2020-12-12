using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class IntroSurveyStep : SurveyStep
    {
        public IntroSurveyStep(string name) : base(name)
        {
        }

        public override void Commit()
        {
            Debug.Log("Intro completed");
        }

        public override void Rollback()
        {
            Debug.Log("rollback Intro");
        }

        public override void DrawContent()
        {
            GUILayout.Label("Intro ");
        }
    }
}