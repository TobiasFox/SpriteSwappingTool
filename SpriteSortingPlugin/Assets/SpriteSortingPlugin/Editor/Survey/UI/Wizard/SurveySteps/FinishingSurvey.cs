#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using System.Collections.Generic;
using System.IO;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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

        private static readonly string[] TestExampleScenesFolderPath = new string[]
        {
            "Assets", "_Scenes", "TestExamples"
        };

        private bool isSendingDataButtonPressed;
        private bool isSendingDataButtonPressedThisFrame;
        private TransmitResult transmitResult;
        private TransmitResult lastTransmitResult;
        private string resultDataFolder;
        private string resultDataName;
        private float lastPathLabelHeight;

        private bool isEditorUpdateMethodAdded;

        private double lastEditorTime;
        private float progressValue;
        private bool isUpdatingProgressbar;

        private bool isChangingFireWorkSprite;
        private float remainingTimeToChangeSprite;
        private List<Sprite> fireworkSprites = new List<Sprite>();
        private int currentSpriteIndex;
        private Sprite currentSprite;
        private Texture2D spriteTexture;
        private GUIStyle labelWrapStyle;

        private HowToDescription howToDescription;
        private AutoSortingHowToDescription autoSortingHowToDescription;

        public FinishingSurvey(string name) : base(name)
        {
            labelWrapStyle = new GUIStyle(Styling.LabelWrapStyle) {alignment = TextAnchor.MiddleCenter};
            howToDescription = new HowToDescription() {isBoldHeader = false};
            autoSortingHowToDescription = new AutoSortingHowToDescription(){isBoldHeader = false};
        }

        public override void Start()
        {
            base.Start();
            if (transmitResult == TransmitResult.NotFinished)
            {
                isUpdatingProgressbar = true;
            }
        }

        public override void DrawContent()
        {
            if (fireworkSprites.Count == 0)
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
                    labelWrapStyle);
            }

            var progressbarRect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(progressbarRect, progressValue, "");

            if (Event.current.type == EventType.Layout)
            {
                lastTransmitResult = transmitResult;
            }

            switch (lastTransmitResult)
            {
                case TransmitResult.Succeeded:
                    DrawSendingDataSucceeded();
                    break;
                case TransmitResult.Failed:
                    DrawSendingDataFailed();
                    break;
            }
        }

        private void DrawSendingDataSucceeded()
        {
            GUILayout.Space(25);
            var greenFinishStyle = new GUIStyle(labelWrapStyle)
            {
                normal = {background = Styling.SpriteSortingNoSortingOrderIssuesBackgroundTexture}
            };
            var successfullySentLabel =
                new GUIContent("Data was successfully sent!", Styling.NoSortingOrderIssuesIcon);
            EditorGUILayout.LabelField(successfullySentLabel, greenFinishStyle);
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("You can close this window now.", labelWrapStyle);

            EditorGUILayout.Space(15);
            var folderPath = Path.Combine(TestExampleScenesFolderPath);
            EditorGUILayout.LabelField(
                $"If you want, you can test the {GeneralData.Name} tool with the ExampleScenes located at:\n" +
                $"{folderPath}", labelWrapStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show Folder", GUILayout.Width(150)))
                {
                    var path = folderPath;

                    if (folderPath[folderPath.Length - 1].Equals(Path.PathSeparator))
                    {
                        path = folderPath.Substring(0, folderPath.Length - 1);
                    }

                    var testExampleSceneFolder = AssetDatabase.LoadAssetAtPath<Object>(path);
                    Selection.activeObject = testExampleSceneFolder;
                    EditorGUIUtility.PingObject(testExampleSceneFolder);
                }

                GUILayout.FlexibleSpace();
            }

            howToDescription.DrawHowTo();
            autoSortingHowToDescription.DrawHowTo();
        }

        string MyEscapeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }

        private void DrawSendingDataFailed()
        {
            EditorGUILayout.LabelField(
                "I am really sorry! Some error occurred, while sending the data. Please try again and make sure this PC has an active internet connection. Otherwise, please send the data manually using the email and data below.",
                labelWrapStyle);
            GUILayout.Space(25);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isSendingDataButtonPressed))
                {
                    isSendingDataButtonPressedThisFrame = GUILayout.Button("Send data again",
                        GUILayout.Height(EditorGUIUtility.singleLineHeight * 2), GUILayout.ExpandWidth(true));
                    if (isSendingDataButtonPressedThisFrame)
                    {
                        StartSendingData();
                        transmitResult = TransmitResult.NotFinished;
                    }

                    if (GUILayout.Button("Open System-Mail client\n(Attachment needs to be added)",
                        GUILayout.Height(EditorGUIUtility.singleLineHeight * 2), GUILayout.ExpandWidth(false)))
                    {
                        var subject = MyEscapeURL($"[{GeneralData.Name} Tool] Survey result");
                        var body = MyEscapeURL("Additional and optional message:\n");

                        var mailUrl =
                            $"mailto:{GeneralData.DeveloperMailAddress}?subject={subject}&body={body}";
                        Application.OpenURL(mailUrl);
                    }
                }
            }

            GUILayout.Space(25);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Email", GUILayout.ExpandWidth(false), GUILayout.Width(147));
                EditorGUILayout.SelectableLabel(GeneralData.DeveloperMailAddress,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
            }

            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Folder", GUILayout.ExpandWidth(false), GUILayout.Width(147));

                var guiStyle = Styling.LabelWrapStyle;
                if (Event.current.type == EventType.Repaint)
                {
                    var pathLabelWidth = horizontalScope.rect.width - 147;
                    lastPathLabelHeight =
                        guiStyle.CalcHeight(new GUIContent(resultDataFolder), pathLabelWidth);
                }

                EditorGUILayout.SelectableLabel(resultDataFolder, guiStyle,
                    GUILayout.Height(lastPathLabelHeight), GUILayout.ExpandWidth(true));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Zip name", GUILayout.ExpandWidth(false), GUILayout.Width(147));
                EditorGUILayout.SelectableLabel(resultDataName,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
            }

            if (GUILayout.Button("Open data in System-Explorer"))
            {
                EditorUtility.RevealInFinder(Path.Combine(resultDataFolder, resultDataName));
            }
        }

        public bool GetAndConsumeIsSendingDataButtonPressedThisFrame()
        {
            var returnBool = isSendingDataButtonPressedThisFrame;
            isSendingDataButtonPressedThisFrame = false;
            return returnBool;
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

        public void SetResultDataPath(string resultDataFolder, string resultDataName)
        {
            this.resultDataFolder = resultDataFolder;
            this.resultDataName = resultDataName;
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

        private void EditorUpdate()
        {
            if (isChangingFireWorkSprite)
            {
                if (EditorApplication.timeSinceStartup > remainingTimeToChangeSprite)
                {
                    remainingTimeToChangeSprite = (float) (EditorApplication.timeSinceStartup + SpriteDisplayTime);
                    currentSpriteIndex = (currentSpriteIndex + 1) % fireworkSprites.Count;
                    currentSprite = fireworkSprites[currentSpriteIndex];
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
                        fireworkSprites.Add(fireworkSprite);
                        break;
                    case Texture2D texture:
                        spriteTexture = texture;
                        break;
                }
            }

            currentSprite = fireworkSprites[currentSpriteIndex];
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