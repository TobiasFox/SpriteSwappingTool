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

namespace SpriteSortingPlugin.Survey.UI.Wizard
{
    public class CreatingSpriteDataStep
    {
        // private int questionCounter = 4;
        //
        // public CreatingSpriteDataStep(string name) : base(name)
        // {
        // }
        //
        // public override void DrawContent()
        // {
        //     EditorGUI.indentLevel++;
        //     EditorGUILayout.LabelField(
        //         "In the last step, the Bounds given by Unity were used to identify overlapping SpriteRenderers. However, it could happen, that the " +
        //         GeneralData.Name +
        //         " tool identifies SpriteRenderers, even though only their transparent regions overlap.",
        //         Styling.LabelWrapStyle);
        //     EditorGUILayout.Space(20);
        //
        //     EditorGUILayout.LabelField(
        //         "In that case you can use a more accurate Sprite outline type such as an object oriented bounding box or a pixel-perfect Sprite outline.\n" +
        //         "Both of them can be generated with the " + GeneralData.DataAnalysisName + " window."
        //         ,
        //         Styling.LabelWrapStyle);
        //     EditorGUILayout.Space(20);
        //
        //     EditorGUILayout.LabelField("You can find this window " + GeneralData.UnityMenuMainCategory + " -> " +
        //                                GeneralData.Name + " -> " + GeneralData.DataAnalysisName +
        //                                ".", Styling.LabelWrapStyle);
        //
        //     EditorGUILayout.Space();
        //
        //     using (new GUILayout.VerticalScope(Styling.HelpBoxStyle))
        //     {
        //         var taskLabelStyle = new GUIStyle(Styling.QuestionLabelStyle) {fontStyle = FontStyle.Bold};
        //         EditorGUILayout.LabelField(questionCounter +
        //                                    ". Open the " + GeneralData.DataAnalysisName + " window and create a " +
        //                                    nameof(SpriteData) + " asset.\n" +
        //                                    "This asset will be used in the next task.",
        //             taskLabelStyle);
        //     }
        //
        //     EditorGUI.indentLevel--;
        // }
    }
}