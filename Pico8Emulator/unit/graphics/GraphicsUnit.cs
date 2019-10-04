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
			
			script.AddFunction("pset", (Func<int, int, byte?, object>) Pset);
			script.AddFunction("flip", (Action) Flip);
		}

		public object Cls() {
			for (var i = 0; i < 0x2000; i++) {
				Emulator.Memory.Ram[RamAddress.Screen + i] = 0;
			}

			return null;
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
			var prtStr = s.ToString();

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

		public object Map(int cel_x, int cel_y, int sx, int sy, int cel_w, int cel_h, byte? layer = null) {
			for (int h = 0; h < cel_h; h++) {
				for (int w = 0; w < cel_w; w++) {
					int addr = (cel_y + h) < 32 ? RamAddress.Map : RamAddress.GfxMap;
					byte spr_index = Emulator.Memory.Peek(addr + (cel_y + h) % 32 * 128 + cel_x + w);
					byte flags = (byte) Emulator.Memory.Fget(spr_index, null);

					// Spr index 0 is reserved for empty tiles
					if (spr_index == 0) {
						continue;
					}

					// If layer has not been specified, draw regardless
					if (!layer.HasValue || (flags & layer.Value) != 0) {
						Spr(spr_index, sx + 8 * w, sy + 8 * h, 1, 1, false, false);
					}
				}
			}

			return null;
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

		public object Spr(int n, int x, int y, int? w = null, int? h = null, bool? flip_x = null, bool? flip_y = null) {
			if (n < 0 || n > 255) {
				return null;
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

			bool flipX = false, flipY = false;

			if (flip_x.HasValue) {
				flipX = flip_x.Value;
			}

			if (flip_y.HasValue) {
				flipY = flip_y.Value;
			}

			for (int i = 0; i < 8 * width; i++) {
				for (int j = 0; j < 8 * height; j++) {
					byte sprColor = Sget(i + sprX, j + sprY);
					Psett(x + (flipX ? 8 * width - i : i), y + (flipY ? 8 * height - j : j), sprColor);
				}
			}

			return null;
		}

		public object Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int? dw = null, int? dh = null,
			bool? flip_x = null, bool? flip_y = null) {
			if (!dw.HasValue) {
				dw = sw;
			}

			if (!dh.HasValue) {
				dh = sh;
			}

			if (!flip_x.HasValue) {
				flip_x = false;
			}

			if (!flip_y.HasValue) {
				flip_y = false;
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

					Psett((flip_x.Value ? dx + dw.Value - ((int) screenX - dx) : (int) screenX),
						(flip_y.Value ? dy + dh.Value - ((int) screenY - dy) : (int) screenY),
						sprColor);

					y += ratioY;
					screenY += 1;
				}

				x += ratioX;
				screenX += 1;
			}

			return null;
		}
		
		public byte Sget(int x, int y) {
			return Emulator.Memory.GetPixel(x, y, RamAddress.Gfx);
		}

		public object Sset(int x, int y, byte? col = null) {
			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			Emulator.Memory.WritePixel(x, y, Emulator.Memory.DrawState.DrawColor, RamAddress.Gfx);

			return null;
		}

		public object Pset(int x, int y, byte? col = null) {
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
			return null;
		}

		private object Psett(int x, int y, byte? col = null) {
			x -= Emulator.Memory.DrawState.CameraX;
			y -= Emulator.Memory.DrawState.CameraY;

			if (!col.HasValue) {
				col = Emulator.Memory.DrawState.DrawColor;
			}

			int f = Emulator.Memory.DrawState.GetFillPBit(x, y);

			if (f == 0) {
				Emulator.Memory.WritePixel(x, y, Emulator.Memory.DrawState.GetDrawColor(col.Value & 0x0f));
			} else if (!Emulator.Memory.DrawState.FillpTransparent && !Emulator.Memory.DrawState.IsTransparent(col.Value)) {
				Emulator.Memory.WritePixel(x, y, (Emulator.Memory.DrawState.GetDrawColor(col.Value >> 4)));
			}

			// We only want to set the default color if the color given is not transparent.
			if (!Emulator.Memory.DrawState.IsTransparent(col.Value)) {
				Emulator.Memory.DrawState.DrawColor = (byte) (col.Value & 0x0f);
			}

			return null;
		}

		public byte Pget(int x, int y) {
			return Emulator.Memory.GetPixel((int) x, (int) y);
		}
		
		public object Palt(int? col = null, bool? t = null) {
			if (!col.HasValue || !t.HasValue) {
				Emulator.Memory.DrawState.SetTransparent(0);

				for (byte i = 1; i < 16; i++) {
					Emulator.Memory.DrawState.ResetTransparent(i);
				}

				return null;
			}

			if (t.Value) {
				Emulator.Memory.DrawState.SetTransparent(col.Value);
			} else {
				Emulator.Memory.DrawState.ResetTransparent(col.Value);
			}

			return null;
		}

		public object Pal(int? c0 = null, int? c1 = null, int p = 0) {
			if (!c0.HasValue || !c1.HasValue) {
				for (byte i = 0; i < 16; i++) {
					Emulator.Memory.DrawState.SetDrawPalette(i, i);
					Emulator.Memory.DrawState.SetScreenPalette(i, i);
				}

				Palt(null, null);
				return null;
			}

			if (p == 0) {
				Emulator.Memory.DrawState.SetDrawPalette(c0.Value, c1.Value);
			} else if (p == 1) {
				Emulator.Memory.DrawState.SetScreenPalette(c0.Value, c1.Value);
			}

			return null;
		}

		public object Clip(int? x = null, int? y = null, int? w = null, int? h = null) {
			if (!x.HasValue || !y.HasValue || !w.HasValue || !h.HasValue) {
				return null;
			}

			Emulator.Memory.DrawState.ClipLeft = (byte) x.Value;
			Emulator.Memory.DrawState.ClipTop = (byte) y.Value;
			Emulator.Memory.DrawState.ClipRight = (byte) (x.Value + w.Value);
			Emulator.Memory.DrawState.ClipBottom = (byte) (y.Value + h.Value);

			return null;
		}
		
		public object Rect(int x0, int y0, int x1, int y1, byte? col = null) {
			Line(x0, y0, x1, y0, col);
			Line(x0, y0, x0, y1, col);
			Line(x1, y1, x1, y0, col);
			Line(x1, y1, x0, y1, col);

			return null;
		}

		public object Rectfill(int x0, int y0, int x1, int y1, byte? col = null) {
			if (y0 > y1) {
				Util.Swap(ref y0, ref y1);
			}

			for (int y = y0; y < y1; y++) {
				Line(x0, y, x1, y, col);
			}

			return null;
		}

		public object Line(int x0, int y0, int? x1 = null, int? y1 = null, byte? col = null) {
			if (x1.HasValue) {
				Emulator.Memory.DrawState.LineX = x1.Value;
			}

			if (y1.HasValue) {
				Emulator.Memory.DrawState.LineY = y1.Value;
			}

			int x0_screen = x0;
			int y0_screen = y0;
			int x1_screen = Emulator.Memory.DrawState.LineX;
			int y1_screen = Emulator.Memory.DrawState.LineY;

			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			bool steep = false;

			if (Math.Abs(x1_screen - x0_screen) < Math.Abs(y1_screen - y0_screen)) {
				Util.Swap(ref x0_screen, ref y0_screen);
				Util.Swap(ref x1_screen, ref y1_screen);
				steep = true;
			}

			if (x0_screen > x1_screen) {
				Util.Swap(ref x0_screen, ref x1_screen);
				Util.Swap(ref y0_screen, ref y1_screen);
			}

			int dx = (int) (x1_screen - x0_screen);
			int dy = (int) (y1_screen - y0_screen);
			int d_err = 2 * Math.Abs(dy);
			int err = 0;
			int y = (int) y0_screen;

			for (int x = (int) x0_screen; x <= x1_screen; x++) {
				if (steep) {
					Pset(y, x, null);
				} else {
					Pset(x, y, null);
				}

				err += d_err;

				if (err > dx) {
					y += y1_screen > y0_screen ? 1 : -1;
					err -= dx * 2;
				}
			}

			return null;
		}

		public object Circ(int x, int y, double r, byte? col = null) {
			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			DrawCircle(x, y, (int) Math.Ceiling(r), false);
			return null;
		}

		public object CircFill(int x, int y, double r, byte? col = null) {
			if (col.HasValue) {
				Emulator.Memory.DrawState.DrawColor = col.Value;
			}

			DrawCircle(x, y, (int) r, true);
			return null;
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
			int cx = posX, cy = posY;
			int x = r;
			int y = 0;
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