using SpriteSortingPlugin.Survey.UI.Wizard.Data;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class FinishingSurvey : SurveyStep
    {
        private UserData userData;

        private const string FinishingSpritePathAndName =
            "Assets/SpriteSortingPlugin/Editor/Survey/UI/Wizard/SurveySteps/firework2.jpg";

        private Sprite finishingSprite;

        public FinishingSurvey(string name, UserData userData) : base(name)
        {
            this.userData = userData;
        }

        public override void DrawContent()
        {
            // if (finishingSprite == null)
            // {
            //     LoadFinishingSprite();
            // }
            //
            //
            // var spritePosition = new Rect(0, 0, 300, 300);
            // EditorGUI.DrawTextureTransparent(spritePosition, finishingSprite.texture, ScaleMode.ScaleToFit);


            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField(
                    "Thank you very much for participating in this survey. Your input helps me very much!",
                    Styling.QuestionLabelStyle);

                GUILayout.Label("picture");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "If you want to be informed about the results or to keep updated, you can optionally fill in your mail address:",
                    Styling.LabelWrapStyle);

                userData.mailAddress = EditorGUILayout.TextField("Your Email", userData.mailAddress);

                EditorGUILayout.Space(30);
                EditorGUILayout.LabelField("And now the final step: sending the data.", Styling.LabelWrapStyle);
                EditorGUILayout.LabelField(
                    "Please keep this window open and make sure the PC is connected to the internet.",
                    Styling.LabelWrapStyle);
            }
        }

        private void LoadFinishingSprite()
        {
            // finishingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FinishingSpritePathAndName);
        }
    }
}