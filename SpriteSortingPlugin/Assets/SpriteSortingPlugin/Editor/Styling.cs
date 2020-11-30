using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public static class Styling
    {
        public static readonly Texture MoveIcon;
        public static readonly Texture WarnIcon;
        public static readonly Texture RemoveIcon;
        public static readonly Texture SpriteIcon;
        public static readonly Texture BaseItemIcon;
        public static readonly Texture SortingGroupIcon;

        public static readonly GUIStyle HelpBoxStyle;
        public static readonly GUIStyle CenteredStyle;
        public static readonly GUIStyle ButtonStyle;

        public static readonly Color ListElementBackground1 = new Color(0.83f, 0.83f, 0.83f);
        public static readonly Color ListElementBackground2 = new Color(0.76f, 0.76f, 0.76f);
        public static readonly Color ListElementActiveColor = new Color(0.1f, 0.69f, 1f, 0.7f);
        public static readonly Color ListElementFocussingColor = new Color(0.45f, 0.77f, 0.95f, 0.91f);
        public static readonly Color TransparentBackgroundColor = new Color(0.76f, 0.76f, 0.76f, 0.7f);
        public static readonly Color SortingCriteriaHeaderBackgroundLightColor = new Color(1f, 1f, 1f, 0.3f);
        public static readonly Color HorizontalLineColor = new Color(0.62f, 0.62f, 0.62f, 1f);

        static Styling()
        {
            MoveIcon = EditorGUIUtility.IconContent("MoveTool@2x").image;
            WarnIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
            RemoveIcon = EditorGUIUtility.IconContent("Toolbar Minus@2x").image;
            SpriteIcon = EditorGUIUtility.IconContent("Sprite Icon").image;
            BaseItemIcon = EditorGUIUtility.IconContent("PreMatCylinder@2x").image;
            SortingGroupIcon = EditorGUIUtility.IconContent("BlendTree Icon").image;

            HelpBoxStyle = new GUIStyle("HelpBox");
            ButtonStyle = new GUIStyle("Button");
            CenteredStyle = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
        }
    }
}