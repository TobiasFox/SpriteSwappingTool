using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class HowToDescription
    {
        private bool isFoldable;
        private bool isExpanded;
        private bool isCameraInformationExpanded;
        private bool isOutlinePrecisionInformationExpanded;
        private bool isSpriteDataInformationExpanded;

        public bool isBoldHeader = true;

        public HowToDescription(bool isFoldable = true, bool isInitialExpanded = false)
        {
            this.isFoldable = isFoldable;
            if (isInitialExpanded)
            {
                isExpanded = true;
            }
        }

        public void DrawHowTo()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label($"How to use the {GeneralData.FullDetectorName}",
                    isBoldHeader ? Styling.CenteredStyleBold : Styling.CenteredStyle);

                if (isFoldable)
                {
                    DrawHeaderFoldout();

                    if (!isExpanded)
                    {
                        return;
                    }
                }

                EditorGUILayout.LabelField("1. Select a Camera");
                using (new EditorGUI.IndentLevelScope())
                {
                    isCameraInformationExpanded = EditorGUILayout.Foldout(isCameraInformationExpanded,
                        new GUIContent("Information"), true);

                    if (isCameraInformationExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(
                                "Select a camera which renders the Scene. It is necessary to determine visual glitches.",
                                Styling.LabelWrapStyle);
                        }
                    }
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("2. Optionally: Adjust Outline Precision");
                using (new EditorGUI.IndentLevelScope())
                {
                    isOutlinePrecisionInformationExpanded = EditorGUILayout.Foldout(
                        isOutlinePrecisionInformationExpanded,
                        new GUIContent("Information"), true);

                    if (isOutlinePrecisionInformationExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            var labelWrapRichStyle = new GUIStyle(Styling.LabelWrapStyle) {richText = true};
                            EditorGUILayout.LabelField(
                                "The default Sprite outline is an \"Axis Aligned Bounding Box\" (<b>AABB</b>) also known as Unity-Bounds.",
                                labelWrapRichStyle);
                            EditorGUILayout.Space(2.5f);
                            EditorGUILayout.LabelField(
                                "It is the fastest way to detect visual glitches. However, it could happen that SpriteRenderers are identified, which overlap with transparent pixels only.",
                                Styling.LabelWrapStyle);
                            EditorGUILayout.Space(2.5f);
                            EditorGUILayout.LabelField(
                                $"Therefore, more accurate outlines can be created with this tool: \"Object Aligned Bounding Box\" (<b>OOBB</b>) and \"<b>Pixel Perfect</b>\".",
                                labelWrapRichStyle);
                            EditorGUILayout.Space(2.5f);
                            EditorGUILayout.LabelField(
                                "However, the more accurate an outline is, the more time is needed.",
                                Styling.LabelWrapStyle);
                            EditorGUILayout.Space(2.5f);

                            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Label("Trade-off (fast vs. accurate)");
                                    GUILayout.FlexibleSpace();
                                }

                                UIUtil.DrawHorizontalLine(true);

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
                                    GUILayout.Label("Fast");
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.LabelField("Accurate");
                                }

                                UIUtil.DrawHorizontalLine();


                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    GUILayout.Space((EditorGUI.indentLevel + 1) * 10);
                                    GUILayout.Label("AABB");
                                    GUILayout.Label("OOBB");
                                    GUILayout.Label("Pixel Perfect");
                                }
                            }
                        }
                    }

                    EditorGUILayout.Space(5);

                    isSpriteDataInformationExpanded = EditorGUILayout.Foldout(
                        isSpriteDataInformationExpanded,
                        new GUIContent("If \"Object Aligned Bounding Box\" or \"Pixel Perfect\" is selected"), true);

                    if (isSpriteDataInformationExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(
                                $"Both outlines require a {nameof(SpriteData)} asset. Generate it by open the {GeneralData.DataAnalysisName} window and click \"Analyze & generate new {nameof(SpriteData)}\".",
                                Styling.LabelWrapStyle);
                        }
                    }
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("3. Find glitch");
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("4. Adjust Sorting options");
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Tip: Drag to reorder and use preview"));
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("5. Confirm");
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.LabelField(new GUIContent(
                            $"The {GeneralData.DetectorName} can determine, if new glitches on surrounding SpriteRenderers are produced through the adjusted sorting options and solves them directly."),
                        Styling.LabelWrapStyle);
                }
            }
        }

        private void DrawHeaderFoldout()
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            var foldoutRect = new Rect(0, lastRect.y, 12, lastRect.height);

            isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, GUIContent.none, true);

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && lastRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.button == 0)
                {
                    isExpanded = !isExpanded;
                }

                currentEvent.Use();
            }
        }
    }
}