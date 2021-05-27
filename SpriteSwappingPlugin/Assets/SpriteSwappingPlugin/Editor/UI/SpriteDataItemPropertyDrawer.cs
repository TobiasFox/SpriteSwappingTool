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

using UnityEditor;
using UnityEngine;

namespace SpriteSwappingPlugin.UI
{
    [CustomPropertyDrawer(typeof(SpriteDataItem))]
    public class SpriteDataItemPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUI.IndentLevelScope())
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded,
                    property.FindPropertyRelative("assetName").stringValue, true);

                if (!property.isExpanded)
                {
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    property.serializedObject.Update();
                    var oobbProperty = property.FindPropertyRelative("objectOrientedBoundingBox");
                    EditorGUILayout.PropertyField(oobbProperty, true);
                    property.serializedObject.ApplyModifiedProperties();

                    property.serializedObject.Update();
                    var outlinePointsProperty = property.FindPropertyRelative("outlinePoints");
                    EditorGUILayout.PropertyField(outlinePointsProperty, true);
                    property.serializedObject.ApplyModifiedProperties();

                    property.serializedObject.Update();
                    var spriteAnalysisDataProperty = property.FindPropertyRelative("spriteAnalysisData");
                    EditorGUILayout.PropertyField(spriteAnalysisDataProperty, true);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }
    }
}