using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.CustomEditors
{
    public abstract class CriterionDataBaseEditor<T> : Editor where T : SortingCriterionData
    {
        protected T sortingCriterionData;
        protected string title = "Criteria";
        protected string tooltip = "";

        private Rect headerRect;
        private bool isShowingInInspector;

        public string Title => title;
        public string Tooltip => tooltip;

        public void Initialize(T sortingCriterionData, bool isShowingInInspector = false)
        {
            this.isShowingInInspector = isShowingInInspector;
            this.sortingCriterionData = sortingCriterionData;
            InternalInitialize();
        }

        protected virtual void InternalInitialize()
        {
        }

        public void UpdateSortingCriterionData(T sortingCriterionData)
        {
            this.sortingCriterionData = sortingCriterionData;
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
            priorityRect.yMax -= 2;
            priorityRect.yMin += 2;
            if (!isShowingInInspector)
            {
                priorityRect.xMax = priorityRect.width - 60;
                priorityRect.xMin = priorityRect.width - 220;
            }
            else
            {
                priorityRect.xMax = priorityRect.width + 15;
                priorityRect.xMin = priorityRect.width - 160;
            }

            var foldoutRect = new Rect(headerRect.x, headerRect.y + 2, 13, 13);
            var toggleRect = new Rect(headerRect.x + 16, headerRect.y + 2, 13, 13);

            // adjust rect, to draw it completely
            // headerRect.xMin = 0f;
            // headerRect.width += 4f;
            var headerColor = isShowingInInspector
                ? Styling.SortingCriteriaInspectorHeaderBackgroundColor
                : Styling.SortingCriteriaHeaderBackgroundColor;
            EditorGUI.DrawRect(headerRect, headerColor);


            using (new EditorGUI.DisabledScope(!sortingCriterionData.isActive))
            {
                if (sortingCriterionData is ContainmentSortingCriterionData)
                {
                    GUI.Label(priorityRect, "Preferred over all other criteria");
                }
                else
                {
                    EditorGUIUtility.labelWidth = 45;
                    sortingCriterionData.priority =
                        EditorGUI.IntSlider(priorityRect, "Priority", sortingCriterionData.priority, 1, 5);
                    EditorGUIUtility.labelWidth = 0;
                }

                EditorGUI.LabelField(labelRect, new GUIContent(title, tooltip), EditorStyles.boldLabel);
            }

            if (!isShowingInInspector)
            {
                var removeButtonRect = headerRect;
                removeButtonRect.xMax = removeButtonRect.width + 7.5f;
                removeButtonRect.xMin = removeButtonRect.width - 33f;
                removeButtonRect.yMax -= 2;
                removeButtonRect.yMin += 2;

                if (GUI.Button(removeButtonRect, Styling.RemoveIcon))
                {
                    sortingCriterionData.isAddedToEditorList = false;
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