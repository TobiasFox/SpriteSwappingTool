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
            "UI",
            "Wizard",
            "SurveySteps",
            "SurveyPreviewParent.prefab"
        };

        private SurveyPreview preview;

        public IntroSurveyStep(string name) : base(name)
        {
            preview = new SurveyPreview(Path.Combine(PreviewPrefabPathAndName));
        }

        public override void Commit()
        {
            base.Commit();
            Finish(SurveyFinishState.Succeeded);
            preview.CleanUp();
        }

        public override void Rollback()
        {
            base.Rollback();
            preview.CleanUp();
        }

        public override void DrawContent()
        {
            GUILayout.Label("Thank you very much for taking the time to participate in this survey :) ",
                Styling.CenteredStyleBold);
            EditorGUILayout.Space(VerticalSpacing);

            GUILayout.Label(
                "The topic of this survey is a visual glitch in 2D games, where the order of Sprites to be rendered can swap (see preview). As part of my master thesis at the HAW Hamburg, I developed a Unity tool, which identifies such Sprites and helps to sort them.",
                Styling.LabelWrapStyle);

            preview?.DoPreview();
            EditorGUILayout.Space(VerticalSpacing);

            // GUILayout.Label(
            //     "With your participation, you help to evaluate the tool, especially the functionality to generate automatic sorting order suggestions.",
            //     Styling.LabelWrapStyle);
            //
            // EditorGUILayout.Space(VerticalSpacing);

            EditorGUILayout.LabelField("Duration", "10 - 15 min");
            EditorGUILayout.LabelField("Data",
                "Completely anonymous!\n" +
                "The data will only be used for the purpose of my master thesis and will be completely deleted after finishing the thesis. At the latest on 23.02.2021.",
                Styling.LabelWrapStyle);

            EditorGUILayout.LabelField("Developed by",
                GeneralData.DevelopedBy + " (Games-Master student at HAW Hamburg)");

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Contact", GUILayout.ExpandWidth(false), GUILayout.Width(147));
                EditorGUILayout.SelectableLabel(GeneralData.DeveloperMailAddress,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.Space(VerticalSpacing);
            GUILayout.Label("I really appreciate your input!");

            EditorGUILayout.Space(VerticalSpacing);
            GUILayout.Label("And now, have fun while using the tool and let's start :)", Styling.CenteredStyleBold);
        }

        public override void CleanUp()
        {
            preview?.CleanUp();
        }
    }
}