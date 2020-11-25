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
        public PolygonCollider2D surfaceCollider;

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

        public void CalculateSurfaceArea()
        {
            var path = GenerateColliderPath(surfaceCollider);
            var surface = Math.Abs(Clipper.Area(path));
            surface /= ScaleFactor * ScaleFactor;
            Debug.Log(surface);
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

            if (GUILayout.Button("Surface"))
            {
                clipperTest.CalculateSurfaceArea();
            }
        }
    }
#endif
}