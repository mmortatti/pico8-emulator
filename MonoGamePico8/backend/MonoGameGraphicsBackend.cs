using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pico8Emulator;
using Pico8Emulator.backend;
using Pico8Emulator.unit.graphics;
using Pico8Emulator.unit.mem;

namespace MonoGamePico8.backend {
	public class MonoGameGraphicsBackend : GraphicsBackend {
		public Texture2D Surface;
		
		private GraphicsDevice graphics;
		private Color[] screenColorData = new Color[GraphicsUnit.ScreenSize];
		private Color[] palette;

		private SpriteBatch spriteBatch;

		private static Mutex mut = new Mutex();

		public MonoGameGraphicsBackend(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) {
			graphics = graphicsDevice;
			this.spriteBatch = spriteBatch;
			palette = new Color[Palette.Size];

			for (var i = 0; i < Palette.Size; i++) {
				palette[i] = new Color(Palette.standardPalette[i, 0], Palette.standardPalette[i, 1], Palette.standardPalette[i, 2]);
			}
		}

		public override void Draw()
		{
			mut.WaitOne();
			Surface.SetData(screenColorData);
			mut.ReleaseMutex();

			spriteBatch.Draw(Surface, new Rectangle(0, 0, 512, 512), Color.White);
		}
		
		public override void CreateSurface() {
			Surface = new Texture2D(graphics, 128, 128, false, SurfaceFormat.Color);
		}

		public override void Flip() {
			var ram = Emulator.Memory.ram;
			var drawState = Emulator.Memory.drawState;

			mut.WaitOne();
			for (var i = 0; i < 8192; i++) {
				var val = ram[i + RamAddress.Screen];

				screenColorData[i * 2] = palette[drawState.GetScreenColor(val & 0x0f)];
				screenColorData[i * 2 + 1] = palette[drawState.GetScreenColor(val >> 4)];
			}
			mut.ReleaseMutex();
		}
		
		private byte ColorToPalette(Color col) {
			for (var i = 0; i < 16; i += 1) {
				if (palette[i] == col) {
					return (byte) i;
				}
			}

			return 0;
		}
	}
}