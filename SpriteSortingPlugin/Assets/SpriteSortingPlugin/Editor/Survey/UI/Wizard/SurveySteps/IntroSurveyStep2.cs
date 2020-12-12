using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class IntroSurveyStep2 : SurveyStep
    {
        public IntroSurveyStep2(string name) : base(name)
        {
        }

        public override void Commit()
        {
        }

        public override void Rollback()
        {
        }

        public override void DrawContent()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label(
                "If you want to be informed about the results or to keep updated, you can optionally enter your mail address at the end of this survey.");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label(
                "This editor window guides you through the survey and sends the data back to me. Therefore, please let this window the whole time opened and make sure this PC is connected to the internet to send the generated data.",
                Styling.LabelWrapStyle);
        }

    }
}
