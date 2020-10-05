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

        [MenuItem("Window/Sprite Alpha Analysis %e")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteAlphaEditorWindow>();
            window.titleContent = new GUIContent("Sprite Alpha Analysis");
            window.Show();
        }

        private void Awake()
        {
            spriteList = new List<string>
            {
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
                "a", "b", "c",
            };
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

        private float lastHeight;

        private void OnGUI()
        {
            // serializedObject.Update();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.ObjectField(new GUIContent("Sprite Alpha Data Asset"), spriteAlphaData,
                typeof(SpriteAlphaData), false);

            if (GUILayout.Button("Load"))
            {
                Debug.Log("loaded sprite Alpha Data");
                isShowingSpriteAlphaGUI = true;
            }

            if (GUILayout.Button("Save"))
            {
                Debug.Log("Save sprite Alpha Data");
            }

            if (GUILayout.Button("Analyze Alpha of Sprites"))
            {
                Debug.Log("analyze alpha");
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

        private Material material;
        private Shader transparentUnlitShader;

        private void InitReordableSpriteList()
        {
            reorderableSpriteList = new ReorderableList(spriteList, typeof(string), false, false, false, false);
            reorderableSpriteList.onSelectCallback += OnSpriteSelected;
        }

        private void OnSpriteSelected(ReorderableList list)
        {
        }

        private void OnDestroy()
        {
            reorderableSpriteList.onSelectCallback = null;
            reorderableSpriteList = null;
        }
    }
}