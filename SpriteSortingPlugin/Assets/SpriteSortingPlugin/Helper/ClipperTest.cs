﻿#region license

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
using System.Collections.Generic;
using ClipperLib;
using UnityEditor;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace SpriteSortingPlugin.Helper
{
    public class ClipperTest : MonoBehaviour
    {
        private const float ScaleFactor = 100;
        public PolygonCollider2D[] polygons;
        public PolygonCollider2D outputCollider;
        public SpriteRenderer[] spriteRenderers;
        public SpriteData spriteData;
        public PolygonCollider2D areaCollider;

        public void ColliderIntersection()
        {
            polygons = new PolygonCollider2D[2];

            for (var i = 0; i < spriteRenderers.Length; i++)
            {
                var spriteRenderer = spriteRenderers[i];
                var assetGuid1 =
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteRenderer.sprite.GetInstanceID()));

                var polyColliderGameObject1 = new GameObject("ToCheck- PolygonCollider " + spriteRenderer.name);
                var currentTransform = spriteRenderer.transform;
                polyColliderGameObject1.transform.SetPositionAndRotation(
                    currentTransform.position, currentTransform.rotation);
                polyColliderGameObject1.transform.localScale = currentTransform.lossyScale;

                var polygonColliderToCheck = polyColliderGameObject1.AddComponent<PolygonCollider2D>();
                polygonColliderToCheck.points = spriteData.spriteDataDictionary[assetGuid1].outlinePoints;

                polygons[i] = polygonColliderToCheck;
            }

            Intersect();

            foreach (var polygon in polygons)
            {
                DestroyImmediate(polygon.gameObject);
            }
        }

        public void Intersect()
        {
            var clipper = new Clipper();

            for (int i = 0; i < polygons.Length; i++)
            {
                var path = GenerateColliderPath(polygons[i]);
                clipper.AddPath(path, i == 0 ? PolyType.ptClip : PolyType.ptSubject, true);
            }

            var clipTypes = new ClipType[] {ClipType.ctIntersection, ClipType.ctUnion};
            var colliderParent = new GameObject("colliderParent").transform;
            var allPolyFillTypes = Enum.GetValues(typeof(PolyFillType));

            foreach (var clipType in clipTypes)
            {
                var currentTransformParent = new GameObject(clipType.ToString()).transform;
                currentTransformParent.parent = colliderParent;

                foreach (PolyFillType clipPolyFillType in allPolyFillTypes)
                {
                    // if (clipType == ClipType.ctUnion && clipPolyFillType == PolyFillType.pftPositive)
                    // {
                    //     continue;
                    // }

                    foreach (PolyFillType subjectPolyfillType in allPolyFillTypes)
                    {
                        // if (clipType == ClipType.ctUnion && subjectPolyfillType == PolyFillType.pftPositive)
                        // {
                        //     continue;
                        // }


                        var solution = new Paths();
                        clipper.Execute(clipType, solution, subjectPolyfillType, clipPolyFillType);
                        if (solution.Count <= 0)
                        {
                            continue;
                        }

                        var colliderGO =
                            new GameObject("clip: " + clipPolyFillType + ", subj:" + subjectPolyfillType);
                        colliderGO.transform.parent = currentTransformParent;
                        SetOutputOnCollider(solution, colliderGO.transform);

                        // var tree = new PolyTree();
                        // clipper.Execute(clipType, tree, subjectPolyfillType, clipPolyFillType);
                        //
                        // if (tree.ChildCount <= 0)
                        // {
                        //     continue;
                        // }
                        // CreateTreeCollider(tree, colliderGO.transform);
                    }
                }
            }
        }

        private void CreateTreeCollider(PolyTree tree, Transform colliderGO)
        {
            for (var i = 0; i < tree.Childs.Count; i++)
            {
                var child = tree.Childs[i];
                var childGO =
                    new GameObject("child" + i);
                childGO.transform.parent = colliderGO;
                var polygonCollider = childGO.AddComponent<PolygonCollider2D>();

                SetOutputOnCollider(child.Contour, polygonCollider);

                if (child.ChildCount > 0)
                {
                    Debug.Log("has children");
                }
            }
        }

        private void SetOutputOnCollider(Paths solution, Transform parent)
        {
            for (var i = 0; i < solution.Count; i++)
            {
                var path = solution[i];
                var childGO = new GameObject("child" + i);
                childGO.transform.parent = parent;
                var polygonCollider = childGO.AddComponent<PolygonCollider2D>();

                SetOutputOnCollider(path, polygonCollider);
            }
        }

        private void SetOutputOnCollider(List<IntPoint> path, PolygonCollider2D output)
        {
            var pointList = new Vector2[path.Count];
            for (var i = 0; i < path.Count; i++)
            {
                var point = path[i];
                pointList[i] = (new Vector2(point.X / ScaleFactor, point.Y / ScaleFactor));
            }

            output.points = pointList;
        }

        private List<IntPoint> GenerateColliderPath(PolygonCollider2D polygon)
        {
            var path = new List<IntPoint>(polygon.points.Length);

            foreach (var polygonPoint in polygon.points)
            {
                // var point = new DoublePoint(polygonPoint.x * 100, polygonPoint.y * 100);
                var transformedPoint = polygon.transform.TransformPoint(polygonPoint);
                var point = new IntPoint(Math.Round(transformedPoint.x * ScaleFactor),
                    Math.Round(transformedPoint.y * ScaleFactor));
                path.Add(point);
            }

            return path;
        }

        public void CalculateArea()
        {
            var path = GenerateColliderPath(areaCollider);
            var area = Math.Abs(Clipper.Area(path));
            area /= ScaleFactor * ScaleFactor;
            Debug.Log(area);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ClipperTest))]
    public class ClipperTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var clipperTest = (ClipperTest) target;
            if (GUILayout.Button("Intersection"))
            {
                clipperTest.Intersect();
            }

            if (GUILayout.Button("SpriteRenderer Intersection"))
            {
                clipperTest.ColliderIntersection();
            }

            if (GUILayout.Button("Area"))
            {
                clipperTest.CalculateArea();
            }
        }
    }
#endif
}