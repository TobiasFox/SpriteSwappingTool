﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    public class SpriteAlphaEditorWindow : EditorWindow
    {
        private const int MinWidthRightContentBar = 200;
        private const float LineSpacing = 1.5f;

        private static Texture moveIcon;
        private static bool isIconInitialized;

        private SerializedObject serializedObject;

        [SerializeField] private SpriteAlphaData spriteAlphaData;
        private Sprite selectedSprite;

        private string searchString;
        private bool isShowingSpriteAlphaGUI = true;

        private List<ObjectOrientedBoundingBox> spriteList;
        private ReorderableList reorderableSpriteList;
        private SearchField searchField;
        private Vector2 leftBarScrollPosition;
        private float lastHeight;
        private Material material;
        private Shader transparentUnlitShader;

        private SpriteAlphaAnalyzer spriteAlphaAnalyzer;
        private string assetPath = "Assets/SpriteSortingPlugin/SpriteAlphaData";

        // private ObjectOrientedBoundingBoxComponent oobbComponent;
        private ObjectOrientedBoundingBox selectedOOBB;

        [MenuItem("Window/Sprite Alpha Analysis %e")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteAlphaEditorWindow>();
            window.titleContent = new GUIContent("Sprite Alpha Analysis");
            window.Show();
        }

        private void Awake()
        {
            ResetSpriteList();

            //TODO: remove
            SelectDefaultSpriteAlphaData();

            if (!isIconInitialized)
            {
                moveIcon = EditorGUIUtility.IconContent("MoveTool@2x").image;
                isIconInitialized = true;
            }
        }

        private void SelectDefaultSpriteAlphaData()
        {
            try
            {
                var guids = AssetDatabase.FindAssets("SpriteAlphaData 2");
                spriteAlphaData =
                    AssetDatabase.LoadAssetAtPath<SpriteAlphaData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            catch (Exception e)
            {
                Debug.Log("auto selection of SpriteAlphaData went wrong");
            }
        }

        private void ResetSpriteList()
        {
            if (spriteList == null)
            {
                spriteList = new List<ObjectOrientedBoundingBox>();
            }
            else
            {
                spriteList.Clear();
            }
        }

        private void OnEnable()
        {
            // if (serializedObject == null)
            // {
            //     serializedObject = new SerializedObject(this);
            // }

            if (searchField == null)
            {
                searchField = new SearchField();
            }

            if (isShowingSpriteAlphaGUI)
            {
                if (reorderableSpriteList == null)
                {
                    InitReordableSpriteList();
                }

                if (transparentUnlitShader == null)
                {
                    transparentUnlitShader = Shader.Find("Unlit/Transparent");
                }
            }
        }

        private void OnGUI()
        {
            // serializedObject.Update();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            spriteAlphaData = EditorGUILayout.ObjectField(new GUIContent("Sprite Alpha Data Asset"), spriteAlphaData,
                typeof(SpriteAlphaData), false) as SpriteAlphaData;

            if (GUILayout.Button("Load"))
            {
                Debug.Log("loaded sprite Alpha Data");
                LoadSpriteAlphaData();
                isShowingSpriteAlphaGUI = true;
            }

            if (GUILayout.Button("Reset List"))
            {
                // Debug.Log("Save sprite Alpha Data");
                ResetSpriteList();
            }

            if (GUILayout.Button("Analyze Alpha of Sprites"))
            {
                Debug.Log("analyze alpha");

                AnalyzeSpriteAlphas();

                isShowingSpriteAlphaGUI = true;
            }

            EditorGUILayout.EndHorizontal();
            // serializedObject.ApplyModifiedProperties();

            if (!isShowingSpriteAlphaGUI)
            {
                return;
            }

            var leftBarWidth = position.width / 4;

            if (leftBarWidth < MinWidthRightContentBar)
            {
                leftBarWidth = MinWidthRightContentBar;
            }

            if (Event.current.type == EventType.Repaint)
            {
                lastHeight = GUILayoutUtility.GetLastRect().height;
            }

            var leftAreaRect = new Rect(0, lastHeight, leftBarWidth, position.height);
            GUILayout.BeginArea(leftAreaRect);

            leftBarScrollPosition = EditorGUILayout.BeginScrollView(leftBarScrollPosition);
            {
                EditorGUILayout.LabelField("Sprites");
                // oobbComponent =
                //     EditorGUILayout.ObjectField("HandleTest", oobbComponent, typeof(ObjectOrientedBoundingBoxComponent),
                //         true) as ObjectOrientedBoundingBoxComponent;

                searchField.OnGUI(searchString);

                if (reorderableSpriteList == null)
                {
                    InitReordableSpriteList();
                }

                reorderableSpriteList.DoLayoutList();

                EditorGUILayout.EndScrollView();
            }

            GUILayout.EndArea();

            var rightAreaRect = new Rect(leftBarWidth, lastHeight, position.width - leftBarWidth,
                position.height - lastHeight);

            GUILayout.BeginArea(rightAreaRect, new GUIStyle {normal = {background = Texture2D.whiteTexture}});

            // if (reorderableSpriteList.index == -1)
            // {
            //     GUILayout.EndArea();
            //     return;
            // }

            // EditorGUI.BeginChangeCheck();
            // testSprite =
            //     EditorGUILayout.ObjectField("test", testSprite, typeof(Sprite), false,
            //             GUILayout.Height(EditorGUIUtility.singleLineHeight)) as
            //         Sprite;

            if (selectedOOBB != null)
            {
                var textureRect = new Rect(0, 0, rightAreaRect.width, rightAreaRect.height);
                EditorGUI.DrawTextureTransparent(textureRect, selectedSprite.texture, ScaleMode.ScaleToFit);

                Handles.color = Color.green;

                var rectWithAlphaBorders = CalculateDisplayingTextureRect(textureRect);
                rectWithAlphaBorders = ApplyAlphaBorderOffset(ref rectWithAlphaBorders, textureRect);

                DrawDoubleRectangleLines(rectWithAlphaBorders);


                {
                    GUILayout.BeginArea(new Rect(0, position.height - 125, rightAreaRect.width, 100));
                    var alphaRectangleBorderRect = new Rect(0, 0, rightAreaRect.width, 125);
                    EditorGUI.DrawRect(alphaRectangleBorderRect, ReordableBackgroundColors.TransparentBackgroundColor);

                    EditorGUI.BeginChangeCheck();

                    DrawAlphaRectangleBorderSettings(alphaRectangleBorderRect);

                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedOOBB.UpdateBosSizeWithBorder();
                    }

                    GUILayout.EndArea();
                }
            }

            GUILayout.EndArea();
        }

        private static void DrawDoubleRectangleLines(Rect rectWithAlphaBorders)
        {
            var topLeft = new Vector2(rectWithAlphaBorders.x, rectWithAlphaBorders.y);
            var bottomLeft = new Vector2(rectWithAlphaBorders.x,
                rectWithAlphaBorders.y + rectWithAlphaBorders.height);
            var topRight = new Vector2(rectWithAlphaBorders.x + rectWithAlphaBorders.width, rectWithAlphaBorders.y);
            var bottomRight = new Vector2(rectWithAlphaBorders.x + rectWithAlphaBorders.width,
                rectWithAlphaBorders.y + rectWithAlphaBorders.height);

            //top
            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(new Vector2(topLeft.x + 1, topLeft.y + 1),
                new Vector2(topRight.x + 1, topRight.y + 1));

            //right
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(new Vector2(topRight.x + 1, topRight.y + 1),
                new Vector2(bottomRight.x + 1, bottomRight.y + 1));

            //bottom
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(new Vector2(bottomRight.x + 1, bottomRight.y + 1),
                new Vector2(bottomLeft.x + 1, bottomLeft.y + 1));

            //left
            Handles.DrawLine(bottomLeft, topLeft);
            Handles.DrawLine(new Vector2(bottomLeft.x + 1, bottomLeft.y + 1),
                new Vector2(topLeft.x + 1, topLeft.y + 1));
        }

        private Rect ApplyAlphaBorderOffset(ref Rect rectWithAlphaBorders, Rect textureRect)
        {
            var offset = rectWithAlphaBorders.position;
            var pixelHeight = rectWithAlphaBorders.height / selectedOOBB.AlphaRectangleBorder.spriteHeight;
            var pixelWidth = rectWithAlphaBorders.width / selectedOOBB.AlphaRectangleBorder.spriteWidth;

            // top and left
            rectWithAlphaBorders.xMin = pixelWidth * selectedOOBB.AlphaRectangleBorder.leftBorder;
            rectWithAlphaBorders.yMin = pixelHeight * selectedOOBB.AlphaRectangleBorder.topBorder;

            // bottom and right
            rectWithAlphaBorders.xMax = pixelWidth * (selectedOOBB.AlphaRectangleBorder.spriteWidth -
                                                      selectedOOBB.AlphaRectangleBorder.rightBorder);
            rectWithAlphaBorders.yMax = pixelHeight * (selectedOOBB.AlphaRectangleBorder.spriteHeight -
                                                       selectedOOBB.AlphaRectangleBorder.bottomBorder);

            //move position to center the rect again
            rectWithAlphaBorders.position += offset;

            return rectWithAlphaBorders;
        }

        private Rect CalculateDisplayingTextureRect(Rect textureRect)
        {
            var spriteAspectRatio = (float) selectedSprite.texture.width / (float) selectedSprite.texture.height;
            var rectAspectRatio = textureRect.width / textureRect.height;

            // float textureHeight;
            // float textureWidth;
            Rect displayingTextureRect;

            // set rect and sprite in ratio and adjust bounding box according to the ScalingMode ScaleToFit, from UnityEngine.GUI.CalculateScaledTextureRects
            if (rectAspectRatio > spriteAspectRatio)
            {
                //rectangle is longer than sprite's width
                var num2 = spriteAspectRatio / rectAspectRatio;

                displayingTextureRect =
                    new Rect(textureRect.xMin + (float) ((double) textureRect.width * (1.0 - (double) num2) * 0.5),
                        textureRect.yMin, num2 * textureRect.width, textureRect.height);
            }
            else
            {
                var num3 = rectAspectRatio / spriteAspectRatio;

                displayingTextureRect = new Rect(textureRect.xMin,
                    textureRect.yMin + (float) ((double) textureRect.height * (1.0 - (double) num3) * 0.5),
                    textureRect.width, num3 * textureRect.height);
            }

            return displayingTextureRect;
        }

        private void DrawAlphaRectangleBorderSettings(Rect rect)
        {
            var intFieldLength = rect.width / 3f;
            var halfSpriteWidth = selectedOOBB.AlphaRectangleBorder.spriteWidth / 2;
            var halfSpriteHeight = selectedOOBB.AlphaRectangleBorder.spriteHeight / 2;

            EditorGUI.LabelField(new Rect(rect.width / 2 - 45, rect.y, 90, EditorGUIUtility.singleLineHeight),
                "Adjust Borders");

            if (GUI.Button(new Rect(rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), "Reset"))
            {
                selectedOOBB.ResetAlphaRectangleBorder();
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;
            var alphaBorder = selectedOOBB.AlphaRectangleBorder;

            alphaBorder.topBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                alphaBorder.topBorder, 0, halfSpriteHeight);

            rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;

            {
                alphaBorder.leftBorder = EditorGUI.IntSlider(
                    new Rect(rect.x, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                    alphaBorder.leftBorder, 0, halfSpriteWidth);

                EditorGUI.LabelField(new Rect(rect.width / 2 - moveIcon.width,
                    rect.y - (moveIcon.height / 2f) + EditorGUIUtility.singleLineHeight / 2f, moveIcon.width,
                    EditorGUIUtility.singleLineHeight * 2), new GUIContent(moveIcon));

                alphaBorder.rightBorder = EditorGUI.IntSlider(
                    new Rect(rect.x + 2 * intFieldLength, rect.y, intFieldLength,
                        EditorGUIUtility.singleLineHeight),
                    alphaBorder.rightBorder, 0, halfSpriteWidth);

                rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;
            }

            alphaBorder.bottomBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                alphaBorder.bottomBorder, 0, halfSpriteHeight);

            selectedOOBB.AlphaRectangleBorder = alphaBorder;
        }

        private void LoadSpriteAlphaData()
        {
            if (spriteAlphaData == null)
            {
                return;
            }

            spriteList.Clear();
            foreach (var objectOrientedBoundingBox in spriteAlphaData.objectOrientedBoundingBoxDictionary.Values)
            {
                spriteList.Add(objectOrientedBoundingBox);
            }
        }

        private void AnalyzeSpriteAlphas()
        {
            if (spriteAlphaAnalyzer == null)
            {
                spriteAlphaAnalyzer = new SpriteAlphaAnalyzer();
            }

            spriteAlphaAnalyzer.Initialize();

            var oobbList = spriteAlphaAnalyzer.GenerateOOBBs();

            spriteAlphaData = CreateInstance<SpriteAlphaData>();

            spriteList.Clear();

            foreach (var objectOrientedBoundingBox in oobbList)
            {
                spriteList.Add(objectOrientedBoundingBox);
                spriteAlphaData.objectOrientedBoundingBoxDictionary.Add(objectOrientedBoundingBox.assetGuid,
                    objectOrientedBoundingBox);
            }

            var assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(assetPath + "/" + nameof(SpriteAlphaData) + ".asset");

            AssetDatabase.CreateAsset(spriteAlphaData, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void InitReordableSpriteList()
        {
            reorderableSpriteList = new ReorderableList(spriteList, typeof(string), false, false, false, false);
            reorderableSpriteList.onSelectCallback += OnSpriteSelected;
            reorderableSpriteList.drawElementCallback += DrawElementCallBack;
            reorderableSpriteList.drawElementBackgroundCallback += DrawElementBackgroundCallBack;
        }

        private void DrawElementBackgroundCallBack(Rect rect, int index, bool isActive, bool isFocused)
        {
            Color color;
            if (isActive)
            {
                color = ReordableBackgroundColors.ActiveColor;
            }
            else if (isFocused)
            {
                color = ReordableBackgroundColors.FocussingColor;
            }
            else
            {
                color = index % 2 == 0
                    ? ReordableBackgroundColors.BackgroundColor1
                    : ReordableBackgroundColors.BackgroundColor2;
            }

            EditorGUI.DrawRect(rect, color);
        }

        private void DrawElementCallBack(Rect rect, int index, bool isactive, bool isfocused)
        {
            var oobb = spriteList[index];
            EditorGUI.LabelField(rect, oobb.assetName);
        }

        private void OnSpriteSelected(ReorderableList list)
        {
            if (list.index < 0)
            {
                return;
            }

            selectedOOBB = spriteList[list.index];

            var path = AssetDatabase.GUIDToAssetPath(selectedOOBB.assetGuid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            selectedSprite = sprite;
        }

        private void OnDestroy()
        {
            reorderableSpriteList.onSelectCallback = null;
            reorderableSpriteList.drawElementCallback = null;
            reorderableSpriteList.drawElementBackgroundCallback = null;
            reorderableSpriteList = null;
        }
    }
}