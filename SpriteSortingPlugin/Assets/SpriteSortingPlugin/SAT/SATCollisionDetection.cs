namespace SpriteSortingPlugin.SAT
{
    public static class SATCollisionDetection
    {
        public static bool IsOverlapping(ObjectOrientedBoundingBox oobb, ObjectOrientedBoundingBox otherOOBB)
        {
            return CheckAxisProjectionsOnOOBB(oobb, otherOOBB) && CheckAxisProjectionsOnOOBB(otherOOBB, oobb);
        }

        private static bool CheckAxisProjectionsOnOOBB(ObjectOrientedBoundingBox oobb,
            ObjectOrientedBoundingBox otherOOBB)
        {
            foreach (var axis in oobb.Axes)
            {
                var projection = oobb.ProjectAxis(axis);
                var otherProjection = otherOOBB.ProjectAxis(axis);

                if (!projection.IsOverlapping(otherProjection))
                {
                    return false;
                }
            }

            return true;
        }
    }
}