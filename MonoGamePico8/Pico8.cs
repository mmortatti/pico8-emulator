using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pico8Emulator;

namespace MonoGamePico8 {
	public class Pico8 : Game {
		private Emulator emulator;
		private GraphicsDeviceManager graphics;
		private SpriteBatch batch;

		public Pico8() {
			graphics = new GraphicsDeviceManager(this);
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

			if (!emulator.CartridgeLoader.Load("game")) {
				Exit();
			}
		}

		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);
			emulator.Update();
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