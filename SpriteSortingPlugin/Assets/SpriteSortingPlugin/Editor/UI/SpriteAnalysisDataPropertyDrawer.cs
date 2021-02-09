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

using System;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.UI
{
    [CustomPropertyDrawer(typeof(SpriteAnalysisData))]
    public class SpriteAnalysisDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            var sharpnessProperty = property.FindPropertyRelative("sharpness");
            sharpnessProperty.doubleValue = EditorGUILayout.DoubleField(new GUIContent(sharpnessProperty.displayName,
                UITooltipConstants.SpriteDataSharpnessTooltip), sharpnessProperty.doubleValue);
            if (EditorGUI.EndChangeCheck())
            {
                sharpnessProperty.doubleValue = Math.Max(0, sharpnessProperty.doubleValue);
            }

            var perceivedLightnessProperty = property.FindPropertyRelative("perceivedLightness");
            perceivedLightnessProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent(perceivedLightnessProperty.displayName,
                    UITooltipConstants.SpriteDataPerceivedLightnessTooltip),
                perceivedLightnessProperty.floatValue, 0, 100);

            EditorGUI.BeginChangeCheck();
            var averageAlphaProperty = property.FindPropertyRelative("averageAlpha");
            averageAlphaProperty.floatValue = EditorGUILayout.Slider(
                new GUIContent("Average alpha", UITooltipConstants.SpriteDataAverageAlphaTooltip),
                averageAlphaProperty.floatValue, 0, 1);


            var primaryColorProperty = property.FindPropertyRelative("primaryColor");
            EditorGUILayout.PropertyField(primaryColorProperty,
                new GUIContent(primaryColorProperty.displayName, UITooltipConstants.SpriteDataPrimaryColorTooltip));

            EditorGUI.indentLevel--;
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}