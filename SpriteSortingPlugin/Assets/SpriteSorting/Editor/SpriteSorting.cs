using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SpriteSorting : EditorWindow
    {
        
        [SerializeField] private bool ignoreAlphaOfSprites;
        
        [MenuItem("Window/Sprite Sorting")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SpriteSorting));
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Sprite Sorting", EditorStyles.boldLabel);
            // groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
            
        }
    }
}