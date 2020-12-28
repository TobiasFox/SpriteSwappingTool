using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    [InitializeOnLoad]
    public class DelayedOpenSurveyScene : MonoBehaviour
    {
        private static readonly string[] SceneFolderPath = new string[]
        {
            "Assets", "_Scenes"
        };

        private const string IsSurveySceneShowedSessionStateName = "DelayedOpenSurveyScene.showedSurveyScene";

        private const string SurveyScene = "Survey.unity";

        static DelayedOpenSurveyScene()
        {
            EditorApplication.delayCall += SelectSurveySceneAutomatically;
        }

        private static void SelectSurveySceneAutomatically()
        {
            if (SessionState.GetBool(IsSurveySceneShowedSessionStateName, false))
            {
                return;
            }

            SessionState.SetBool(IsSurveySceneShowedSessionStateName, true);
            LoadSurveyScene();
        }

        private static void LoadSurveyScene()
        {
            var scenePath = Path.Combine(Path.Combine(SceneFolderPath), SurveyScene);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);


            var selectionGO = GameObject.Find("Intro");
            if (selectionGO == null)
            {
                return;
            }

            Selection.activeObject = selectionGO;
            EditorGUIUtility.PingObject(selectionGO);
            SceneView.FrameLastActiveSceneView();
        }
    }
}