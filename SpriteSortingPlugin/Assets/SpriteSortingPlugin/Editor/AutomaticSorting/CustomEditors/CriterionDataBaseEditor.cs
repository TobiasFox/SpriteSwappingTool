using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    public abstract class CriterionDataBaseEditor<T> : Editor where T : SortingCriterionData
    {
        protected T sortingCriterionData;

        public void Initialize(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
            InternalInitialize();
        }

        protected virtual void InternalInitialize()
        {
        }

        public override void OnInspectorGUI()
        {
            DrawHeader();

            if (!sortingCriterionData.isExpanded)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(!sortingCriterionData.isActive))
            {
                EditorGUI.indentLevel++;
                OnInspectorGuiInternal();
                EditorGUI.indentLevel--;
            }
        }

        protected abstract void OnInspectorGuiInternal();

        protected virtual string GetTitleName()
        {
            return "Criteria";
        }

        private new void DrawHeader()
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 22);

            var labelRect = backgroundRect;
            labelRect.xMin += 37f;

            var priorityRect = backgroundRect;
            // priorityRect.xMax -= priorityRect.width / 2f - 100;
            // priorityRect.xMin = priorityRect.xMax - 200;
            priorityRect.xMax = priorityRect.width-10;
            priorityRect.xMin = priorityRect.width - 200;
            priorityRect.yMax -= 2;
            priorityRect.yMin += 2;

            var foldoutRect = new Rect(backgroundRect.x, backgroundRect.y + 2, 13, 13);
            var toggleRect = new Rect(backgroundRect.x + 16, backgroundRect.y + 2, 13, 13);

            // adjust rect, to draw it completely
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;
            EditorGUI.DrawRect(backgroundRect, EditorBackgroundColors.HeaderBackgroundLight);


            using (new EditorGUI.DisabledScope(!sortingCriterionData.isActive))
            {
                EditorGUIUtility.labelWidth = 45;
                sortingCriterionData.priority =
                    EditorGUI.IntSlider(priorityRect, "Priority", sortingCriterionData.priority, 1, 5);
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.LabelField(labelRect, GetTitleName(), EditorStyles.boldLabel);
            }

            sortingCriterionData.isExpanded = GUI.Toggle(foldoutRect, sortingCriterionData.isExpanded, GUIContent.none,
                EditorStyles.foldout);

            sortingCriterionData.isActive =
                GUI.Toggle(toggleRect, sortingCriterionData.isActive, GUIContent.none);

            var e = Event.current;
            if (e.type != EventType.MouseDown)
            {
                return;
            }

            if (labelRect.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    sortingCriterionData.isExpanded = !sortingCriterionData.isExpanded;
                }

                e.Use();
            }
        }
    }
}