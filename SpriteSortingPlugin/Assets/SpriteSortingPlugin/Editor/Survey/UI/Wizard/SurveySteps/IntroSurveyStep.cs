using System.IO;
using SpriteSortingPlugin.UI;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class IntroSurveyStep : SurveyStep
    {
        private const float VerticalSpacing = 5;

        private static readonly string[] PreviewPrefabPathAndName = new string[]
        {
            "Assets",
            "SpriteSortingPlugin",
            "Editor",
            "Survey",
            "Prefabs",
            "SurveyPreviewParent.prefab"
        };

        private SurveyPreview preview;

        public IntroSurveyStep(string name) : base(name)
        {
            preview = new SurveyPreview(Path.Combine(PreviewPrefabPathAndName), false);
        }

        public override void Commit()
        {
            base.Commit();
            preview.CleanUp();
        }

        public override void DrawContent()
        {
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                GUILayout.Label("Thank you very much for taking the time to participate in this survey.",
                    Styling.CenteredStyleBold);
                EditorGUILayout.Space(VerticalSpacing);

                GUILayout.Label(
                    "This survey is about visual glitches in 2D games. As part of my master thesis, I developed an Unity tool, which identifies such glitches and helps to solve them.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.Space();

                var wrappedCenterStyle = new GUIStyle(Styling.CenteredStyle) {wordWrap = true};
                GUILayout.Label("I really appreciate your input!", wrappedCenterStyle);
                preview?.DoPreview();
            }


            EditorGUILayout.Space(VerticalSpacing);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                EditorGUILayout.LabelField("Duration", "10 - 15 min");
                var smallerLabelWrapStyle = new GUIStyle(Styling.LabelWrapStyle);
                smallerLabelWrapStyle.fontSize--;
                EditorGUILayout.LabelField(new GUIContent("Data"),
                    new GUIContent("Completely anonymous and only for the purpose of the master thesis",
                        "The data will only be used for the purpose of my master thesis and will be completely deleted after finishing the thesis. At the latest on 23.02.2021."),
                    Styling.LabelWrapStyle);

                EditorGUILayout.LabelField("Developed by",
                    GeneralData.DevelopedBy + " (Games-Master student at HAW Hamburg)");

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Contact", GUILayout.ExpandWidth(false), GUILayout.Width(147));
                    EditorGUILayout.SelectableLabel(GeneralData.DeveloperMailAddress,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
                }
            }

            EditorGUILayout.Space(VerticalSpacing);
            EditorGUILayout.Space(VerticalSpacing);
            using (new EditorGUILayout.VerticalScope(Styling.HelpBoxStyle))
            {
                EditorGUILayout.LabelField(
                    "This editor window guides you through the survey consisting of four short parts and sends the data back to me.",
                    Styling.LabelWrapStyle);
                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField(new GUIContent(
                    "Please let this window the whole time opened and make sure this PC is connected to the internet to send the generated data.",
                    Styling.InfoIcon), Styling.LabelWrapStyle);
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight / 2f);
                EditorGUILayout.LabelField(new GUIContent(
                    "Also, please do not recompile any code while the survey window is open. If you do so, it will result in errors due to Unity`s serialization behaviour.",
                    Styling.InfoIcon), Styling.LabelWrapStyle);
            }
        }

        public override void CleanUp()
        {
            preview?.CleanUp();
        }

        public override int GetProgress(out int totalProgress)
        {
            totalProgress = 1;
            return totalProgress;
        }
    }
}