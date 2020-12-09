using SpriteSortingPlugin.SpriteSorting.UI;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin
{
    public class AboutWindow : EditorWindow
    {
        [MenuItem(GeneralData.UnityMenuMainCategory + "/" + GeneralData.Name + "/About %g", false, 3)]
        public static void ShowWindow()
        {
            var window = GetWindow<AboutWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Sprite Swapping Tool");
            maxSize = new Vector2(400, 200);
            minSize = maxSize;

            var currentResolution = Screen.currentResolution;
            position = new Rect((currentResolution.width * .5f) - (position.width * .5f),
                (currentResolution.height * .5f) - (position.height * .5f), position.width, position.height);
        }

        private void OnGUI()
        {
            EditorGUI.indentLevel++;
            var boldLabelStyle = new GUIStyle(EditorStyles.largeLabel) {fontStyle = FontStyle.Bold};

            GUILayout.Label(GeneralData.Name + " Tool", boldLabelStyle);
            UIUtil.DrawHorizontalLine(true);

            EditorGUILayout.LabelField("Version number", GeneralData.GetFullVersionNumber());
            EditorGUILayout.LabelField("Developed By", GeneralData.DevelopedBy);
            EditorGUILayout.LabelField("Contact", GeneralData.DeveloperMailAddress);
            EditorGUILayout.LabelField("", "All rights reserved");
            EditorGUI.indentLevel--;


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Third-party libraries");
            UIUtil.DrawHorizontalLine();

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(GeneralData.ClipperLibName + " " + GeneralData.ClipperLibVersion,
                GeneralData.ClipperLibLicense);
            LinkButton(GeneralData.ClipperLibLink, GeneralData.ClipperLibLink);

            EditorGUI.indentLevel--;
        }

        private void LinkButton(string buttonName, string url)
        {
            var buttonContent = new GUIContent(buttonName);
            var linkStyle = Styling.linkStyle;
            var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
            rect.y += EditorGUIUtility.singleLineHeight;
            var isLinkClicked = GUI.Button(rect, buttonContent, linkStyle);

            rect.width = linkStyle.CalcSize(buttonContent).x;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            var isMouseHoveringLink = rect.Contains(Event.current.mousePosition);
            if (isMouseHoveringLink)
            {
                Handles.BeginGUI();
                Handles.color = linkStyle.normal.textColor;
                Handles.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
                Handles.color = Color.white;
                Handles.EndGUI();
            }

            if (isLinkClicked)
            {
                Application.OpenURL(url);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}