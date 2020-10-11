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

        private Camera previewGameObject;

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

            if (selectedSprite != null)
            {
                var textureRect = new Rect(0, 0, rightAreaRect.width, rightAreaRect.height);
                EditorGUI.DrawTextureTransparent(textureRect, selectedSprite.texture, ScaleMode.ScaleToFit);
            }

            if (selectedOOBB != null)
            {
                Handles.color = Color.green;

                var num1 = rightAreaRect.width / rightAreaRect.height;
                var aspectRatio = selectedSprite.texture.width / selectedSprite.texture.height;
                var num3 = num1 / aspectRatio;

                var textureWidth = rightAreaRect.width;
                var textureHeight = num3 * rightAreaRect.height;

                var scaleXFactor = textureWidth / selectedSprite.bounds.size.x;
                var scaleYFactor = textureHeight / selectedSprite.bounds.size.y;

                var newBoundsWidth = scaleXFactor * selectedOOBB.OwnBounds.size.x;
                var newBoundsHeight = scaleYFactor * selectedOOBB.OwnBounds.size.y;

                var scaledSize = new Vector3(newBoundsWidth, newBoundsHeight);
                var rectCenter = new Vector3(rightAreaRect.width / 2, rightAreaRect.height / 2);

                Handles.DrawWireCube(rectCenter, scaledSize);
                Handles.DrawWireCube(rectCenter, new Vector3(scaledSize.x + 1, scaledSize.y + 1));
            }
            // {
            //     GUILayout.BeginArea(new Rect(0, position.height - 100, position.width, 100));
            //     EditorGUI.DrawRect(new Rect(0, 0, position.width, 100), ReordableBackgroundColors.TransparentBackgroundColor);
            //     
            //
            //     GUILayout.EndArea();
            // }


            GUILayout.EndArea();
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