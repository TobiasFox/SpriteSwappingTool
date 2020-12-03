using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.UI
{
    public static class Styling
    {
        public static readonly Texture MoveIcon;
        public static readonly Texture WarnIcon;
        public static readonly Texture RemoveIcon;
        public static readonly Texture SpriteIcon;
        public static readonly Texture BaseItemIcon;
        public static readonly Texture SortingGroupIcon;
        public static readonly Texture InfoIcon;

        public static readonly GUIStyle HelpBoxStyle;
        public static readonly GUIStyle CenteredStyleBold;
        public static readonly GUIStyle CenteredStyle;
        public static readonly GUIStyle ButtonStyle;

        //light
        private static readonly Color ListElementBackground1Light = new Color(0.83f, 0.83f, 0.83f);
        private static readonly Color ListElementBackground2Light = new Color(0.76f, 0.76f, 0.76f);
        private static readonly Color ListElementActiveColorLight = new Color(0.1f, 0.69f, 1f, 0.7f);
        private static readonly Color ListElementFocussingColorLight = new Color(0.45f, 0.77f, 0.95f, 0.91f);
        private static readonly Color TransparentBackgroundColorLight = new Color(0.76f, 0.76f, 0.76f, 0.7f);
        private static readonly Color SortingCriteriaHeaderBackgroundColorLight = new Color(1f, 1f, 1f, 0.3f);
        private static readonly Color HorizontalLineColorLight = new Color(0.62f, 0.62f, 0.62f, 1f);
        private static readonly Color SpriteDataPreviewSpriteBackgroundColorLight = new Color(1, 1, 1, 0.1216f);
        private static readonly Color SortingEditorPreviewSpriteBackgroundColorLight = new Color(1, 1, 1, 0.7647f);

        private static Texture2D spriteDataOutlinePreviewBackgroundTextureLight;

        private static Texture2D SpriteDataOutlinePreviewBackgroundTextureLight
        {
            get
            {
                if (spriteDataOutlinePreviewBackgroundTextureLight == null)
                {
                    spriteDataOutlinePreviewBackgroundTextureLight = new Texture2D(1, 1, TextureFormat.ARGB32, false)
                        {name = nameof(spriteDataOutlinePreviewBackgroundTextureLight)};
                    spriteDataOutlinePreviewBackgroundTextureLight.SetPixel(0, 0,
                        SpriteDataPreviewSpriteBackgroundColorLight);
                    spriteDataOutlinePreviewBackgroundTextureLight.Apply();
                }


                return spriteDataOutlinePreviewBackgroundTextureLight;
            }
        }

        private static Texture2D spriteSortingEditorSpritePreviewBackgroundTextureLight;

        private static Texture2D SpriteSortingEditorSpriteEditorSpritePreviewBackgroundTextureLight
        {
            get
            {
                if (spriteSortingEditorSpritePreviewBackgroundTextureLight == null)
                {
                    spriteSortingEditorSpritePreviewBackgroundTextureLight =
                        new Texture2D(1, 1, TextureFormat.ARGB32, false)
                            {name = nameof(spriteSortingEditorSpritePreviewBackgroundTextureLight)};
                    spriteSortingEditorSpritePreviewBackgroundTextureLight.SetPixel(0, 0,
                        SortingEditorPreviewSpriteBackgroundColorLight);
                    spriteSortingEditorSpritePreviewBackgroundTextureLight.Apply();
                }

                return spriteSortingEditorSpritePreviewBackgroundTextureLight;
            }
        }

        //dark
        private static readonly Color ListElementBackground1Dark = new Color(0.217f, 0.217f, 0.217f);
        private static readonly Color ListElementBackground2Dark = new Color(0.242f, 0.242f, 0.242f);
        private static readonly Color ListElementActiveColorDark = new Color(0.1275f, 0.2654f, 0.383f, 0.5804f);
        private static readonly Color ListElementFocussingColorDark = new Color(0.1657f, 0.3355f, 0.4811f, 0.5804f);
        private static readonly Color TransparentBackgroundColorDark = new Color(0.217f, 0.217f, 0.217f, 0.7f);
        private static readonly Color SortingCriteriaHeaderBackgroundColorDark = new Color(0.22f, 0.22f, 0.22f, 0.3f);

        private static readonly Color SortingCriteriaHeaderInspectorBackgroundColorDark =
            new Color(0.283f, 0.283f, 0.283f, 0.3f);

        private static readonly Color HorizontalLineColorDark = new Color(0.36f, 0.36f, 0.36f, 0.6f);

        private static readonly Color spritePreviewBackgroundTextureColorDark =
            new Color(0.1981f, 0.1981f, 0.1981f, 0.7f);

        private static Texture2D spriteSortingPreviewBackgroundTextureDark;

        private static Texture2D SpritePreviewBackgroundTextureDark
        {
            get
            {
                if (spriteSortingPreviewBackgroundTextureDark == null)
                {
                    spriteSortingPreviewBackgroundTextureDark = new Texture2D(1, 1, TextureFormat.ARGB32, false)
                        {name = nameof(spriteSortingPreviewBackgroundTextureDark)};
                    spriteSortingPreviewBackgroundTextureDark.SetPixel(0, 0, spritePreviewBackgroundTextureColorDark);
                    spriteSortingPreviewBackgroundTextureDark.Apply();
                }

                return spriteSortingPreviewBackgroundTextureDark;
            }
        }

        public static Color ListElementBackground1 =>
            EditorGUIUtility.isProSkin ? ListElementBackground1Dark : ListElementBackground1Light;

        public static Color ListElementBackground2 =>
            EditorGUIUtility.isProSkin ? ListElementBackground2Dark : ListElementBackground2Light;

        public static Color ListElementActiveColor =>
            EditorGUIUtility.isProSkin ? ListElementActiveColorDark : ListElementActiveColorLight;

        public static Color ListElementFocussingColor =>
            EditorGUIUtility.isProSkin ? ListElementFocussingColorDark : ListElementFocussingColorLight;

        public static Color TransparentBackgroundColor =>
            EditorGUIUtility.isProSkin ? TransparentBackgroundColorDark : TransparentBackgroundColorLight;

        public static Color SortingCriteriaHeaderBackgroundColor =>
            EditorGUIUtility.isProSkin
                ? SortingCriteriaHeaderBackgroundColorDark
                : SortingCriteriaHeaderBackgroundColorLight;

        public static Color SortingCriteriaInspectorHeaderBackgroundColor =>
            EditorGUIUtility.isProSkin
                ? SortingCriteriaHeaderInspectorBackgroundColorDark
                : SortingCriteriaHeaderBackgroundColorLight;

        public static Color HorizontalLineColor =>
            EditorGUIUtility.isProSkin ? HorizontalLineColorDark : HorizontalLineColorLight;

        public static Texture2D SpriteSortingPreviewBackgroundTexture =>
            EditorGUIUtility.isProSkin
                ? SpritePreviewBackgroundTextureDark
                : SpriteSortingEditorSpriteEditorSpritePreviewBackgroundTextureLight;

        public static Texture2D SpriteDataEditorOutlinePreviewBackgroundTexture =>
            EditorGUIUtility.isProSkin
                ? SpritePreviewBackgroundTextureDark
                : SpriteDataOutlinePreviewBackgroundTextureLight;

        static Styling()
        {
            MoveIcon = EditorGUIUtility.IconContent("MoveTool@2x").image;
            WarnIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
            RemoveIcon = EditorGUIUtility.IconContent("Toolbar Minus@2x").image;
            SpriteIcon = EditorGUIUtility.IconContent("Sprite Icon").image;
            BaseItemIcon = EditorGUIUtility.IconContent("PreMatCylinder@2x").image;
            SortingGroupIcon = EditorGUIUtility.IconContent("BlendTree Icon").image;
            InfoIcon = EditorGUIUtility.IconContent("console.infoicon.sml").image;

            HelpBoxStyle = new GUIStyle("HelpBox");
            ButtonStyle = new GUIStyle("Button");
            CenteredStyleBold = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
            CenteredStyle = new GUIStyle(EditorStyles.boldLabel)
                {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal};
        }
    }
}