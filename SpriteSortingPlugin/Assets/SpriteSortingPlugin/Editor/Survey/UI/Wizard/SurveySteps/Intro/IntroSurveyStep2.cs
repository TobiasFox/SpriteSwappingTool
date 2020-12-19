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
            "Comparing manual approach and using the Sprite Swapping tool",
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

            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                EditorGUILayout.LabelField(
                    "This editor window guides you through the survey and sends the data back to me.",
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
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "If you want to be informed about the results or to keep updated, you can optionally enter your mail address at the end of this survey.",
                Styling.LabelWrapStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(new GUIContent(
                "Please let this window the whole time opened and make sure this PC is connected to the internet to send the generated data.",
                Styling.InfoIcon), Styling.LabelWrapStyle);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight / 2f);
            EditorGUILayout.LabelField(new GUIContent(
                "Also, please do not recompile any code while the survey window is open. If you do so, it will result in exceptions due to Unity`s serialization behaviour.",
                Styling.InfoIcon), Styling.LabelWrapStyle);

            EditorGUI.indentLevel--;
        }
    }
}