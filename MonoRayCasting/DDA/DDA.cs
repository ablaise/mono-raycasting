using System;

namespace MonoRayCasting
{
    public class DDA
    {
        private DDAIteration iteration = new DDAIteration();
        private readonly int[,] grid;
        private double FOV;

        public bool fixFishEyeEffect { get; set; }

        /// <summary>
        /// DDA Algorithm.
        /// </summary>
        /// <param name="grid">grid used by the algorithm</param>
        /// <param name="FOV">Field Of View</param>
        public DDA(int[,] grid, double FOV)
        {
            this.grid = grid;
            this.FOV = FOV;
        }

        /// <summary>
        /// Executes the DDA algorithm for a given ray.
        /// </summary>
        /// <param name="pAngle"></param>
        /// <param name="position"></param>
        /// <param name="ray"></param>
        /// <param name="rays"></param>
        /// <returns>DDAIteration</returns>
        public DDAIteration Run(double pAngle, Point position, int ray, double rays)
        {
            Point ceil = new Point();
            Point point = new Point();
            Point direction = new Point();

            double constant = 0;

            double angle = (pAngle - FOV / 2) + FOV / rays * ray;

            direction = Utils.ToQuadrant(angle);

            // calculating the first ray
            if (direction.Y < 0)
            {
                point.Y = Math.Floor(position.Y / 64) * 64 - 1;
            }
            else
            {
                point.Y = Math.Floor(position.Y / 64) * 64 + 64;
            }

            point.X = (point.Y - position.Y) / Math.Tan(Utils.ToRad(angle)) + position.X;

            // horizontal DDA
            while (true)
            {
                ceil.X = Math.Floor(point.X / 64);
                ceil.Y = Math.Floor(point.Y / 64);

                if (Utils.CheckBounds(grid, ceil))
                {
                    if (direction.X > 0)
                    {
                        point.X = grid.GetLength(1) * 64;
                    }
                    else
                    {
                        point.X = 0;
                    }

                    point.Y = position.Y + Math.Tan(Utils.ToRad(angle)) * (point.X - position.X);
                    break;
                }

                if (grid[(int)ceil.Y, (int)ceil.X] > 0)
                {
                    break;
                }

                constant = Math.Abs(64 / Math.Tan(Utils.ToRad(angle)));

                point.X = point.X + constant * direction.X;
                point.Y = point.Y + direction.Y * 64;
            }

            iteration.horizontalHit.X = point.X;
            iteration.horizontalHit.Y = point.Y;

            // calculating the first ray
            if (direction.X > 0)
            {
                point.X = Math.Floor(position.X / 64) * 64 + 64;
            }
            else
            {
                point.X = Math.Floor(position.X / 64) * 64 - 1;
            }

            point.Y = position.Y + Math.Tan(Utils.ToRad(angle)) * (point.X - position.X);

            // vertical DDA
            while (true)
            {
                ceil.X = Math.Floor(point.X / 64);
                ceil.Y = Math.Floor(point.Y / 64);

                if (Utils.CheckBounds(grid, ceil))
                {
                    if (direction.Y < 0)
                    {
                        point.Y = 0;
                    }
                    else
                    {
                        point.Y = grid.GetLength(0) * 64;
                    }

                    point.X = (point.Y - position.Y) / Math.Tan(Utils.ToRad(angle)) + position.X;
                    break;
                }

                if (grid[(int)ceil.Y, (int)ceil.X] > 0)
                {
                    break;
                }

                constant = Math.Abs(Math.Tan(Utils.ToRad(angle)) * 64);

                point.X = point.X + 64 * direction.X;
                point.Y = point.Y + constant * direction.Y;
            }

            iteration.verticalHit.X = point.X;
            iteration.verticalHit.Y = point.Y;

            // which "wall" is the closest between horizontal and vertical
            double h = Math.Sqrt(Math.Pow(position.X - iteration.horizontalHit.X, 2) + Math.Pow(position.Y - iteration.horizontalHit.Y, 2));
            double v = Math.Sqrt(Math.Pow(position.X - iteration.verticalHit.X, 2) + Math.Pow(position.Y - iteration.verticalHit.Y, 2));
            iteration.distance = Math.Min(h, v);

            // if h is smaller than v it means that the ray hit a horizontal "wall", vertical if not
            if (h < v)
            {
                iteration.closest = DDAIteration.Closest.HORIZONTAL;
            }
            else
            {
                iteration.closest = DDAIteration.Closest.VERTICAL;
            }

            // fix Fisheye Effect if needed
            if (fixFishEyeEffect)
            {
                iteration.distance = iteration.distance * Math.Cos(Utils.ToRad(pAngle - angle));
            }

            return iteration;
        }
    }
}
