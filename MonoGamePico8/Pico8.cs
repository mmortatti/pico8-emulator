using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pico8Emulator;

namespace MonoGamePico8 {
	public class Pico8 : Game {
		private const float UpdateTime = 1 / 60f;
		
		private Emulator emulator;
		private GraphicsDeviceManager graphics;
		private SpriteBatch batch;
		private FrameCounter counter;
		private float delta;
		
		public Pico8() {
			graphics = new GraphicsDeviceManager(this);
			counter = new FrameCounter();

			IsFixedTimeStep = false;
		}

		protected override void Initialize() {
			base.Initialize();
			
			batch = new SpriteBatch(GraphicsDevice);
			
			graphics.PreferredBackBufferWidth = 512;
			graphics.PreferredBackBufferHeight = 512;
			graphics.ApplyChanges();
		}

		protected override void LoadContent() {
			base.LoadContent();
			
			emulator = new Emulator(GraphicsDevice);

			if (!emulator.CartridgeLoader.Load("test")) {
				Exit();
			}
		}

		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);
			var dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

			delta += dt;

			while (delta >= UpdateTime) {
				delta -= UpdateTime;
				emulator.Update();
			}
			
			counter.Update(dt);
			Window.Title = $"{counter.AverageFramesPerSecond} fps {emulator.Graphics.DrawCalls} calls";
			emulator.Graphics.DrawCalls = 0;
		}

		protected override void Draw(GameTime gameTime) {
			base.Draw(gameTime);

			emulator.Draw();
			
			GraphicsDevice.Clear(Color.Black);
			
			batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			batch.Draw(emulator.Graphics.Surface, new Rectangle(0, 0, 512, 512), Color.White);
			batch.End();
		}
	}
}