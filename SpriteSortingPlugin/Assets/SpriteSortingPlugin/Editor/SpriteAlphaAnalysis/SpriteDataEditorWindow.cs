﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    public class SpriteDataEditorWindow : EditorWindow
    {
        private const int MinWidthRightContentBar = 200;
        private const float LineSpacing = 1.5f;

        private static Texture moveIcon;
        private static bool isIconInitialized;

        private SerializedObject serializedObject;

        [SerializeField] private SpriteData spriteData;
        private Sprite selectedSprite;
        private float selectedSpriteAspectRatio;

        private string searchString;
        private bool isShowingSpriteAlphaGUI = true;
        private bool isShowingOOBB = true;
        private Color outlineColor = Color.green;

        private List<SpriteDataItem> spriteList;
        private ReorderableList reorderableSpriteList;
        private SearchField searchField;
        private Vector2 leftBarScrollPosition;
        private float lastHeight;
        private Material material;
        private Shader transparentUnlitShader;

        private SpriteAlphaAnalyzer spriteAlphaAnalyzer;
        private string assetPath = "Assets/SpriteSortingPlugin/SpriteAlphaData";
        private OutlineAnalysisType outlineAnalysisType;
        private OutlinePrecision outlinePrecision;

        // private ObjectOrientedBoundingBoxComponent oobbComponent;
        private SpriteDataItem selectedSpriteDataItem;

        [MenuItem("Window/Sprite Alpha Analysis %e")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteDataEditorWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Alpha Analysis");
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
                var guids = AssetDatabase.FindAssets("DefaultSpriteAlphaData");
                spriteData =
                    AssetDatabase.LoadAssetAtPath<SpriteData>(AssetDatabase.GUIDToAssetPath(guids[0]));
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
                spriteList = new List<SpriteDataItem>();
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

        private Rect lastToolbarRect;

        private void OnGUI()
        {
            // serializedObject.Update();
            {
                var toolbarRect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                EditorGUIUtility.labelWidth = 110;
                spriteData = EditorGUILayout.ObjectField(new GUIContent("Sprite Data Asset"), spriteData,
                    typeof(SpriteData), false, GUILayout.MinWidth(290)) as SpriteData;
                EditorGUIUtility.labelWidth = 0;

                if (GUILayout.Button("Load"))
                {
                    Debug.Log("loaded sprite Alpha Data");
                    LoadSpriteAlphaData();
                    isShowingSpriteAlphaGUI = true;
                }

                // //TODO remove
                // if (GUILayout.Button("Reset List"))
                // {
                //     ResetSpriteList();
                // }

                GUILayout.FlexibleSpace();

                EditorGUIUtility.labelWidth = 80;
                outlineAnalysisType =
                    (OutlineAnalysisType) EditorGUILayout.EnumFlagsField("Outline Type", outlineAnalysisType,
                        GUILayout.MinWidth(200));
                EditorGUIUtility.labelWidth = 0;

                if (Event.current.type == EventType.Repaint)
                {
                    lastToolbarRect = toolbarRect;
                }

                if (GUILayout.Button("Analyze sprite outlines"))
                {
                    Debug.Log("analyze alpha");

                    AnalyzeSpriteAlphas();

                    isShowingSpriteAlphaGUI = true;
                }

                EditorGUILayout.EndHorizontal();
            }
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

            {
                var leftAreaRect = new Rect(0, lastHeight, leftBarWidth, position.height);
                GUILayout.BeginArea(leftAreaRect);

                leftBarScrollPosition = EditorGUILayout.BeginScrollView(leftBarScrollPosition);
                {
                    var style = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
                    GUILayout.Label("Sprites", style, GUILayout.ExpandWidth(true));
                    // oobbComponent =
                    //     EditorGUILayout.ObjectField("HandleTest", oobbComponent, typeof(ObjectOrientedBoundingBoxComponent),
                    //         true) as ObjectOrientedBoundingBoxComponent;

                    outlinePrecision =
                        (OutlinePrecision) EditorGUILayout.EnumPopup("Preview Outline", outlinePrecision);

                    outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);

                    searchField.OnGUI(searchString);

                    if (reorderableSpriteList == null)
                    {
                        InitReordableSpriteList();
                    }

                    reorderableSpriteList.DoLayoutList();

                    EditorGUILayout.EndScrollView();
                }

                GUILayout.EndArea();
            }

            var rightAreaRect = new Rect(leftBarWidth, lastHeight, position.width - leftBarWidth,
                position.height - lastHeight);

            GUILayout.BeginArea(rightAreaRect, new GUIStyle {normal = {background = Texture2D.whiteTexture}});

            // testSprite =
            //     EditorGUILayout.ObjectField("test", testSprite, typeof(Sprite), false,
            //             GUILayout.Height(EditorGUIUtility.singleLineHeight)) as
            //         Sprite;

            if (selectedSpriteDataItem == null)
            {
                GUILayout.EndArea();
                return;
            }

            var textureRect = new Rect(0, 0, rightAreaRect.width, rightAreaRect.height);
            EditorGUI.DrawTextureTransparent(textureRect, selectedSprite.texture, ScaleMode.ScaleToFit);

            var rectWithAlphaBorders = CalculateRectWithAlphaBorders(textureRect);
            Handles.color = outlineColor;

            switch (outlinePrecision)
            {
                case OutlinePrecision.ObjectOrientedBoundingBox:
                    DrawOOBB(rectWithAlphaBorders, rightAreaRect);
                    break;
                case OutlinePrecision.PixelPerfect:
                    DrawOutline(rectWithAlphaBorders);
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawOutline(Rect rectWithAlphaBorders)
        {
            if (selectedSpriteDataItem.outlinePoints == null || selectedSpriteDataItem.outlinePoints.Count < 2)
            {
                return;
            }

            var scaleXFactor = rectWithAlphaBorders.width / selectedSprite.bounds.size.x;
            var scaleYFactor = rectWithAlphaBorders.height / selectedSprite.bounds.size.y;

            var scale = new Vector2(scaleYFactor, -scaleXFactor);

            var lastPoint = selectedSpriteDataItem.outlinePoints[0] * scale + rectWithAlphaBorders.center;
            for (var i = 1; i < selectedSpriteDataItem.outlinePoints.Count; i++)
            {
                var nextPoint = selectedSpriteDataItem.outlinePoints[i] * scale + rectWithAlphaBorders.center;
                Handles.DrawLine(lastPoint, nextPoint);
                Handles.DrawLine(new Vector2(lastPoint.x + 1, lastPoint.y), new Vector2(nextPoint.x + 1, nextPoint.y));
                lastPoint = nextPoint;
            }
        }

        private void DrawOOBB(Rect rectWithAlphaBorders, Rect rightAreaRect)
        {
            if (selectedSpriteDataItem.objectOrientedBoundingBox == null)
            {
                return;
            }

            var scaleXFactor = rectWithAlphaBorders.width / selectedSprite.bounds.size.x;
            var scaleYFactor = rectWithAlphaBorders.height / selectedSprite.bounds.size.y;

            var newBoundsWidth = scaleXFactor *
                                 (selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteWidth -
                                  selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.leftBorder -
                                  selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.rightBorder) /
                                 selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.pixelPerUnit;
            var newBoundsHeight = scaleYFactor *
                                  (selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteHeight -
                                   selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.topBorder -
                                   selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.bottomBorder) /
                                  selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.pixelPerUnit;

            var scaledSize = new Vector2(newBoundsWidth, newBoundsHeight);
            var rectCenter = rectWithAlphaBorders.center - new Vector2(
                selectedSpriteDataItem.objectOrientedBoundingBox.BoundsCenterOffset.x * scaleXFactor,
                selectedSpriteDataItem.objectOrientedBoundingBox.BoundsCenterOffset.y * scaleYFactor);

            Handles.DrawWireCube(rectCenter, scaledSize);
            Handles.DrawWireCube(rectCenter, new Vector3(scaledSize.x + 1, scaledSize.y + 1));
            Handles.DrawWireCube(rectCenter, new Vector3(scaledSize.x - 1, scaledSize.y - 1));

            {
                GUILayout.BeginArea(new Rect(0, position.height - 125, rightAreaRect.width, 100));
                var alphaRectangleBorderRect = new Rect(0, 0, rightAreaRect.width, 125);
                EditorGUI.DrawRect(alphaRectangleBorderRect, ReordableBackgroundColors.TransparentBackgroundColor);

                EditorGUI.BeginChangeCheck();

                DrawAlphaRectangleBorderSettings(alphaRectangleBorderRect);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(spriteData, "changed OOBB size");
                    selectedSpriteDataItem.objectOrientedBoundingBox.UpdateBoxSizeWithBorder();
                }

                GUILayout.EndArea();
            }
        }

        private Rect CalculateRectWithAlphaBorders(Rect textureRect)
        {
            var rectAspectRatio = textureRect.width / textureRect.height;

            Rect displayingTextureRect;

            // set rect and sprite in ratio and adjust bounding box according to the ScalingMode ScaleToFit, from UnityEngine.GUI.CalculateScaledTextureRects
            //https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Modules/IMGUI/GUI.cs#L262
            if (rectAspectRatio > selectedSpriteAspectRatio)
            {
                //rectangle is longer than sprite's width
                var num2 = selectedSpriteAspectRatio / rectAspectRatio;

                displayingTextureRect =
                    new Rect(textureRect.xMin + (float) ((double) textureRect.width * (1.0 - (double) num2) * 0.5),
                        textureRect.yMin, num2 * textureRect.width, textureRect.height);
            }
            else
            {
                var num3 = rectAspectRatio / selectedSpriteAspectRatio;

                displayingTextureRect = new Rect(textureRect.xMin,
                    textureRect.yMin + (float) ((double) textureRect.height * (1.0 - (double) num3) * 0.5),
                    textureRect.width, num3 * textureRect.height);
            }

            return displayingTextureRect;
        }

        private void DrawAlphaRectangleBorderSettings(Rect rect)
        {
            var intFieldLength = rect.width / 3f;
            var halfSpriteWidth = selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteWidth / 2;
            var halfSpriteHeight =
                selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder.spriteHeight / 2;

            EditorGUI.LabelField(new Rect(rect.width / 2 - 45, rect.y, 90, EditorGUIUtility.singleLineHeight),
                "Adjust Borders");

            if (GUI.Button(new Rect(rect.width - 60, rect.y, 60, EditorGUIUtility.singleLineHeight), "Reset"))
            {
                selectedSpriteDataItem.objectOrientedBoundingBox.ResetAlphaRectangleBorder();
            }

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;
            var alphaBorder = selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder;

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

            selectedSpriteDataItem.objectOrientedBoundingBox.AlphaRectangleBorder = alphaBorder;
        }

        private void LoadSpriteAlphaData()
        {
            if (spriteData == null)
            {
                return;
            }

            spriteList.Clear();
            foreach (var spriteDataItem in spriteData.spriteDataDictionary.Values)
            {
                spriteList.Add(spriteDataItem);
            }
        }

        private void AnalyzeSpriteAlphas()
        {
            if (outlineAnalysisType == OutlineAnalysisType.Nothing)
            {
                return;
            }

            if (spriteAlphaAnalyzer == null)
            {
                spriteAlphaAnalyzer = new SpriteAlphaAnalyzer();
            }

            reorderableSpriteList.index = -1;
            selectedSpriteDataItem = null;
            spriteList.Clear();

            spriteData = CreateInstance<SpriteData>();

            GenerateSpriteDataItems();
            spriteAlphaAnalyzer.AddAlphaShapeToSpriteAlphaData(ref spriteData, outlineAnalysisType);

            var assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(assetPath + "/" + nameof(SpriteData) + ".asset");

            AssetDatabase.CreateAsset(spriteData, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateSpriteDataItems()
        {
            var spriteRenderers = FindObjectsOfType<SpriteRenderer>();

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (!spriteRenderer.enabled || !spriteRenderer.gameObject.activeInHierarchy ||
                    spriteRenderer.sprite == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID());
                var guid = AssetDatabase.AssetPathToGUID(path);

                if (spriteData.spriteDataDictionary.ContainsKey(guid))
                {
                    continue;
                }

                var spriteDataItem = new SpriteDataItem(guid, spriteRenderer.sprite.name);
                spriteData.spriteDataDictionary.Add(guid, spriteDataItem);

                spriteList.Add(spriteDataItem);
            }
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
            EditorGUI.LabelField(rect, oobb.AssetName);
        }

        private void OnSpriteSelected(ReorderableList list)
        {
            if (list.index < 0)
            {
                return;
            }

            selectedSpriteDataItem = spriteList[list.index];

            var path = AssetDatabase.GUIDToAssetPath(selectedSpriteDataItem.AssetGuid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            selectedSprite = sprite;
            selectedSpriteAspectRatio = (float) selectedSprite.texture.width / (float) selectedSprite.texture.height;
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