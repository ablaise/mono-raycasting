namespace MonoRayCasting
{
    public class DDAIteration
    {
        public enum Closest { HORIZONTAL, VERTICAL };
        public Point horizontalHit { get; set; }
        public Point verticalHit { get; set; }
        public double distance { get; set; }
        public Closest closest { get; set; }

        /// <summary>
        /// Storage for each iteration of the DDA Algorithm.
        /// </summary>
        public DDAIteration()
        {
            horizontalHit = new Point();
            verticalHit = new Point();
            distance = 0;
            closest = 0;
        }
    }
}
