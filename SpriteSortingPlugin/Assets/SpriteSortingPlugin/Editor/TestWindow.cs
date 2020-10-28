using UnityEditor;
using UnityEngine;

public class TestWindow : EditorWindow
{
    private int counter;

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        var counterPosition = new Rect(0, 0, 300, EditorGUIUtility.singleLineHeight);
        counter = EditorGUI.IntField(counterPosition, "Counter", counter);

        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("counter has changed");
        }
    }

    [MenuItem("Window/TestWindow %t")]
    public static void ShowWindow()
    {
        var window = GetWindow<TestWindow>();
        window.Show();
    }
}