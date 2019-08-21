namespace pico8_interpreter
{
    using IndependentResolutionRendering;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using pico8_interpreter.Pico8;
    using System;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        /// <summary>
        /// Defines the graphics
        /// </summary>
        internal GraphicsDeviceManager graphics;

        /// <summary>
        /// Defines the spriteBatch
        /// </summary>
        internal SpriteBatch spriteBatch;

        /// <summary>
        /// Defines the pico8Logo
        /// </summary>
        private Texture2D pico8Logo;

        /// <summary>
        /// Defines the rasterizerState
        /// </summary>
        internal RasterizerState rasterizerState;

        /// <summary>
        /// Defines the pico8
        /// </summary>
        internal PicoInterpreter<Color> pico8;

        /// <summary>
        /// Defines the screenColorData
        /// </summary>
        internal Color[] screenColorData;

        /// <summary>
        /// Defines the screenTexture
        /// </summary>
        internal Texture2D screenTexture;

        /// <summary>
        /// Initializes a new instance of the <see cref="Game1"/> class.
        /// </summary>
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

            screenColorData = new Color[128 * 128];
            screenTexture = new Texture2D(GraphicsDevice, 128, 128, false, SurfaceFormat.Color);

            pico8Logo = Content.Load<Texture2D>("pico8");
            rasterizerState = new RasterizerState { MultiSampleAntiAlias = true };

            pico8 = new PicoInterpreter<Color>(ref screenColorData, ((r, g, b) => new Color(r, g, b)));
            pico8.LoadGame("test5.lua", new MoonSharpInterpreter());
            pico8.SetBtnPressedCallback(((x) => Keyboard.GetState().IsKeyDown((Keys)x)));
            pico8.SetControllerKeys(0, (int)Keys.Left, (int)Keys.Right, (int)Keys.Up, (int)Keys.Down, (int)Keys.Z, (int)Keys.X);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
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
            screenTexture.SetData(screenColorData);
            spriteBatch.Draw(screenTexture, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
