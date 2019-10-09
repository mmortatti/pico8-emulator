using System;
using System.IO;
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

		public MonoGameGraphicsBackend(GraphicsDevice graphicsDevice) {
			graphics = graphicsDevice;
			palette = new Color[Palette.Size];

			for (var i = 0; i < Palette.Size; i++) {
				palette[i] = new Color(Palette.StandardPalette[i, 0], Palette.StandardPalette[i, 1], Palette.StandardPalette[i, 2]);
			}
		}
		
		public override void CreateSurface() {
			Surface = new Texture2D(graphics, 128, 128, false, SurfaceFormat.Color);
		}

		public override void Flip() {
			var ram = Emulator.Memory.Ram;
			var drawState = Emulator.Memory.DrawState;

			for (var i = 0; i < 8192; i++) {
				var val = ram[i + RamAddress.Screen];

				screenColorData[i * 2] = palette[drawState.GetScreenColor(val & 0x0f)];
				screenColorData[i * 2 + 1] = palette[drawState.GetScreenColor(val >> 4)];
			}
			
			Surface.SetData(screenColorData);
		}
		
		private byte ColorToPalette(Color col) {
			for (var i = 0; i < 16; i += 1) {
				if (palette[i] == col) {
					return (byte) i;
				}
			}

			return 0;
		}

		public override void Import(string path, bool onlyHalf) {
			var texture = Texture2D.FromStream(graphics, new FileStream(path, FileMode.Open));
			
			if (texture.Height != 128 || texture.Width != 128) {
				throw new ArgumentException($"{path} must be a 128x128 image, but is {texture.Width}x{texture.Width}.");
			}

			var bound = onlyHalf ? 64 : 128;
			var data = new Color[GraphicsUnit.ScreenSize];

			texture.GetData(data);

			for (var i = 0; i < bound; i += 1) {
				for (var j = 0; j < 128; j += 1) {
					Emulator.Graphics.Sset(j, i, ColorToPalette(data[j + i * 128]));
				}
			}

			texture.Dispose();
		}

		public override void Export(string path) {
			var texture = new Texture2D(graphics, 128, 128, false, SurfaceFormat.Color);
			var data = new Color[GraphicsUnit.ScreenSize];
			
			for (var i = 0; i < 128; i += 1) {
				for (var j = 0; j < 128; j += 1) {
					data[j + i * 128] = palette[Emulator.Graphics.Sget(j, i)];
				}
			}

			texture.SetData(data);
			texture.SaveAsPng(File.Create(path), 128, 128);
			texture.Dispose();
		}
	}
}