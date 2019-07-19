using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using MonoGame.Extended;
using IndependentResolutionRendering;
using pico8_interpreter.Pico8;
using System.IO;
using System;

namespace pico8_interpreter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D pico8Logo;
        RasterizerState rasterizerState;

        PicoInterpreter pico8;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 600;

            Resolution.Init(ref graphics);
            Resolution.SetVirtualResolution(128, 128);
            Resolution.SetResolution(600, 600, false);

            this.IsFixedTimeStep = true;//false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d); //60);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pico8Logo = Content.Load<Texture2D>("pico8");
            rasterizerState = new RasterizerState { MultiSampleAntiAlias = true };

            pico8 = new PicoInterpreter(spriteBatch);
            pico8.LoadGame("test4.lua", new MoonSharpInterpreter());
            pico8.SetBtnPressedCallback(((x) => Keyboard.GetState().IsKeyDown((Keys)x)));
            pico8.SetControllerKeys(0, (int)Keys.Left, (int)Keys.Right, (int)Keys.Up, (int)Keys.Down, (int)Keys.Z, (int)Keys.X);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            pico8.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Resolution.BeginDraw();

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, rasterizerState, null, Resolution.getTransformationMatrix());
            pico8.Draw();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
