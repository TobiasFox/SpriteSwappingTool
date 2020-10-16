namespace SpriteSortingPlugin.SAT
{
    public readonly struct Projection
    {
        private readonly double min;
        private readonly double max;

        public Projection(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsOverlapping(Projection otherProjection)
        {
            return (min <= otherProjection.max && min >= otherProjection.min)
                   || (otherProjection.min <= max && otherProjection.min >= min);
        }
    }
}