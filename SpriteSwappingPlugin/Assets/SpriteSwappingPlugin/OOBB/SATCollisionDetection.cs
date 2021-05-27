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

using UnityEngine;

namespace SpriteSwappingPlugin.OOBB
{
    public static class SATCollisionDetection
    {
        public static bool IsColliding(ObjectOrientedBoundingBox oobb, ObjectOrientedBoundingBox otherOOBB)
        {
            if (oobb == null || otherOOBB == null)
            {
                return false;
            }

            return IsIntersecting(oobb, otherOOBB) && IsIntersecting(otherOOBB, oobb);
        }

        private static bool IsIntersecting(ObjectOrientedBoundingBox oobb,
            ObjectOrientedBoundingBox otherOOBB)
        {
            var oobbAxes = oobb.Axes;
            foreach (var axis in oobbAxes)
            {
                // if (i == 0)
                // {
                //     DrawAxisAndPerp(oobb.Points[3], oobb.Points[0], axis);
                // }
                // else
                // {
                //     DrawAxisAndPerp(oobb.Points[1], oobb.Points[0], axis);
                // }


                var projection = oobb.ProjectAxis(axis);
                var otherProjection = otherOOBB.ProjectAxis(axis);

                if (!projection.IsOverlapping(otherProjection))
                {
                    return false;
                }
            }

            return true;
        }

        private static void DrawAxisAndPerp(Vector2 point1, Vector2 point2, Vector2 axis, bool isFirst)
        {
            Debug.DrawLine(point1, new Vector3(point1.x, point1.y, 2));
            Debug.DrawLine(point2, new Vector3(point2.x, point2.y, 2));
            Debug.DrawLine(point1, point2, Color.green);

            var center = (point1 + point2) / 2;
            var endPerp = axis + center;

            Debug.DrawLine(center, new Vector3(center.x, center.y, 2));
            Debug.DrawLine(endPerp, new Vector3(endPerp.x, endPerp.y, 2));
            Debug.DrawLine(center, endPerp, Color.magenta);

            Debug.DrawLine(center, new Vector3(center.x, center.y, 2));
            Debug.DrawLine(endPerp, new Vector3(endPerp.x, endPerp.y, 2));
            Debug.DrawLine(Vector2.zero, axis, isFirst ? Color.yellow : Color.black);
        }

        private static void DrawOOBB(ObjectOrientedBoundingBox oobb, Color color)
        {
            var oobbPoints = oobb.Points;
            Debug.DrawLine(oobbPoints[0], oobbPoints[1], color, 2);
            Debug.DrawLine(oobbPoints[1], oobbPoints[2], color, 2);
            Debug.DrawLine(oobbPoints[2], oobbPoints[3], color, 2);
            Debug.DrawLine(oobbPoints[3], oobbPoints[0], color, 2);
        }
    }
}