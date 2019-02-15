using System;

namespace MonoRayCasting
{
    public class Utils
    {
        /// <summary>
        /// Conversion in quadrant of an angle in degrees.
        /// </summary>
        /// <param name="angle">angle in degree</param>
        /// <returns>Point</returns>
        public static Point ToQuadrant(double angle)
        {
            Point direction = new Point();

            angle = NormalizeAngle(angle);

            if (angle >= 0 && angle <= 90)
            {
                direction.Y = 1;
                direction.X = 1;
            }
            else if (angle > 90 && angle <= 180)
            {
                direction.Y = 1;
                direction.X = -1;
            }
            else if (angle > 180 && angle <= 270)
            {
                direction.Y = -1;
                direction.X = -1;
            }
            else if (angle > 270 && angle <= 360)
            {
                direction.Y = -1;
                direction.X = 1;
            }

            return direction;
        }

        /// <summary>
        /// Returns true if the cell is outside the array.
        /// </summary>
        /// <param name="ceil">cell to check</param>
        /// <returns>boolean</returns>
        public static bool CheckBounds(int[,] grid, Point ceil)
        {
            return ceil.X < 0 || ceil.Y < 0 || ceil.X >= grid.GetLength(1) || ceil.Y >= grid.GetLength(0);
        }

        /// <summary>
        /// Conversion in radian of an angle in degrees.
        /// </summary>
        /// <param name="angle">angle in degree</param>
        /// <returns>double</returns>
        public static double ToRad(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Normalize an angle in degrees in a valid value.
        /// </summary>
        /// <param name="angle">angle in degrees</param>
        /// <returns>double</returns>
        public static double NormalizeAngle(double angle)
        {
            if (angle < 0)
            {
                angle += 360;
            }

            if (angle >= 360)
            {
                angle -= 360;
            }

            return angle;
        }
    }
}
