using SpriteSortingPlugin.UI;
using UnityEditor;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class SendingResultSurveyStep : SurveyStep
    {
        public SendingResultSurveyStep(string name) : base(name)
        {
        }

        public override void DrawContent()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("The data was successfully sent!", Styling.LabelWrapStyle);
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("You can now close this window.", Styling.LabelWrapStyle);
            }
        }
    }
}