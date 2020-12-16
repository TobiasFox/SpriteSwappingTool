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
        private bool isSendingDataButtonPressed;
        private bool wasDataSendingSuccessfull;
        private float progressValue;
        private double lastEditorTime;
        private bool isUpdatingProgressbarDelegatedAdded;
        private float progressbarSpeed = 0.45f;

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

            progressbarSpeed = OwnGUIHelper.DrawField(progressbarSpeed);

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


                using (new EditorGUI.DisabledScope(isSendingDataButtonPressed))
                {
                    if (GUILayout.Button("Send data", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    {
                        isSendingDataButtonPressed = true;
                        lastEditorTime = EditorApplication.timeSinceStartup;

                        isUpdatingProgressbarDelegatedAdded = true;
                        EditorApplication.update += UpdateProgressbar;
                    }
                }

                var progressbarRect = EditorGUILayout.GetControlRect();
                EditorGUI.ProgressBar(progressbarRect, progressValue, "");

                if (wasDataSendingSuccessfull)
                {
                    EditorGUILayout.LabelField("The data was successfully sent!", Styling.LabelWrapStyle);
                    EditorGUILayout.Space(15);
                    EditorGUILayout.LabelField("You can now close this window.", Styling.LabelWrapStyle);
                }

                if (GUILayout.Button("Debug: successful send Data"))
                {
                    UpdateUIAfterSuccessfullySendData();
                }
            }
        }

        private void UpdateUIAfterSuccessfullySendData()
        {
            wasDataSendingSuccessfull = true;
            isUpdatingProgressbarDelegatedAdded = false;
            progressValue = 1;
            EditorApplication.update -= UpdateProgressbar;
        }

        private void UpdateProgressbar()
        {
            var scaledEditorDeltaTime =
                (float) (EditorApplication.timeSinceStartup - lastEditorTime) * progressbarSpeed;

            progressValue += scaledEditorDeltaTime;
            if (progressValue > 1)
            {
                progressValue = 0;
            }

            lastEditorTime = EditorApplication.timeSinceStartup;
        }

        public override void CleanUp()
        {
            if (isUpdatingProgressbarDelegatedAdded)
            {
                EditorApplication.update -= UpdateProgressbar;
            }
        }

        private void LoadFinishingSprite()
        {
            // finishingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FinishingSpritePathAndName);
        }
    }
}