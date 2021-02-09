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
using SpriteSortingPlugin.OOBB;
using SpriteSortingPlugin.SpriteAnalyzer;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Helper
{
    public class SATTester : MonoBehaviour
    {
        private const float Tolerance = 0.0001f;

        // public Bounds bounds1;
        // public float rotation1;
        //
        // public Bounds bounds2;
        // public float rotation2;

        public SpriteRenderer[] spriteRenderers;
        public SpriteData spriteData;
        private ObjectOrientedBoundingBox[] oobbs;
        public Vector2 point1;
        public Vector2 point2;
        public Vector2 point3;
        public Vector2 point4;
        public Color backgroundColor;
        public Color foregroundColor;
        public Color primaryColor;

        public void Test()
        {
            // var oobb = new ObjectOrientedBoundingBox(bounds1, rotation1);
            // var otherOOBB = new ObjectOrientedBoundingBox(bounds2, rotation2);
            // var oobb = new ObjectOrientedBoundingBox(new Bounds(Vector3.zero, new Vector2(10, 10)),
            //     Quaternion.identity);
            // var otherOOBB = new ObjectOrientedBoundingBox(new Bounds(new Vector2(0, 5), new Vector2(10, 10)),
            //     Quaternion.identity);

            oobbs = new ObjectOrientedBoundingBox[spriteRenderers.Length];
            for (var i = 0; i < spriteRenderers.Length; i++)
            {
                var spriteRenderer = spriteRenderers[i];
                var assetGuid1 =
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

                var oobb = spriteData.spriteDataDictionary[assetGuid1].objectOrientedBoundingBox;
                if (i == 1 && oobb == oobbs[0])
                {
                    oobb = new ObjectOrientedBoundingBox(oobb);
                }

                oobb.UpdateBox(spriteRenderer.transform);

                oobbs[i] = oobb;
            }

            // var isOverlapping = SATCollisionDetection.IsOverlapping(oobbs[0], oobbs[1]);
            // Debug.Log(isOverlapping);

            Debug.Log(spriteRenderers[1].name + " in " + spriteRenderers[0] + oobbs[0].Contains(oobbs[1]));
            Debug.Log(spriteRenderers[0].name + " in " + spriteRenderers[1] + oobbs[1].Contains(oobbs[0]));

            Debug.Log("intersection: "+SATCollisionDetection.IsOverlapping(oobbs[0], oobbs[1]));
        }

        public void Test2()
        {
            var inDirection = point2 - point1;
            var perp = Vector2.Perpendicular(inDirection);

            Debug.DrawLine(point1, new Vector3(point1.x, point1.y, 2));
            Debug.DrawLine(point2, new Vector3(point2.x, point2.y, 2));
            Debug.DrawLine(point1, point2, Color.green);

            var start = (point1 + point2) / 2;

            Debug.DrawLine(start, new Vector3(start.x, start.y, 2));
            Debug.DrawLine(start, perp + start, Color.magenta);
        }

        public void ContainingPointTest()
        {
            var spriteRenderer = spriteRenderers[0];
            var assetGuid1 =
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

            var oobb = spriteData.spriteDataDictionary[assetGuid1].objectOrientedBoundingBox;
            oobb.UpdateBox(spriteRenderer.transform);
            Debug.Log(oobb.Contains(point1));
        }

        public void AnalyzeSharpness()
        {
            var sharpnessAnalyzer = new SharpnessAnalyzer();

            var sharpness = sharpnessAnalyzer.Analyze(spriteRenderers[0].sprite);
            Debug.Log(sharpness);
        }

        public void AnalyzeLightness()
        {
            var lightnessAnalyzer = new LightnessAnalyzer();

            var lightness = lightnessAnalyzer.Analyze(spriteRenderers[0]);
            Debug.Log(lightness);
        }

        public void AnalyzeBrightness2()
        {
            var lightnessAnalyzer = new LightnessAnalyzer();

            var lightness = lightnessAnalyzer.Analyze2(spriteRenderers[0]);
            var lightness2 = lightnessAnalyzer.Analyze(spriteRenderers[0]);
            Debug.Log("rgb " + lightness + " cielab" + lightness2);
        }

        public void ColorTester()
        {
            var color = spriteRenderers[0].color;
            var color2 = spriteRenderers[1].color;

            for (int i = 0; i < 3; i++)
            {
                var isInForeground = IsInForeground(color, color2, i);
            }
        }

        private bool IsInForeground(Color primaryColor, Color otherPrimaryColor, int channel)
        {
            var from = backgroundColor[channel];
            var to = foregroundColor[channel];
            var primaryChannel = primaryColor[channel];
            var otherPrimaryChannel = otherPrimaryColor[channel];

            var tPrimary = Mathf.InverseLerp(from, to, primaryChannel);
            var tOtherPrimary = Mathf.InverseLerp(from, to, otherPrimaryChannel);

            var isInForeground = tPrimary >= tOtherPrimary;
            Debug.LogFormat("channel {0} tPrimary {1} tOtherPrimary {2} isInForeground {3}", channel, tPrimary,
                tOtherPrimary, isInForeground);
            return isInForeground;
        }

        public void LineTest()
        {
            Debug.Log(IsOnLine());
        }

        private bool IsOnLine()
        {
            var origin = point1;
            var dir = point2 - point1;

            if (dir.x == 0)
            {
                return Math.Abs(point3.x - origin.x) < Tolerance;
            }

            if (dir.y == 0)
            {
                return Math.Abs(point3.y - origin.y) < Tolerance;
            }

            var t1 = (point3.x - origin.x) / dir.x;
            var t2 = (point3.y - origin.y) / dir.y;

            var isInLine = Math.Abs(t1 - t2) < Tolerance;

            return isInLine;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(point1, 0.1f);
            Gizmos.DrawSphere(point2, 0.1f);
            Gizmos.DrawSphere(point3, 0.1f);
            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
        }
    }

    [CustomEditor(typeof(SATTester))]
    public class SATTesterDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var satTester = (SATTester) target;
            if (GUILayout.Button("Test"))
            {
                satTester.Test();
            }

            if (GUILayout.Button("Test2"))
            {
                satTester.Test2();
            }

            if (GUILayout.Button(nameof(SATTester.ContainingPointTest)))
            {
                satTester.ContainingPointTest();
            }

            if (GUILayout.Button(nameof(SATTester.AnalyzeSharpness)))
            {
                satTester.AnalyzeSharpness();
            }

            if (GUILayout.Button(nameof(SATTester.AnalyzeLightness)))
            {
                satTester.AnalyzeLightness();
            }

            if (GUILayout.Button(nameof(SATTester.AnalyzeBrightness2)))
            {
                satTester.AnalyzeBrightness2();
            }

            if (GUILayout.Button(nameof(SATTester.ColorTester)))
            {
                satTester.ColorTester();
            }

            if (GUILayout.Button(nameof(SATTester.LineTest)))
            {
                satTester.LineTest();
            }
        }
    }
}