#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

using UnityEditor.Presets;

namespace SpriteSortingPlugin.SpriteSorting.UI.AutoSorting
{
    public class SortingCriteriaPresetSelector : PresetSelectorReceiver
    {
        private AutoSortingOptionsUI currentWindow;
        private Preset initialValues;
        private SortingCriteriaPreset currentPreset;

        public void Init(AutoSortingOptionsUI autoSortingOptionsUI)
        {
            currentWindow = autoSortingOptionsUI;
        }

        public void ShowPresetSelector()
        {
            currentPreset = currentWindow.GenerateSortingCriteriaPreset();
            initialValues = new Preset(currentPreset);
            PresetSelector.ShowSelector(currentPreset, null, true, this);
        }

        public override void OnSelectionChanged(Preset selection)
        {
            if (selection != null)
            {
                selection.ApplyTo(currentPreset);
            }
            else
            {
                initialValues.ApplyTo(currentPreset);
            }

            var presetClone = (SortingCriteriaPreset) currentPreset.Clone();
            currentWindow.UpdateSortingCriteriaFromPreset(presetClone);
        }
    }
}