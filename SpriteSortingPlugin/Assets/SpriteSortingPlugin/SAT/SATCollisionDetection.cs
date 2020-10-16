using UnityEngine;

namespace SpriteSortingPlugin.SAT
{
    public static class SATCollisionDetection
    {
        public static bool IsOverlapping(ObjectOrientedBoundingBox oobb, ObjectOrientedBoundingBox otherOOBB)
        {
            if (oobb == null || otherOOBB == null)
            {
                return false;
            }

            // DrawOOBB(oobb, Color.blue);
            // DrawOOBB(otherOOBB, Color.green);

            return CheckAxisProjectionsOnOOBB(oobb, otherOOBB) && CheckAxisProjectionsOnOOBB(otherOOBB, oobb);
        }

        private static bool CheckAxisProjectionsOnOOBB(ObjectOrientedBoundingBox oobb,
            ObjectOrientedBoundingBox otherOOBB)
        {
            var oobbAxes = oobb.Axes;
            for (var i = 0; i < oobbAxes.Length; i++)
            {
                var axis = oobbAxes[i];

                // OOBB specific
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
            Debug.DrawLine(Vector2.zero, axis,isFirst? Color.yellow:Color.black);
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