using System;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    [Serializable]
    public struct AlphaRectangleBorder
    {
        public int topBorder;
        public int leftBorder;
        public int bottomBorder;
        public int rightBorder;

        public int spriteWidth;
        public int spriteHeight;
    }

    [CustomPropertyDrawer(typeof(AlphaRectangleBorder))]
    public class AlphaRectangleBorderEditor : PropertyDrawer
    {
        private const string TopBorderName = "topBorder";
        private const string LeftBorderName = "leftBorder";
        private const string BottomBorderName = "bottomBorder";
        private const string RightBorderName = "rightBorder";
        private const string SpriteWidthName = "spriteWidth";
        private const string SpriteHeightName = "spriteHeight";

        private static Texture moveIcon;
        private static bool isIconInitialized;

        private const float LineSpacing = 1.5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!isIconInitialized)
            {
                moveIcon = EditorGUIUtility.IconContent("MoveTool@2x").image;
                isIconInitialized = true;
            }

            var intFieldLength = position.width / 4f;

            property.serializedObject.Update();

            EditorGUI.BeginProperty(position, label, property);

            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            EditorGUI.LabelField(
                new Rect(position.width / 3, position.y, 90, EditorGUIUtility.singleLineHeight), "Adjust Borders",
                centeredStyle);

            position.y += EditorGUIUtility.singleLineHeight + LineSpacing;

            // EditorGUI.PropertyField(
            //     new Rect(position.x + intFieldLength, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
            //     property.FindPropertyRelative(TopBorderName), GUIContent.none);

            var serializedTopBorderProperty = property.FindPropertyRelative(TopBorderName);
            serializedTopBorderProperty.intValue = EditorGUI.IntSlider(
                new Rect(position.x + intFieldLength, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                serializedTopBorderProperty.intValue, 0, property.FindPropertyRelative(SpriteHeightName).intValue);


            // serializedTopBorderProperty.intValue = EditorGUI.IntField(
            //     new Rect(position.x + intFieldLength, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
            //     "label", serializedTopBorderProperty.intValue);


            position.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;

            // EditorGUI.PropertyField(
            //     new Rect(position.x, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
            //     property.FindPropertyRelative(LeftBorderName), GUIContent.none);
            var serializedLeftBorderProperty = property.FindPropertyRelative(LeftBorderName);
            serializedLeftBorderProperty.intValue = EditorGUI.IntSlider(
                new Rect(position.x, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                serializedLeftBorderProperty.intValue, 0, property.FindPropertyRelative(SpriteWidthName).intValue);

            EditorGUI.LabelField(
                new Rect(position.width / 2 - (intFieldLength / 2) - (moveIcon.width / 2f),
                    position.y - (moveIcon.height / 3f), intFieldLength, EditorGUIUtility.singleLineHeight * 2),
                new GUIContent(moveIcon));

            // EditorGUI.PropertyField(
            //     new Rect(position.x + 2 * intFieldLength, position.y, intFieldLength,
            //         EditorGUIUtility.singleLineHeight),
            //     property.FindPropertyRelative(RightBorderName), GUIContent.none);

            var serializedRightBorderProperty = property.FindPropertyRelative(RightBorderName);
            serializedRightBorderProperty.intValue = EditorGUI.IntSlider(
                new Rect(position.x + 2 * intFieldLength, position.y, intFieldLength,
                    EditorGUIUtility.singleLineHeight),
                serializedRightBorderProperty.intValue, 0, property.FindPropertyRelative(SpriteWidthName).intValue);

            position.y += 1.5f * EditorGUIUtility.singleLineHeight + LineSpacing;

            // EditorGUI.PropertyField(
            //     new Rect(position.x + intFieldLength, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
            //     property.FindPropertyRelative(BottomBorderName), GUIContent.none);

            var serializedBottomBorderProperty = property.FindPropertyRelative(BottomBorderName);
            serializedBottomBorderProperty.intValue = EditorGUI.IntSlider(
                new Rect(position.x + intFieldLength, position.y, intFieldLength, EditorGUIUtility.singleLineHeight),
                serializedBottomBorderProperty.intValue, 0, property.FindPropertyRelative(SpriteWidthName).intValue);

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}