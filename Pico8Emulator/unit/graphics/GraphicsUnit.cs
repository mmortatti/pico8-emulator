using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pico8Emulator.lua;
using Pico8Emulator.unit.mem;

namespace Pico8Emulator.unit.graphics {
	public class GraphicsUnit : Unit {
		public const int ScreenSize = 128 * 128;
		
		public Texture2D Surface;
		public int DrawCalls;
		
		private Color[] screenColorData = new Color[ScreenSize];

		public GraphicsUnit(Emulator emulator, GraphicsDevice graphics) : base(emulator) {
			Surface = new Texture2D(graphics, 128, 128, false, SurfaceFormat.Color);
		}

		public override void Init() {
			base.Init();
			Emulator.Memory.DrawState.DrawColor = 6;
		}

		public override void DefineApi(LuaInterpreter script) {
			base.DefineApi(script);
			
			script.AddFunction("pset", (Action<int, int, byte?>) Pset);
			script.AddFunction("pget", (Func<int, int, byte>) Pget);
			script.AddFunction("flip", (Action) Flip);
			script.AddFunction("cls", (Action<byte?>) Cls);

			script.AddFunction("spr", (Action<int, int, int, int?, int?, bool, bool>) Spr);
			script.AddFunction("sspr", (Action<int, int, int, int, int, int, int?, int?, bool, bool>) Sspr);
			
			script.AddFunction("print", (Action<string, int?, int?, byte?>) Print);
			script.AddFunction("map", (Action<int?, int?, int?, int?, int?, int?, byte?>) Map);
			
			script.AddFunction("line", (Action<int, int, int?, int?, byte?>) Line);
			script.AddFunction("rect", (Action<int, int, int, int, byte?>) Rect);
			script.AddFunction("rectfill", (Action<int, int, int, int, byte?>) Rectfill);
			script.AddFunction("circ", (Action<int, int, double?, byte?>) Circ);
			script.AddFunction("circfill", (Action<int, int, double?, byte?>) Circfill);

			script.AddFunction("sset", (Action<int, int, byte?>) Sset);
			script.AddFunction("sget", (Func<int, int, byte>) Sget);
		}

		public void Cls(byte? color) {
			var c = 0;
			
			if (color.HasValue) {
				var v = color.Value % 16;
				c = v | (v << 4);
			}
			
			for (var i = 0; i < 0x2000; i++) {
				Emulator.Memory.Ram[RamAddress.Screen + i] = (byte) c;
			}
		}

		public static byte ColorToPalette(Color col) {
			for (var i = 0; i < 16; i += 1) {
				if (Palette.StandardPalette[i] == col) {
					return (byte) i;
				}
			}

			return 0;
		}
		
		public void Print(object s, int? x = null, int? y = null, byte? c = null) {
			if (x.HasValue) {
				Emulator.Memory.DrawState.CursorX = x.Value;
			} else {
				x = Emulator.Memory.DrawState.CursorX;
			}

			if (y.HasValue) {
				Emulator.Memory.DrawState.CursorY = y.Value;
			} else {
				y = Emulator.Memory.DrawState.CursorY;
				Emulator.Memory.DrawState.CursorY += 6;
			}

			var xOrig = x.Value;
			var prtStr = s.ToString().ToUpper();

			if (prtStr.Contains("U__")) {
				prtStr = LuaPatcher.ReplaceCodesWithEmojis(prtStr);
			}

			foreach (var l in prtStr) {
				if (l == '\n') {
					y += 6;
					x = xOrig;
					continue;
				}

				if (Font.Dictionary.ContainsKey(l)) {
					byte[,] digit = Font.Dictionary[l];

					for (int i = 0; i < digit.GetLength(0); i += 1) {
						for (int j = 0; j < digit.GetLength(1); j += 1) {
							if (digit[i, j] == 1) {
								Pset(x.Value + j, y.Value + i, c);
							}
						}
					}

					x += digit.GetLength(1) + 1;
				}
			}
		}

		public void Map(int? cellX, int? cellY, int? sx, int? sy, int? cellW, int? cellH, byte? layer = null) {
			var x = cellX ?? 0;
			var y = cellY ?? 0;
			var px = sx ?? 0;
			var py = sy ?? 0;
			var tw = cellW ?? 16;
			var th = cellH ?? 16;

			for (var h = 0; h < th; h++) {
				for (var w = 0; w < tw; w++) {
					var addr = (y + h) < 32 ? RamAddress.Map : RamAddress.GfxMap;
					var spr = Emulator.Memory.Peek(addr + (y + h) % 32 * 128 + x + w);

					// Spr index 0 is reserved for empty tiles
					if (spr == 0) {
						continue;
					}
					
					// If layer has not been specified, draw regardless
					if (!layer.HasValue || ((byte) Emulator.Memory.Fget(spr, null) & layer.Value) != 0) {
						Spr(spr, px + 8 * w, py + 8 * h, 1, 1, false, false);
					}
				}
			}
		}

		public void Flip() {
			var ram = Emulator.Memory.Ram;

			for (int i = 0; i < 8192; i++) {
				byte val = ram[i + RamAddress.Screen];
				byte left = (byte) (val & 0x0f);
				byte right = (byte) (val >> 4);

				byte lc = (byte) Emulator.Memory.DrawState.GetScreenColor(left);
				byte rc = (byte) Emulator.Memory.DrawState.GetScreenColor(right);

				// Convert color if alternative palette bit is set.
				lc = (byte) ((lc & 0b10000000) != 0 ? (lc & 0b00001111) + 16 : (lc & 0b00001111));
				rc = (byte) ((rc & 0b10000000) != 0 ? (rc & 0b00001111) + 16 : (rc & 0b00001111));

				screenColorData[i * 2] = Palette.StandardPalette[lc];
				screenColorData[i * 2 + 1] = Palette.StandardPalette[rc];
			}
			
			Surface.SetData(screenColorData);
		}

		public void Spr(int n, int x, int y, int? w = null, int? h = null, bool flipX = false, bool flipY = false) {
			if (n < 0 || n > 255) {
				return;
			}

			var sprX = (n % 16) * 8;
			var sprY = (n / 16) * 8;
			var width = 1;
			var height = 1;

			if (w.HasValue) {
				width = w.Value;
			}

			if (h.HasValue) {
				height = h.Value;
			}

			for (var i = 0; i < 8 * width; i++) {
				for (var j = 0; j < 8 * height; j++) {
					Psett(x + (flipX ? 8 * width - i : i), y + (flipY ? 8 * height - j : j), Sget(i + sprX, j + sprY));
				}
			}

			DrawCalls++;
		}

		public void Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int? dw = null, int? dh = null,
			bool flipX = false, bool flipY = false) {
			
			if (!dw.HasValue) {
				dw = sw;
			}

			if (!dh.HasValue) {
				dh = sh;
			}

			float ratioX = (float) sw / (float) dw.Value;
			float ratioY = (float) sh / (float) dh.Value;
			float x = sx;
			float y = sy;
			float screenX = dx;
			float screenY = dy;

			while (x < sx + sw && screenX < dx + dw) {
				y = sy;
				screenY = dy;

				while (y < sy + sh && screenY < dy + dh) {
					byte sprColor = Sget((int) x, (int) y);

					Psett((flipX ? dx + dw.Value - ((int) screenX - dx) : (int) screenX),
						(flipY ? dy + dh.Value - ((int) screenY - dy) : (int) screenY), sprColor);

					y += ratioY;
					screenY += 1;
				}

				x += ratioX;
				screenX += 1;
			}
		}
		
		public byte Sget(int x, int y) {
			return Emulator.Memory.GetPixel(x, y, RamAddress.Gfx);
		}

		public void Sset(int x, int y, byte? col = null) {
			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			Emulator.Memory.WritePixel(x, y, Emulator.Memory.DrawState.DrawColor, RamAddress.Gfx);
		}

		public void Pset(int x, int y, byte? col = null) {
			x -= Emulator.Memory.DrawState.CameraX;
			y -= Emulator.Memory.DrawState.CameraY;

			if (!col.HasValue) {
				col = Emulator.Memory.DrawState.DrawColor;
			}

			int f = Emulator.Memory.DrawState.GetFillPBit(x, y);

			if (f == 0) {
				// Do not consider transparency bit for this operation.
				Emulator.Memory.WritePixel(x, y, (byte) (Emulator.Memory.DrawState.GetDrawColor(col.Value & 0x0f) & 0x0f));
			} else if (!Emulator.Memory.DrawState.FillpTransparent) {
				// Do not consider transparency bit for this operation.
				Emulator.Memory.WritePixel(x, y, (byte) (Emulator.Memory.DrawState.GetDrawColor(col.Value >> 4) & 0x0f));
			}

			Emulator.Memory.DrawState.DrawColor = (byte) (col.Value & 0x0f);
		}

		private void Psett(int x, int y, byte? col = null) {
			x -= Emulator.Memory.DrawState.CameraX;
			y -= Emulator.Memory.DrawState.CameraY;

			if (!col.HasValue) {
				col = Emulator.Memory.DrawState.DrawColor;
			}

			var f = Emulator.Memory.DrawState.GetFillPBit(x, y);
			var t = !Emulator.Memory.DrawState.IsTransparent(col.Value);
			
			if (f == 0) {
				Emulator.Memory.WritePixel(x, y, Emulator.Memory.DrawState.GetDrawColor(col.Value & 0x0f));
			} else if (!Emulator.Memory.DrawState.FillpTransparent && t) {
				Emulator.Memory.WritePixel(x, y, (Emulator.Memory.DrawState.GetDrawColor(col.Value >> 4)));
			}

			// We only want to set the default color if the color given is not transparent.
			if (t) {
				Emulator.Memory.DrawState.DrawColor = (byte) (col.Value & 0x0f);
			}
		}

		public byte Pget(int x, int y) {
			return Emulator.Memory.GetPixel((int) x, (int) y);
		}
		
		public void Rect(int x0, int y0, int x1, int y1, byte? col = null) {
			Line(x0, y0, x1, y0, col);
			Line(x0, y0, x0, y1, col);
			Line(x1, y1, x1, y0, col);
			Line(x1, y1, x0, y1, col);
		}

		public void Rectfill(int x0, int y0, int x1, int y1, byte? col = null) {
			if (y0 > y1) {
				Util.Swap(ref y0, ref y1);
			}
			
			for (var y = y0; y <= y1; y++) {
				Line(x0, y, x1, y, col);
			}
		}

		public void Line(int x0, int y0, int? x1 = null, int? y1 = null, byte? col = null) {
			if (x1.HasValue) {
				Emulator.Memory.DrawState.LineX = x1.Value;
			}

			if (y1.HasValue) {
				Emulator.Memory.DrawState.LineY = y1.Value;
			}

			var x0_screen = x0;
			var y0_screen = y0;
			var x1_screen = Emulator.Memory.DrawState.LineX;
			var y1_screen = Emulator.Memory.DrawState.LineY;

			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			var steep = false;

			if (Math.Abs(x1_screen - x0_screen) < Math.Abs(y1_screen - y0_screen)) {
				Util.Swap(ref x0_screen, ref y0_screen);
				Util.Swap(ref x1_screen, ref y1_screen);
				steep = true;
			}

			if (x0_screen > x1_screen) {
				Util.Swap(ref x0_screen, ref x1_screen);
				Util.Swap(ref y0_screen, ref y1_screen);
			}

			var dx = (int) (x1_screen - x0_screen);
			var dy = (int) (y1_screen - y0_screen);
			var d_err = 2 * Math.Abs(dy);
			var err = 0;
			var y = (int) y0_screen;

			for (var x = (int) x0_screen; x <= x1_screen; x++) {
				if (steep) {
					Pset(y, x);
				} else {
					Pset(x, y);
				}

				err += d_err;

				if (err > dx) {
					y += y1_screen > y0_screen ? 1 : -1;
					err -= dx * 2;
				}
			}
		}

		public void Circ(int x, int y, double? r, byte? col = null) {
			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			DrawCircle(x, y, (int) Math.Ceiling(r ?? 1), false);
		}

		public void Circfill(int x, int y, double? r, byte? col = null) {
			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			DrawCircle(x, y, (int) (r ?? 1), true);
		}

		private void plot4(int x, int y, int offX, int offY, bool fill) {
			if (fill) {
				Line((x - offX), (y + offY), (x + offX), (y + offY), null);

				if (offY != 0) {
					Line((x - offX), (y - offY), (x + offX), (y - offY), null);
				}
			} else {
				Pset((x - offX), (y + offY), null);
				Pset((x + offX), (y + offY), null);

				if (offY != 0) {
					Pset((x - offX), (y - offY), null);
					Pset((x + offX), (y - offY), null);
				}
			}
		}

		private void DrawCircle(int posX, int posY, int r, bool fill) {
			var x = r;
			var y = 0;
			double err = 1 - r;

			while (y <= x) {
				plot4(posX, posY, x, y, fill);

				if (err < 0) {
					err = err + 2 * y + 3;
				} else {
					if (x != y) {
						plot4(posX, posY, y, x, fill);
					}

					x = x - 1;
					err = err + 2 * (y - x) + 3;
				}

				y = y + 1;
			}
		}
	}
}