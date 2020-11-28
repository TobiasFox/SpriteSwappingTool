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

        private PrimaryColorSortingCriterionData PrimaryColorSortingCriterionData =>
            (PrimaryColorSortingCriterionData) sortingCriterionData;

        protected override void OnInspectorGuiInternal()
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Channels");

                if (GUILayout.Button("All",
                    GUILayout.Width(25f), GUILayout.ExpandWidth(false)))
                {
                    for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
                    {
                        PrimaryColorSortingCriterionData.activeChannels[i] = true;
                    }
                }

                if (GUILayout.Button("None", GUILayout.Width(40f), GUILayout.ExpandWidth(false)))
                {
                    for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
                    {
                        PrimaryColorSortingCriterionData.activeChannels[i] = false;
                    }
                }

                for (var i = 0; i < PrimaryColorSortingCriterionData.activeChannels.Length; i++)
                {
                    PrimaryColorSortingCriterionData.activeChannels[i] = GUILayout.Toggle(
                        PrimaryColorSortingCriterionData.activeChannels[i], ChannelNames[i], Styling.ButtonStyle,
                        GUILayout.ExpandWidth(true));
                }
            }

            PrimaryColorSortingCriterionData.backgroundColor = EditorGUILayout.ColorField("Background Color",
                PrimaryColorSortingCriterionData.backgroundColor);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("\u2191 Switch \u2193", GUILayout.Width(152)))
                {
                    var tempColour = PrimaryColorSortingCriterionData.backgroundColor;
                    PrimaryColorSortingCriterionData.backgroundColor = PrimaryColorSortingCriterionData.foregroundColor;
                    PrimaryColorSortingCriterionData.foregroundColor = tempColour;
                }

                GUILayout.FlexibleSpace();
            }

            PrimaryColorSortingCriterionData.foregroundColor = EditorGUILayout.ColorField("Foreground Color",
                PrimaryColorSortingCriterionData.foregroundColor);
        }

        public override string GetTitleName()
        {
            return "Primary Color";
        }
    }
}