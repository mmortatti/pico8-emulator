using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGamePico8.backend;
using Pico8Emulator;

namespace MonoGamePico8 {
	public class Pico8 : Game {
		private const float UpdateTime = 1 / 30f;
		
		private Emulator _emulator;
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _batch;
		private FrameCounter _counter;
		private MonoGameGraphicsBackend _graphicsBackend;
		private float _deltaUpdate, _deltaDraw;
		
		public Pico8() {
			_graphics = new GraphicsDeviceManager(this);
			_counter = new FrameCounter();

			IsFixedTimeStep = false;
		}

		protected override void Initialize() {
			base.Initialize();
			
			_batch = new SpriteBatch(GraphicsDevice);
			
			_graphics.PreferredBackBufferWidth = 512;
			_graphics.PreferredBackBufferHeight = 512;
			_graphics.ApplyChanges();
		}

		protected override void LoadContent() {
			base.LoadContent();
			
			_graphicsBackend = new MonoGameGraphicsBackend(GraphicsDevice);
			_emulator = new Emulator(_graphicsBackend, new MonoGameAudioBackend(), new MonoGameInputBackend());

			if (!_emulator.CartridgeLoader.Load("testcarts/upcrawlnativerez.p8")) {
				Exit();
			}
		}

		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);
			var dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

			_deltaUpdate += dt;

			while (_deltaUpdate >= UpdateTime) {
				_deltaUpdate -= UpdateTime;
				_emulator.Update();
			}
			
			_counter.Update(dt);
			Window.Title = $"{_counter.AverageFramesPerSecond} fps {_emulator.Graphics.drawCalls} calls";
			_emulator.Graphics.drawCalls = 0;

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
					Keyboard.GetState().IsKeyDown(Keys.Escape)) {
				Exit();
			}
		}

		protected override void Draw(GameTime gameTime) {
			base.Draw(gameTime);

			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			_deltaDraw += dt;

			while (_deltaDraw >= UpdateTime) {
				_deltaDraw -= UpdateTime;
				_emulator.Draw();
			}

			GraphicsDevice.Clear(Color.Black);
			
			_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			_batch.Draw(_graphicsBackend.Surface, new Rectangle(0, 0, 512, 512), Color.White);
			_batch.End();
		}
	}
}