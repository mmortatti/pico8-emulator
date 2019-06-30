using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using MonoGame.Extended;
using IndependentResolutionRendering;
using pico8_interpreter.Pico8;

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
        Effect effect;
        Texture2D picoPalette;
        Texture2D BitwiseAndOp;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 600;

            Resolution.Init(ref graphics);
            Resolution.SetVirtualResolution(128, 128);
            Resolution.SetResolution(600, 600, false);
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
            pico8.LoadGameAndRun("test.lua");

            effect = Content.Load<Effect>("picoShader");
            picoPalette = Content.Load<Texture2D>("picoPalette");

            BitwiseAndOp = new Texture2D(GraphicsDevice, 255, 255, false, SurfaceFormat.Alpha8);
            byte[] opmap = new byte[255*255];
            for (int i = 0; i < 255; i++)
            {
                for (int j = 0; j < 255; j++)
                {
                    opmap[i*255 + j] = (byte)(i & j);
                }
            }
            BitwiseAndOp.SetData<byte>(opmap);
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
            //effect.Parameters["picoPalette"].SetValue(picoPalette);
            //effect.Parameters["bitwiseAnd"].SetValue(BitwiseAndOp);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, rasterizerState, null, Resolution.getTransformationMatrix());
            pico8.Draw();
            GraphicsDevice.Textures[1] = BitwiseAndOp;
            GraphicsDevice.Textures[2] = picoPalette;
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(BitwiseAndOp, new Vector2(0,0), null, null, null, 0, new Vector2(2,1), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
