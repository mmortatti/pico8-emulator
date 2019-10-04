using System;

namespace Pico8Emulator {
	public class PicoState : GameState {
		private Pico8<Color> pico8;
		private DynamicSoundEffectInstance soundInstance;
		private Texture2D texture;

		public override void Init() {
			base.Init();

			Engine.Instance.StateRenderer = new NativeGameRenderer();
			Input.EnableImGuiFocus = false;

			pico8 = new Pico8<Color>();

			pico8.AddLeftButtonDownFunction(() => Keyboard.GetState().IsKeyDown(Keys.Left), 0);
			pico8.AddDownButtonDownFunction(() => Keyboard.GetState().IsKeyDown(Keys.Down), 0);
			pico8.AddUpButtonDownFunction(() => Keyboard.GetState().IsKeyDown(Keys.Up), 0);
			pico8.AddRightButtonDownFunction(() => Keyboard.GetState().IsKeyDown(Keys.Right), 0);
			pico8.AddOButtonDownFunction(() => Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.C), 0);
			pico8.AddXButtonDownFunction(() => Keyboard.GetState().IsKeyDown(Keys.X), 0);

			pico8.screenColorData = new Color[128 * 128];
			pico8.rgbToColor = (r, g, b) => new Color(r, g, b);

			texture = new Texture2D(Engine.GraphicsDevice, 128, 128, false, SurfaceFormat.Color);

			soundInstance = new DynamicSoundEffectInstance(AudioUnit.sampleRate, AudioChannels.Mono);

			pico8.LoadGame("game.p8", new MoonScriptInterpreter());
		}

		public override void Update(float dt) {
			base.Update(dt);

			while (soundInstance.PendingBufferCount < 3) {
				var p8Buffer = pico8.audio.RequestBuffer();
				var samplesPerBuffer = p8Buffer.Length;
				var audioBuffer = new byte[samplesPerBuffer * 2];

				for (var i = 0; i < samplesPerBuffer; i += 1) {
					var floatSample = p8Buffer[i];

					var shortSample =
						(short) (floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

					if (!BitConverter.IsLittleEndian) {
						audioBuffer[i * 2] = (byte) (shortSample >> 8);
						audioBuffer[i * 2 + 1] = (byte) shortSample;
					} else {
						audioBuffer[i * 2] = (byte) shortSample;
						audioBuffer[i * 2 + 1] = (byte) (shortSample >> 8);
					}
				}

				soundInstance.SubmitBuffer(audioBuffer);
			}

			pico8.Update();
		}

		public override void Render() {
			base.Render();

			pico8.Draw();
			texture.SetData(pico8.screenColorData);

			Engine.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
			Graphics.Batch.Draw(texture, new Rectangle(0, 0, 512, 512), Color.White);
		}
	}
}