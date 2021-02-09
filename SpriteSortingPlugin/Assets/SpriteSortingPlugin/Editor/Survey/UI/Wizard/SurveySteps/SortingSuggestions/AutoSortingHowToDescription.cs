using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class AutoSortingHowToDescription
    {
        private bool isFoldable;
        private bool isExpanded;
        private bool isCameraInformationExpanded;
        private bool isOutlinePrecisionInformationExpanded;
        private bool isSpriteDataInformationExpanded;

        public bool isBoldHeader = true;

        public AutoSortingHowToDescription(bool isFoldable = true)
        {
            this.isFoldable = isFoldable;
        }

        public void DrawHowTo()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label("Sorting Criteria information",
                    isBoldHeader ? Styling.CenteredStyleBold : Styling.CenteredStyle);

                if (isFoldable)
                {
                    DrawHeaderFoldout();

                    if (!isExpanded)
                    {
                        return;
                    }
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField(
                    "Each Criterion is adjustable, if it sorts overlapping SpriteRenderer in the foreground or background along other options.",
                    Styling.LabelWrapStyle);

                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Sorting Criteria overview");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Collapse all"))
                        {
                            for (var i = 0; i < criteriaInformation.Length; i++)
                            {
                                criteriaInformation[i].isExpanded = false;
                            }
                        }

                        if (GUILayout.Button("Expand all"))
                        {
                            for (var i = 0; i < criteriaInformation.Length; i++)
                            {
                                criteriaInformation[i].isExpanded = true;
                            }
                        }
                    }

                    UIUtil.DrawHorizontalLine(true);

                    for (var i = 0; i < criteriaInformation.Length; i++)
                    {
                        var criterion = criteriaInformation[i];

                        criteriaInformation[i].isExpanded =
                            EditorGUILayout.Foldout(criteriaInformation[i].isExpanded, criterion.name, true);

                        if (!criteriaInformation[i].isExpanded)
                        {
                            if (i < criteriaInformation.Length - 1)
                            {
                                UIUtil.DrawHorizontalLine();
                            }

                            continue;
                        }

                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.LabelField(criterion.information, Styling.LabelWrapStyle);
                        }

                        if (i < criteriaInformation.Length - 1)
                        {
                            UIUtil.DrawHorizontalLine();
                        }
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"* Requires or can require a {nameof(SpriteData)} asset");
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

        private CriteriaInformation[] criteriaInformation = new CriteriaInformation[]
        {
            new CriteriaInformation()
            {
                name = "Containment*",
                information = "Compares, if SpriteRenderers are completely enclosed by other SpriteRenderers."
            },
            new CriteriaInformation()
            {
                name = "Size*",
                information = "Compares the size of SpriteRenderers."
            },
            new CriteriaInformation()
            {
                name = "Intersection Area*",
                information =
                    "Calculates the intersection area of overlapping Sprites and sets it in relation to the areas of these overlapping Sprites."
            },
            new CriteriaInformation()
            {
                name = "Camera Distance",
                information = "Compares the distance to a selected Camera."
            },
            new CriteriaInformation()
            {
                name = "Sprite Sort Point*",
                information = "Compares the Sort Points of Sprites by testing if they overlap another Sprite."
            },
            new CriteriaInformation()
            {
                name = "Sprite Resolution",
                information = "Compares Sprites resolutions in pixels."
            },
            new CriteriaInformation()
            {
                name = "Sprite Sharpness*",
                information = "Compares the calculated sharpness of Sprites."
            },
            new CriteriaInformation()
            {
                name = "Perceived Lightness*",
                information = "Compares only the perceived lightness of Sprites."
            },
            new CriteriaInformation()
            {
                name = "Primary Color*",
                information =
                    "Compares the primary color of SpriteRenderers between a selectable foreground and background color."
            },
        };

        private struct CriteriaInformation
        {
            public string name;
            public string information;
            public bool isExpanded;
        }
    }
}