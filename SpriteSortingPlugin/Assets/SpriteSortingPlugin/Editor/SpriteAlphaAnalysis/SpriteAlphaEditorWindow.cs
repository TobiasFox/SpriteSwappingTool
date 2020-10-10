using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Sprites;
using UnityEditorInternal;
using UnityEngine;

namespace SpriteSortingPlugin.SpriteAlphaAnalysis
{
    public class SpriteAlphaEditorWindow : EditorWindow
    {
        private const int MinWidthRightContentBar = 200;

        private SerializedObject serializedObject;

        [SerializeField] private SpriteAlphaData spriteAlphaData;
        private Sprite testSprite;

        private string searchString;
        private bool isShowingSpriteAlphaGUI = true;

        private List<string> spriteList;
        private ReorderableList reorderableSpriteList;
        private SearchField searchField;
        private Vector2 rightBarScrollPosition;
        private float lastHeight;
        private Material material;
        private Shader transparentUnlitShader;

        private SpriteAlphaAnalyzer spriteAlphaAnalyzer;
        private string assetPath = "Assets/SpriteSortingPlugin/SpriteAlphaData";

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
        }

        private void ResetSpriteList()
        {
            if (spriteList == null)
            {
                spriteList = new List<string>();
            }
            else
            {
                spriteList.Clear();
            }

            for (int i = 0; i < 75; i++)
            {
                var itemName = "a";
                if (i % 2 == 0)
                {
                    itemName = "b";
                }
                else if (i % 3 == 0)
                {
                    itemName = "c";
                }

                spriteList.Add(itemName);
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

            var rightBarWidth = position.width / 4;

            if (rightBarWidth < MinWidthRightContentBar)
            {
                rightBarWidth = MinWidthRightContentBar;
            }

            if (Event.current.type == EventType.Repaint)
            {
                lastHeight = GUILayoutUtility.GetLastRect().height;
            }

            var rightAreaRect = new Rect(0, lastHeight, rightBarWidth, position.height);
            GUILayout.BeginArea(rightAreaRect);

            rightBarScrollPosition = EditorGUILayout.BeginScrollView(rightBarScrollPosition);
            {
                EditorGUILayout.LabelField("Sprites");
                searchField.OnGUI(searchString);

                if (reorderableSpriteList == null)
                {
                    InitReordableSpriteList();
                }

                reorderableSpriteList.DoLayoutList();

                EditorGUILayout.EndScrollView();
            }

            GUILayout.EndArea();

            var leftAreaRect = new Rect(rightBarWidth, lastHeight, position.width - rightBarWidth, position.height);

            GUILayout.BeginArea(leftAreaRect, new GUIStyle {normal = {background = Texture2D.whiteTexture}});

            // if (reorderableSpriteList.index == -1)
            // {
            //     GUILayout.EndArea();
            //     return;
            // }

            EditorGUI.BeginChangeCheck();
            testSprite =
                EditorGUILayout.ObjectField("test", testSprite, typeof(Sprite), false,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight)) as
                    Sprite;

            if (testSprite != null)
            {
                GUI.DrawTexture(new Rect(0, 0, leftAreaRect.width, leftAreaRect.height), testSprite.texture,
                    ScaleMode.ScaleToFit);
            }

            GUILayout.EndArea();
        }

        private void LoadSpriteAlphaData()
        {
            if (spriteAlphaData == null)
            {
                return;
            }

            spriteList.Clear();
            foreach (var guid in spriteAlphaData.objectOrientedBoundingBoxDictionary.Keys)
            {
                spriteList.Add(guid);
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
                spriteList.Add(objectOrientedBoundingBox.assetGuid);
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
        }

        private void OnSpriteSelected(ReorderableList list)
        {
            if (list.index < 0)
            {
                return;
            }

            var guid = (string) list.list[list.index];

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            testSprite = sprite;
            // Repaint();
        }

        private void OnDestroy()
        {
            reorderableSpriteList.onSelectCallback = null;
            reorderableSpriteList = null;
        }
    }
}