using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    [CustomEditor(typeof(PrimaryColorSortingCriterionData))]
    public class PrimaryColorCriterionDataEditor : CriterionDataBaseEditor<SortingCriterionData>
    {
        private static readonly string[] ChannelNames = new string[] {"Red", "Green", "Blue"};
        private static readonly int[] ChannelLabelWidth = new[] {42, 50, 42};
        private static GUIStyle buttonStyle;

        private PrimaryColorSortingCriterionData PrimaryColorSortingCriterionData =>
            (PrimaryColorSortingCriterionData) sortingCriterionData;

        protected override void InternalInitialize()
        {
            buttonStyle = new GUIStyle("Button");
        }

        protected override void OnInspectorGuiInternal()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                PrimaryColorSortingCriterionData.isUsingSpriteColor =
                    GUILayout.Toggle(PrimaryColorSortingCriterionData.isUsingSpriteColor, "use Color of Sprite",
                        buttonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    PrimaryColorSortingCriterionData.isUsingSpriteRendererColor =
                        !PrimaryColorSortingCriterionData.isUsingSpriteColor;
                }

                EditorGUI.BeginChangeCheck();
                PrimaryColorSortingCriterionData.isUsingSpriteRendererColor =
                    GUILayout.Toggle(PrimaryColorSortingCriterionData.isUsingSpriteRendererColor,
                        "use Color of SpriteRenderer", buttonStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    PrimaryColorSortingCriterionData.isUsingSpriteColor =
                        !PrimaryColorSortingCriterionData.isUsingSpriteRendererColor;
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Channels");

                if (GUILayout.Button("All",
                    GUILayout.Width(25f), GUILayout.ExpandWidth(false)))
                {
                    for (var i = 0; i < PrimaryColorSortingCriterionData.isChannelActive.Length; i++)
                    {
                        PrimaryColorSortingCriterionData.isChannelActive[i] = true;
                    }
                }

                if (GUILayout.Button("None", GUILayout.Width(40f), GUILayout.ExpandWidth(false)))
                {
                    for (var i = 0; i < PrimaryColorSortingCriterionData.isChannelActive.Length; i++)
                    {
                        PrimaryColorSortingCriterionData.isChannelActive[i] = false;
                    }
                }

                for (var i = 0; i < PrimaryColorSortingCriterionData.isChannelActive.Length; i++)
                {
                    EditorGUIUtility.labelWidth = ChannelLabelWidth[i];
                    PrimaryColorSortingCriterionData.isChannelActive[i] = EditorGUILayout.Toggle(ChannelNames[i],
                        PrimaryColorSortingCriterionData.isChannelActive[i]);
                }

                EditorGUIUtility.labelWidth = 0;
                GUILayout.FlexibleSpace();
            }

            PrimaryColorSortingCriterionData.backgroundColor = EditorGUILayout.ColorField("Background Color",
                PrimaryColorSortingCriterionData.backgroundColor);
            PrimaryColorSortingCriterionData.foregroundColor = EditorGUILayout.ColorField("Foreground Color",
                PrimaryColorSortingCriterionData.foregroundColor);
        }

        public override string GetTitleName()
        {
            return "Primary Color";
        }
    }
}