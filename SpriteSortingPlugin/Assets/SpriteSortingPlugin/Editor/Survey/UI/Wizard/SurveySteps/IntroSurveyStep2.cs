using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class IntroSurveyStep2 : SurveyStep
    {
        private static readonly string[] PartNames = new string[]
        {
            "General Questions",
            "Comparing manual sorting and using the Sprite Swapping tool",
            "Evaluation of the functionality to generate sorting order suggestions",
            "Usability"
        };

        public IntroSurveyStep2(string name) : base(name)
        {
        }

        public override void Commit()
        {
            base.Commit();
            Finish(SurveyFinishState.Succeeded);
        }

        public override void DrawContent()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "This editor window guides you through the survey and sends the data back to me. Therefore, please let this window the whole time opened and make sure this PC is connected to the internet to send the generated data.",
                Styling.LabelWrapStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("The survey consist of 4 parts:");

            using (new EditorGUI.IndentLevelScope(2))
            {
                for (var i = 0; i < PartNames.Length; i++)
                {
                    var content = new GUIContent((i + 1) + ". " + PartNames[i]);
                    
                    EditorGUILayout.LabelField(content, Styling.LabelWrapStyle);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "If you want to be informed about the results or to keep updated, you can optionally enter your mail address at the end of this survey.",
                Styling.LabelWrapStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.indentLevel--;
        }
    }
}