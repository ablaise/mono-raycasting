using Microsoft.Xna.Framework.Input;
using System;

namespace MonoRayCasting
{
    public class Player
    {
        private KeyboardState keyboard = new KeyboardState();
        private readonly int[,] map;
        public Point position;
        public double angle;
        public int FOV = 60;
        public int speed = 4;

        public Point direction = new Point();

        public Player(int[,] map, Point position, int angle = 0, int speed = 4)
        {
            this.map = map;
            this.position = position;
            this.angle = angle;
            this.speed = speed;
        }

        public void Update()
        {
            Point quadrant = Utils.ToQuadrant(angle);
            direction.X = quadrant.X;
            direction.Y = quadrant.Y;

            KeyboadHandler();
        }

        public void KeyboadHandler()
        {
            keyboard = Keyboard.GetState();

            Point cell = new Point();
            cell.X = Math.Floor(position.X / 64);
            cell.Y = Math.Floor(position.Y / 64);

            if (keyboard.IsKeyDown(Keys.Up))
            {
                if (map[(int)cell.Y, (int)(cell.X + direction.X)] == 0)
                {
                    position.X += Math.Cos((float)Utils.ToRad(angle)) * speed;
                }

                if (map[(int)(cell.Y + direction.Y), (int)cell.X] == 0)
                {
                    position.Y += Math.Sin(Utils.ToRad(angle)) * speed;
                }
            }
            else if (keyboard.IsKeyDown(Keys.Down))
            {
                if (map[(int)cell.Y, (int)(cell.X - direction.X)] == 0)
                {
                    position.X -= Math.Cos(Utils.ToRad(angle)) * speed;
                }

                if (map[(int)(cell.Y - direction.Y), (int)cell.X] == 0)
                {
                    position.Y -= Math.Sin(Utils.ToRad(angle)) * speed;
                }
            }

            if (keyboard.IsKeyDown(Keys.Right))
            {
                angle += 2;
            }
            else if (keyboard.IsKeyDown(Keys.Left))
            {
                angle -= 2;
            }

            angle = Utils.NormalizeAngle(angle);
        }
    }
}