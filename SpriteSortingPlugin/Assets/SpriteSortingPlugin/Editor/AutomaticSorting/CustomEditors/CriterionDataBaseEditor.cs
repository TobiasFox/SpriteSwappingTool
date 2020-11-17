using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    public abstract class CriterionDataBaseEditor<T> : Editor where T : SortingCriterionData
    {
        private static Texture removeIcon;
        private static bool isIconInitialized;

        protected T sortingCriterionData;

        private Rect headerRect;

        public delegate void RemoveCallback(CriterionDataBaseEditor<T> criterionDataBaseEditor);
        public RemoveCallback removeCallback;

        public void Initialize(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
            InternalInitialize();

            if (!isIconInitialized)
            {
                removeIcon = EditorGUIUtility.IconContent("Toolbar Minus@2x").image;
                isIconInitialized = true;
            }
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

        public virtual string GetTitleName()
        {
            return "Criteria";
        }

        private new void DrawHeader()
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 22);
            DrawHeader(backgroundRect);
        }

        private void DrawHeader(Rect backgroundRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                headerRect = new Rect(backgroundRect) {height = 22};
            }

            var labelRect = headerRect;
            labelRect.xMin += 37f;

            var priorityRect = headerRect;
            priorityRect.xMax = priorityRect.width - 60;
            priorityRect.xMin = priorityRect.width - 220;
            priorityRect.yMax -= 2;
            priorityRect.yMin += 2;

            var removeButtonRect = headerRect;
            removeButtonRect.xMax = removeButtonRect.width + 7.5f;
            removeButtonRect.xMin = removeButtonRect.width - 33f;
            removeButtonRect.yMax -= 2;
            removeButtonRect.yMin += 2;

            var foldoutRect = new Rect(headerRect.x, headerRect.y + 2, 13, 13);
            var toggleRect = new Rect(headerRect.x + 16, headerRect.y + 2, 13, 13);

            // adjust rect, to draw it completely
            // headerRect.xMin = 0f;
            // headerRect.width += 4f;
            EditorGUI.DrawRect(headerRect, EditorBackgroundColors.HeaderBackgroundLight);


            using (new EditorGUI.DisabledScope(!sortingCriterionData.isActive))
            {
                EditorGUIUtility.labelWidth = 45;
                sortingCriterionData.priority =
                    EditorGUI.IntSlider(priorityRect, "Priority", sortingCriterionData.priority, 1, 5);
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.LabelField(labelRect, GetTitleName(), EditorStyles.boldLabel);
            }

            if (GUI.Button(removeButtonRect, removeIcon))
            {
                if (removeCallback != null)
                {
                    removeCallback(this);
                }
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