using System;
using Pico8Emulator.unit.graphics;

namespace Pico8Emulator.unit.mem {
	public class DrawState {
		private MemoryUnit memory;
		private byte[] ram;
		
		public DrawState(MemoryUnit mem) {
			memory = mem;
			ram = mem.Ram;
			
			
			ram[RamAddress.Palette0] = 0x10;
			ram[RamAddress.Palette1] = 0x0;

			for (int i = 1; i < Palette.Size; i++) {
				ram[RamAddress.Palette0 + i] = (byte) i;
				ram[RamAddress.Palette1 + i] = (byte) i;
			}

			ram[RamAddress.ClipLeft] = 0;
			ram[RamAddress.ClipTop] = 0;
			ram[RamAddress.ClipRight] = 127;
			ram[RamAddress.ClipBottom] = 127;
		}
		
		public byte DrawColor {
			get => ram[RamAddress.DrawColor];
			set => ram[RamAddress.DrawColor] = (byte) (value & 0xff);
		}

		public int CursorX {
			get => ram[RamAddress.CursorX];
			set => ram[RamAddress.CursorX] = (byte) value;
		}

		public int CursorY {
			get => ram[RamAddress.CursorY];
			set => ram[RamAddress.CursorY] = (byte) value;
		}

		public int CameraX {
			get => ((sbyte) (ram[RamAddress.CameraX + 1]) << 8) | ram[RamAddress.CameraX];
			set {
				ram[RamAddress.CameraX] = (byte) (value & 0xff);
				ram[RamAddress.CameraX + 1] = (byte) (value >> 8);
			}
		}

		public int CameraY {
			get => ((sbyte) (ram[RamAddress.CameraY + 1]) << 8) | ram[RamAddress.CameraY];
			set {
				ram[RamAddress.CameraY] = (byte) (value & 0xff);
				ram[RamAddress.CameraY + 1] = (byte) (value >> 8);
			}
		}

		public int LineX {
			get => ((sbyte) (ram[RamAddress.LineX + 1]) << 8) | ram[RamAddress.LineX];
			set {
				ram[RamAddress.LineX] = (byte) (value & 0xff);
				ram[RamAddress.LineX + 1] = (byte) (value >> 8);
			}
		}

		public int LineY {
			get => ((sbyte) (ram[RamAddress.LineY + 1]) << 8) | ram[RamAddress.LineY];
			set {
				ram[RamAddress.LineY] = (byte) (value & 0xff);
				ram[RamAddress.LineY + 1] = (byte) (value >> 8);
			}
		}

		public byte ClipLeft {
			get => (byte) (ram[RamAddress.ClipLeft] & 0x7f);
			set => ram[RamAddress.ClipLeft] = value;
		}

		public byte ClipTop {
			get => (byte) (ram[RamAddress.ClipTop] & 0x7f);
			set => ram[RamAddress.ClipTop] = value;
		}

		public byte ClipRight {
			get => (byte) (ram[RamAddress.ClipRight] & 0x7f);
			set => ram[RamAddress.ClipRight] = value;
		}

		public byte ClipBottom {
			get => (byte) (ram[RamAddress.ClipBottom] & 0x7f);
			set => ram[RamAddress.ClipBottom] = value;
		}

		/*
		 * FIXME: completely broken
		 */
		public int ScreenX {
			get {
				// byte i = memory.Peek(Address.ScreenX);
				return 128;
			}
		}

		/*
		 * FIXME: completely broken
		 */
		public int ScreenY {
			get {
				// byte i = Peek(Address.ScreenY);
				return 128;
			}
		}

		public int FillPattern {
			get => (ram[RamAddress.FillPattern + 1] << 8) | ram[RamAddress.FillPattern];
			set {
				ram[RamAddress.FillPattern] = (byte) (value & 0xff);
				ram[RamAddress.FillPattern + 1] = (byte) (value >> 8 & 0xff);
			}
		}

		public bool FillpTransparent {
			get => ram[RamAddress.FillPattern + 2] != 0;
			set => ram[RamAddress.FillPattern + 2] = (byte) (value ? 1 : 0);
		}
		
		public object Fillp(double? p = null) {
			if (!p.HasValue) {
				p = 0;
			}

			FillPattern = (int) p.Value;
			FillpTransparent = Math.Floor(p.Value) < p.Value;

			return null;
		}

		public int GetFillPBit(int x, int y) {
			x %= 4;
			y %= 4;
			
			var i = y * 4 + x;
			var mask = (1 << 15) >> i;

			return (FillPattern & mask) >> (15 - i);
		}
		
		public object Cursor(int x, int y, byte? col = null) {
			CursorX = x;
			CursorY = y;

			if (col.HasValue) {
				DrawColor = col.Value;
			}

			return null;
		}

		public object Color(byte col) {
			DrawColor = col;
			return null;
		}

		public byte GetDrawColor(int color) {
			if (color < 0 || color > 15) {
				return 0;
			}

			return ram[RamAddress.Palette0 + color];
		}

		public int GetScreenColor(int color) {
			if (color < 0 || color > 15) {
				color = 0;
			}

			return ram[RamAddress.Palette1 + color];
		}

		public void SetTransparent(int col) {
			if (col >= 0 && col <= 15) {
				ram[RamAddress.Palette0 + col] &= 0x0f;
				ram[RamAddress.Palette0 + col] |= 0x10;
			}
		}

		public bool IsTransparent(int col) {
			return (ram[RamAddress.Palette0 + col] & 0x10) != 0;
		}

		public void ResetTransparent(int col) {
			if (col >= 0 && col <= 15) {
				ram[RamAddress.Palette0 + col] &= 0x0f;
			}
		}

		public void SetDrawPalette(int c0, int c1) {
			if (c0 >= 0 && c0 <= 15 && c1 >= 0 && c1 <= 15) {
				ram[RamAddress.Palette0 + c0] = (byte) c1;
			}
		}

		public void SetScreenPalette(int c0, int c1) {
			if (c0 >= 0 && c0 <= 15) {
				ram[RamAddress.Palette1 + c0] = (byte) c1;
			}
		}

		public object Camera(int? x = null, int? y = null) {
			if (!x.HasValue && !y.HasValue) {
				CameraX = 0;
				CameraY = 0;

				return null;
			}

			if (x.HasValue) {
				CameraX = x.Value;
			}

			if (!y.HasValue) {
				y = 0;
			}

			CameraY = y.Value;
			return null;
		}
	}
}