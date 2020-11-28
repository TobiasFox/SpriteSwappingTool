using System;
using SpriteSortingPlugin.AutomaticSorting.CustomEditors;
using SpriteSortingPlugin.AutomaticSorting.Data;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.AutomaticSorting.SortingPreset
{
    [Serializable]
    public class SortingCriteriaPreset : ScriptableObject, ISerializationCallbackReceiver
    {
        public string[] jsonStrings;
        public string[] types;
        public SortingCriterionData[] SortingCriterionData { get; set; }

        public SortingCriteriaPreset Copy()
        {
            var clone = CreateInstance<SortingCriteriaPreset>();
            if (SortingCriterionData.Length <= 0)
            {
                return clone;
            }

            clone.SortingCriterionData = new SortingCriterionData[SortingCriterionData.Length];
            for (var i = 0; i < SortingCriterionData.Length; i++)
            {
                clone.SortingCriterionData[i] = SortingCriterionData[i].Copy();
            }

            return clone;
        }

        public void OnBeforeSerialize()
        {
            if (SortingCriterionData == null)
            {
                return;
            }

            jsonStrings = new string[SortingCriterionData.Length];
            types = new string[SortingCriterionData.Length];
            for (var i = 0; i < SortingCriterionData.Length; i++)
            {
                var criterionData = SortingCriterionData[i];
                jsonStrings[i] = JsonUtility.ToJson(criterionData);
                types[i] = criterionData.GetType().FullName;
            }
        }

        public void OnAfterDeserialize()
        {
            SortingCriterionData = new SortingCriterionData[jsonStrings.Length];
            for (var i = 0; i < jsonStrings.Length; i++)
            {
                var data = CreateInstance(types[i]);
                JsonUtility.FromJsonOverwrite(jsonStrings[i], data);
                SortingCriterionData[i] = (SortingCriterionData) data;
            }
        }
    }

    [CustomEditor(typeof(SortingCriteriaPreset))]
    public class SortingCriteriaPresetEditor : Editor
    {
        private Editor[] sortingCriteriaEditors;

        private void Awake()
        {
            var preset = (SortingCriteriaPreset) target;

            sortingCriteriaEditors = new Editor[preset.SortingCriterionData.Length];
            for (var i = 0; i < preset.SortingCriterionData.Length; i++)
            {
                var sortingCriterionData = preset.SortingCriterionData[i];
                var specificEditor = CreateEditor(sortingCriterionData);
                var criterionDataBaseEditor = (CriterionDataBaseEditor<SortingCriterionData>) specificEditor;
                criterionDataBaseEditor.Initialize(sortingCriterionData, true);
                sortingCriteriaEditors[i] = criterionDataBaseEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            foreach (var criteriaEditor in sortingCriteriaEditors)
            {
                criteriaEditor.OnInspectorGUI();
            }
        }

        private void OnDestroy()
        {
            foreach (var sortingCriteriaEditor in sortingCriteriaEditors)
            {
                DestroyImmediate(sortingCriteriaEditor);
            }
        }
    }
}