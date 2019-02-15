using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoRayCasting
{
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private KeyboardState keyboard;

        //
        private readonly bool DEBUG = true;

        // screen size 
        private const int Width = 1024;
        private const int Height = 768;

        // the width of the screen is equal number of rays to calculate
        private const int Rays = Width;

        // font and textures
        private SpriteFont font;
        private Texture2D pixel;
        private Texture2D brickWall;
        private Texture2D eagleWall;
        private Texture2D redBrick;
        private Texture2D texture;
        private Texture2D[] textures;

        private double plane;
        private Player player;
        private DDA dda;
        private DDAIteration iteration;

        // map data
        private int[,] map = new int[,]
        {
            {1,1,1,1,1,1,1,1},
            {1,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,1},
            {1,1,0,0,0,0,0,1},
            {1,0,0,0,1,0,0,1},
            {1,2,0,0,0,0,0,1},
            {1,3,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1}
        };

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = Height;
            graphics.PreferredBackBufferWidth = Width;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            // initialize the player with a position and a FOV
            player = new Player(map, new Point(64 * 5, 64 * 2), 133);

            // initialize the DDA algorithm
            dda = new DDA(map, player.FOV);
            dda.fixFishEyeEffect = true;

            // projection plane
            plane = (Width / 2) / Math.Tan(Utils.ToRad(player.FOV / 2));
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // loading font
            font = Content.Load<SpriteFont>("DebugFont");

            // loading textures
            brickWall = this.Content.Load<Texture2D>("brick");
            eagleWall = this.Content.Load<Texture2D>("eagle");
            redBrick  = this.Content.Load<Texture2D>("redbrick");

            // creating a pixel texture
            pixel = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });

            // textures array
            textures = new Texture2D[] { null, brickWall, eagleWall, redBrick };
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            KeyboardHandler();
            player.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();

            int side, y;
            double height;

            // intersection point calculated for each iteration of the DDA algorithm
            Point hit;

            for (int i = 0; i < Rays; i++)
            {
                iteration = dda.Run(player.angle, player.position, i, Rays);

                // set color
                if (iteration.closest == DDAIteration.Closest.HORIZONTAL)
                {
                    texture = textures[map[(int)Math.Floor(iteration.horizontalHit.Y / 64), (int)Math.Floor(iteration.horizontalHit.X / 64)]];
                    hit = iteration.horizontalHit;
                    side = 0;
                }
                else
                {
                    texture = textures[map[(int)Math.Floor(iteration.verticalHit.Y / 64), (int)Math.Floor(iteration.verticalHit.X / 64)]];
                    hit = iteration.verticalHit;
                    side = 1;
                }

                // computes the height (wall height / distance * plane)
                height = 64 / iteration.distance * plane;

                // intersection point between the wall and the ground
                y = Main.Height / 2 - (int)height / 2;

                // wall
                spriteBatch.Draw(
                    texture,
                    new Vector2(i, y),
                    new Rectangle((int)((side == 0) ? hit.X % 64 : hit.Y % 64), 0, 1, 64),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    (float)height / 64,
                    SpriteEffects.None,
                    1f
                );

                // floor
                spriteBatch.Draw(
                    pixel,
                    new Rectangle(i + 1, (int)(Main.Height / 2 + height / 2),
                    (int)(Main.Height / 2 + height / 2), 1),
                    null,
                    Color.BurlyWood,
                    (float)Utils.ToRad(90),
                    Vector2.Zero,
                    SpriteEffects.None,
                    0
                );

                // ceil
                spriteBatch.Draw(
                    pixel,
                    new Rectangle(i + 1, 0, y, 1),
                    null,
                    Color.SteelBlue,
                    (float)Utils.ToRad(90),
                    Vector2.Zero,
                    SpriteEffects.None,
                    0
                );
            }

            if (DEBUG)
            {
                double px = Math.Round(player.position.X);
                double py = Math.Round(player.position.Y);

                spriteBatch.DrawString(
                    font,
                    string.Format("Position : [{0} ({1}), {2} ({3})]", Math.Floor(px / 64), px, Math.Floor(py / 64), py),
                    new Vector2(10, 20),
                    Color.White
                );

                spriteBatch.DrawString(
                    font,
                    string.Format("FishEye Effect fixed : {0}", dda.fixFishEyeEffect),
                    new Vector2(10, 50),
                    Color.White
                );

                DrawMinimap(new Point(Width - 150, 20), 4);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the mini-map at a specified location.
        /// </summary>
        /// <param name="position">location of the mini-map</param>
        /// <param name="scale">scale of the mini-map, 2 divide the map by two</param>
        private void DrawMinimap(Point position, double scale)
        {
            // we save the previous value to restore it later
            bool fishEyeEffect = dda.fixFishEyeEffect;
            dda.fixFishEyeEffect = !dda.fixFishEyeEffect;

            int xx   = (int)(player.position.X / scale + position.X);
            int yy   = (int)(player.position.Y / scale + position.Y);
            int size = (int)(64 * (1 / scale));

            spriteBatch.Draw(pixel, new Rectangle((int)position.X, (int)position.Y, map.GetLength(1) * size, map.GetLength(0) * size), Color.White);

            // drawing grids
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    // draw a black or red grid depending on the kind of tile
                    DrawBorder(new Rectangle(y * size + (int)position.X, x * size + (int)position.Y, size, size), 1, (map[x, y] > 0) ? Color.Black : Color.Red);

                    // draw a black square if the player is not allowed to walk on the tile
                    if (map[x, y] > 0)
                    {
                        spriteBatch.Draw(pixel, new Rectangle(y * size + (int)position.X, x * size + (int)position.Y, size, size), Color.Black);
                    }
                }
            }

            // 60 rays are enough for debugging
            for (int i = 0; i < 60; i++)
            {
                iteration = dda.Run(player.angle, player.position, i, 60);
                spriteBatch.Draw(pixel, new Rectangle(xx, yy, (int)(iteration.distance / scale), 1), null, Color.Green, (float)Utils.ToRad((player.angle - 30) + 60 / 60 * i), Vector2.Zero, SpriteEffects.None, 0);
            }

            // player locatation
            spriteBatch.Draw(pixel, new Rectangle(xx - 5, yy - 5, 8, 8), Color.Purple);

            dda.fixFishEyeEffect = fishEyeEffect;
        }

        /// <summary>
        /// Draws the sides of a rectangle.
        /// </summary>
        /// <param name="rectangle">rectangle</param>
        /// <param name="border">sides size in pixels</param>
        /// <param name="color">sides color</param>
        private void DrawBorder(Rectangle rectangle, int border, Color color)
        {
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, border), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, border, rectangle.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - border, rectangle.Y, border, rectangle.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - border, rectangle.Width, border), color);
        }

        /// <summary>
        /// Keyboard handler, mainly for displaying debug information.
        /// </summary>
        private void KeyboardHandler()
        {
            keyboard = Keyboard.GetState();

            if (DEBUG)
            {
                if (keyboard.IsKeyDown(Keys.F1))
                {
                    dda.fixFishEyeEffect = true;
                }
                else if (keyboard.IsKeyDown(Keys.F2))
                {
                    dda.fixFishEyeEffect = false;
                }
            }
        }
    }
}
