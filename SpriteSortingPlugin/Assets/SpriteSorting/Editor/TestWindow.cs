using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SpriteSorting
{
    public class TestWindow : EditorWindow
    {
        private ReorderableList reorderableList;

        private List<int> testList;

        [MenuItem("Window/Test")]
        public static void ShowWindow()
        {
            var window = GetWindow<TestWindow>();
            window.Init();
            window.Show();
        }

        private void Init()
        {
            testList = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                testList.Add(i);
            }

            reorderableList = new ReorderableList(testList, typeof(int), true, true, false, false);
            reorderableList.drawHeaderCallback = DrawHeaderCallback;
            reorderableList.drawElementCallback = DrawElementCallback;
            // reorderableList.onSelectCallback = OnSelectCallback;
        }

        private int layer;
        
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isfocused)
        {
             EditorGUI.LabelField(new Rect(rect.x, rect.y, 90, EditorGUIUtility.singleLineHeight),"Test");

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.BeginChangeCheck();
                            EditorGUI.Popup(new Rect(rect.x + 90 + 10, rect.y, 135, EditorGUIUtility.singleLineHeight), "Layer",
                    layer, new string[]{"default", "test"});

            if (EditorGUI.EndChangeCheck())
            {
                // element.tempSpriteRenderer.sortingLayerName = sortingLayerNames[element.sortingLayer];
                // // Debug.Log("changed layer to " + element.tempSpriteRenderer.sortingLayerName);
                // isPreviewUpdating = true;
            }

            //TODO: dynamic spacing depending on number of digits of sorting order
            EditorGUIUtility.labelWidth = 70;

            EditorGUI.BeginChangeCheck();
            testList[index] = EditorGUI.IntField(new Rect(rect.x, rect.y, 300, EditorGUIUtility.singleLineHeight),
                "item ", testList[index]);
            if (EditorGUI.EndChangeCheck())
            {
                var currentEvent = Event.current;
                Debug.Log(currentEvent.type+" "+currentEvent.rawType);
                if (currentEvent.type == EventType.MouseUp)
                {
                    Debug.Log("mouse up");
                }
            }

            if (GUI.Button(
                new Rect(rect.x + 90 + 10 + 135 + 10 + 120 + 10, rect.y, 55, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                // Selection.objects = new Object[] {element.originSpriteRenderer.gameObject};
                // SceneView.lastActiveSceneView.Frame(element.originSpriteRenderer.bounds);

                // var tempItem = result.overlappingItems[index];
                // result.overlappingItems.RemoveAt(index);
                // result.overlappingItems.Insert(0, tempItem);
                // reordableSpriteSortingList.list = result.overlappingItems;
            }
            
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Items");
        }

        private void OnGUI()
        {
            reorderableList.DoLayoutList();
        }

        private void OnDisable()
        {
            reorderableList.drawHeaderCallback = null;
            reorderableList.drawElementCallback = null;
            reorderableList = null;
        }
    }
}