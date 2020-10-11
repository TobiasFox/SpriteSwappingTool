using System;
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

            var rightAreaRect = new Rect(leftBarWidth, lastHeight, position.width - leftBarWidth, position.height);

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

                var spriteAspectRatio = (float) selectedSprite.texture.width / (float) selectedSprite.texture.height;
                var rectAspectRatio = rightAreaRect.width / rightAreaRect.height;

                float textureHeight;
                float textureWidth;

                // set rect and sprite in ratio and adjust bounding box according to the ScalingMode ScaleToFit
                if (rectAspectRatio > spriteAspectRatio)
                {
                    //rectangle is longer than sprite's width
                    var num2 = spriteAspectRatio / rectAspectRatio;

                    textureWidth = num2 * rightAreaRect.width;
                    textureHeight = textureWidth / spriteAspectRatio;
                }
                else
                {
                    var num3 = rectAspectRatio / spriteAspectRatio;

                    textureHeight = num3 * rightAreaRect.height;
                    textureWidth = textureHeight * spriteAspectRatio;
                }

                var scaleXFactor = textureWidth / selectedSprite.bounds.size.x;
                var scaleYFactor = textureHeight / selectedSprite.bounds.size.y;

                var newBoundsWidth = scaleXFactor * selectedOOBB.OwnBounds.size.x;
                var newBoundsHeight = scaleYFactor * selectedOOBB.OwnBounds.size.y;

                var scaledSize = new Vector3(newBoundsWidth, newBoundsHeight);
                var rectCenter = new Vector3(rightAreaRect.width / 2, rightAreaRect.height / 2);

                Handles.DrawWireCube(rectCenter, scaledSize);
                Handles.DrawWireCube(rectCenter, new Vector3(scaledSize.x + 1, scaledSize.y + 1));

                {
                    GUILayout.BeginArea(new Rect(0, position.height - 125, position.width, 100));
                    var alphaRectangleBorderRect = new Rect(0, 0, position.width, 125);
                    EditorGUI.DrawRect(alphaRectangleBorderRect,
                        ReordableBackgroundColors.TransparentBackgroundColor);

                    EditorGUI.BeginChangeCheck();

                    DrawAlphaRectangleBorder(alphaRectangleBorderRect);

                    if (EditorGUI.EndChangeCheck())
                    {
                        selectedOOBB.UpdateBosSizeWithBorder();
                    }

                    GUILayout.EndArea();
                }
            }

            GUILayout.EndArea();
        }

        private void DrawAlphaRectangleBorder(Rect rect)
        {
            var intFieldLength = rect.width / 4f;
            var spriteWidth = selectedOOBB.alphaRectangleBorder.spriteWidth;
            var spriteHeight = selectedOOBB.alphaRectangleBorder.spriteHeight;

            EditorGUI.LabelField(new Rect(rect.width / 3, rect.y, 90, EditorGUIUtility.singleLineHeight),
                "Adjust Borders");

            rect.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            selectedOOBB.alphaRectangleBorder.topBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                selectedOOBB.alphaRectangleBorder.topBorder, 0, spriteHeight);

            rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;

            {
                selectedOOBB.alphaRectangleBorder.leftBorder = EditorGUI.IntSlider(
                    new Rect(rect.x, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                    selectedOOBB.alphaRectangleBorder.leftBorder, 0, spriteWidth);

                EditorGUI.LabelField(
                    new Rect(rect.width / 2 - (intFieldLength / 2) - (moveIcon.width / 2f),
                        rect.y - (moveIcon.height / 3f), intFieldLength, EditorGUIUtility.singleLineHeight * 2),
                    new GUIContent(moveIcon));


                selectedOOBB.alphaRectangleBorder.rightBorder = EditorGUI.IntSlider(
                    new Rect(rect.x + 2 * intFieldLength, rect.y, intFieldLength,
                        EditorGUIUtility.singleLineHeight),
                    selectedOOBB.alphaRectangleBorder.rightBorder, 0, spriteWidth);

                rect.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;
            }

            selectedOOBB.alphaRectangleBorder.bottomBorder = EditorGUI.IntSlider(
                new Rect(rect.x + intFieldLength, rect.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                selectedOOBB.alphaRectangleBorder.bottomBorder, 0, spriteHeight);
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