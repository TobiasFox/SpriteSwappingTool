using System.Collections.Generic;
using System.IO;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class FinishingSurvey : SurveyStep
    {
        private const float FireworkLabelHeight = 256;
        private const float CurrentSpriteWidth = 200;
        private const float SpriteDisplayTime = 0.045f;
        private const float ProgressbarSpeed = 0.45f;
        private const string FireworkSpriteName = "Confetti_small.png";

        private static readonly string[] FireworkFolder = new string[]
        {
            "Assets", "SpriteSortingPlugin", "Editor", "Survey", "Resources"
        };

        private bool isSendingDataButtonPressed;
        private TransmitResult transmitResult;

        private bool isEditorUpdateMethodAdded;

        private double lastEditorTime;
        private float progressValue;
        private bool isUpdatingProgressbar;

        private bool isChangingFireWorkSprite;
        private float remainingTimeToChangeSprite;
        private List<Sprite> fireWorkSprites = new List<Sprite>();
        private int currentSpriteIndex;
        private Sprite currentSprite;
        private Texture2D spriteTexture;

        public bool IsSendingDataButtonPressedThisFrame { get; private set; }

        public FinishingSurvey(string name) : base(name)
        {
        }

        public override void DrawContent()
        {
            if (fireWorkSprites.Count == 0)
            {
                LoadFinishingSprite();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Height(FireworkLabelHeight)))
                {
                    GUILayout.FlexibleSpace();
                    var labelStyle = new GUIStyle(Styling.QuestionLabelStyle) {alignment = TextAnchor.MiddleCenter};
                    labelStyle.fontSize++;
                    EditorGUILayout.LabelField(
                        "Thank you very much for participating in this survey.\nYour input helps me very much!",
                        labelStyle);
                    GUILayout.FlexibleSpace();
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.VerticalScope(GUILayout.Height(FireworkLabelHeight)
                ))
                {
                    GUILayout.FlexibleSpace();
                    DrawCurrentSprite();
                    GUILayout.FlexibleSpace();
                }
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.Space(30);
                EditorGUILayout.LabelField(
                    "Please keep this window open and make sure the PC is connected to the internet.",
                    Styling.LabelWrapStyle);
            }

            using (new EditorGUI.DisabledScope(isSendingDataButtonPressed))
            {
                IsSendingDataButtonPressedThisFrame = GUILayout.Button("Send data",
                    GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                if (IsSendingDataButtonPressedThisFrame)
                {
                    StartSendingData();
                }
            }

            var progressbarRect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(progressbarRect, progressValue, "");

            switch (transmitResult)
            {
                case TransmitResult.Succeeded:
                    var labelWrapStyle = new GUIStyle(Styling.LabelWrapStyle) {alignment = TextAnchor.MiddleCenter};
                    EditorGUILayout.LabelField("The data was successfully sent!", labelWrapStyle);
                    EditorGUILayout.Space(15);
                    EditorGUILayout.LabelField("You can now close this window.", labelWrapStyle);
                    break;
                case TransmitResult.Failed:
                    var labelCenteredWrapStyle = new GUIStyle(Styling.LabelWrapStyle)
                        {alignment = TextAnchor.MiddleCenter};
                    EditorGUILayout.LabelField("Some error occured, while sending the data. Please try again. ",
                        labelCenteredWrapStyle);
                    break;
            }

            // if (GUILayout.Button("Debug: successful send Data"))
            // {
            //     UpdateUIAfterSuccessfullySendData();
            // }
        }

        public void UpdateWithSendResult(TransmitResult transmitResult)
        {
            this.transmitResult = transmitResult;
            isUpdatingProgressbar = false;

            switch (transmitResult)
            {
                case TransmitResult.Succeeded:
                    progressValue = 1;
                    break;
                case TransmitResult.Failed:
                    progressValue = 0;
                    isSendingDataButtonPressed = false;
                    break;
            }
        }

        public override void CleanUp()
        {
            RemoveEditorUpdate();
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = 1;
            return totalProgress;
        }

        private void StartSendingData()
        {
            isSendingDataButtonPressed = true;
            lastEditorTime = EditorApplication.timeSinceStartup;
            isUpdatingProgressbar = true;

            AddEditorUpdate();
        }

        private void DrawCurrentSprite(float width = 0)
        {
            var rect = GUILayoutUtility.GetRect(CurrentSpriteWidth, CurrentSpriteWidth);

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var spriteRect = currentSprite.rect;
            spriteRect.xMin /= spriteTexture.width;
            spriteRect.xMax /= spriteTexture.width;
            spriteRect.yMin /= spriteTexture.height;
            spriteRect.yMax /= spriteTexture.height;
            GUI.DrawTextureWithTexCoords(rect, spriteTexture, spriteRect);
        }

        private void UpdateUIAfterSuccessfullySendData()
        {
            isUpdatingProgressbar = false;
            progressValue = 1;
        }

        private void EditorUpdate()
        {
            if (isChangingFireWorkSprite)
            {
                if (EditorApplication.timeSinceStartup > remainingTimeToChangeSprite)
                {
                    remainingTimeToChangeSprite = (float) (EditorApplication.timeSinceStartup + SpriteDisplayTime);
                    currentSpriteIndex = (currentSpriteIndex + 1) % fireWorkSprites.Count;
                    currentSprite = fireWorkSprites[currentSpriteIndex];
                }
            }

            if (isUpdatingProgressbar)
            {
                var scaledEditorDeltaTime =
                    (float) (EditorApplication.timeSinceStartup - lastEditorTime) * ProgressbarSpeed;

                progressValue += scaledEditorDeltaTime;
                if (progressValue > 1)
                {
                    progressValue = 0;
                }

                lastEditorTime = EditorApplication.timeSinceStartup;
            }
        }

        private void LoadFinishingSprite()
        {
            var pathAndName = Path.Combine(Path.Combine(FireworkFolder), FireworkSpriteName);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(pathAndName);
            foreach (var asset in allAssets)
            {
                switch (asset)
                {
                    case Sprite fireworkSprite:
                        fireWorkSprites.Add(fireworkSprite);
                        break;
                    case Texture2D texture:
                        spriteTexture = texture;
                        break;
                }
            }

            currentSprite = fireWorkSprites[currentSpriteIndex];
            isChangingFireWorkSprite = true;
            remainingTimeToChangeSprite = (float) (EditorApplication.timeSinceStartup + SpriteDisplayTime);

            AddEditorUpdate();
        }

        private void AddEditorUpdate()
        {
            if (isEditorUpdateMethodAdded)
            {
                return;
            }

            isEditorUpdateMethodAdded = true;
            EditorApplication.update += EditorUpdate;
        }

        private void RemoveEditorUpdate()
        {
            if (!isEditorUpdateMethodAdded)
            {
                return;
            }

            EditorApplication.update -= EditorUpdate;
            isEditorUpdateMethodAdded = false;
        }
    }
}