using System;
using System.Diagnostics;
using Pico8Emulator.unit.graphics;

namespace Pico8Emulator.unit.mem {
	public class MemoryUnit : Unit {
		public const int Size = RamAddress.End;
		
		public readonly byte[] Ram = new byte[Size];
		public DrawState DrawState;
		
		public MemoryUnit(Emulator emulator) : base(emulator) {
			DrawState = new DrawState(this);
		}

		public void LoadCartridgeData(byte[] cartridgeRom) {
			Buffer.BlockCopy(cartridgeRom, 0x0, Ram, 0, 0x4300);
		}
		
		public object Memset(int destination, byte val, int len) {
			for (int i = 0; i < len; i++) {
				Ram[destination + i] = val;
			}

			return null;
		}

		public object Memcpy(int destination, int source, int len) {
			Buffer.BlockCopy(Ram, source, Ram, destination, len);

			return null;
		}

		public object Memcpy(int destination, int source, int len, byte[] src) {
			Buffer.BlockCopy(src, source, Ram, destination, len);

			return null;
		}
		
		public byte Peek(int address) {
			if (address < 0 || address >= 0x8000) {
				return 0;
			}

			return Ram[address];
		}

		public object Poke(int address, byte val) {
			/*
			 * FIXME: better error handling
			 */
			Trace.Assert(address >= 0 && address < 0x8000, "bad memory access");
			Ram[address] = val;

			return null;
		}

		public int Peek2(int addr) {
			if (addr < 0 || addr >= 0x8000 - 1) {
				return 0;
			}

			return Ram[addr] | (Ram[addr + 1] << 8);
		}

		public object Poke2(int address, int val) {
			/*
			 * FIXME: better error handling
			 */
			Trace.Assert(address >= 0 && address < 0x8000, "bad memory access");

			Ram[address] = (byte) (val & 0xff);
			Ram[address + 1] = (byte) ((val >> 8) & 0xff);

			return null;
		}

		public double Peek4(int address) {
			if (address < 0 || address >= 0x8000 - 3) return 0;
			int right = Ram[address] | (Ram[address + 1] << 8);
			int left = ((Ram[address + 2] << 16) | (Ram[address + 3] << 24));

			return Util.FixedToFloat(left + right);
		}

		public object Poke4(int address, double val) {
			/*
			 * FIXME: better error handling
			 */
			Trace.Assert(address >= 0 && address < 0x8000, "bad memory access");

			Int32 f = Util.FloatToFixed(val);

			Ram[address] = (byte) (f & 0xff);
			Ram[address + 1] = (byte) ((f >> 8) & 0xff);
			Ram[address + 2] = (byte) ((f >> 16) & 0xff);
			Ram[address + 3] = (byte) ((f >> 24) & 0xff);

			return null;
		}

		public object Fget(int n, byte? f = null) {
			if (f.HasValue) {
				return (Peek(RamAddress.GfxProps + n) & (1 << f)) != 0;
			}

			return Peek(RamAddress.GfxProps + n);
		}

		public object Fset(int n, byte? f = null, bool? v = null) {
			if (!f.HasValue) {
				return null;
			}

			if (v.HasValue) {
				if (v.Value) {
					Poke(RamAddress.GfxProps + n, (byte) (Peek(RamAddress.GfxProps + n) | (1 << f)));
				} else {
					Poke(RamAddress.GfxProps + n, (byte) (Peek(RamAddress.GfxProps + n) & ~(1 << f)));
				}
			} else {
				Poke(RamAddress.GfxProps + n, (byte) (Peek(RamAddress.GfxProps + n) | f));
			}

			return null;
		}

		public byte Mget(int x, int y) {
			int addr = (y < 32 ? RamAddress.Map : RamAddress.GfxMap);
			y = y % 32;
			int index = (y * 128 + x);

			if (index < 0 || index > 32 * 128 - 1) {
				return 0x0;
			}

			return Ram[index + addr];
		}

		public object Mset(int x, int y, byte v) {
			int addr = (y < 32 ? RamAddress.Map : RamAddress.GfxMap);
			y = y % 32;
			int index = (y * 128 + x);

			if (index < 0 || index > 32 * 128 - 1) {
				return null;
			}

			Ram[index + addr] = v;

			return null;
		}

		public byte GetPixel(int x, int y, int offset = RamAddress.Screen) {
			int index = (y * 128 + x) / 2;

			if (index < 0 || index > 64 * 128 - 1) {
				return 0x10;
			}

			return Util.GetHalf(Ram[index + offset], x % 2 == 0);
		}

		public void WritePixel(int x, int y, byte color, int offset = RamAddress.Screen) {
			int index = (y * 128 + x) / 2;

			if (color >= Palette.Size || x < DrawState.ClipLeft || y < DrawState.ClipTop || x > DrawState.ClipRight || y > DrawState.ClipBottom) {
				return;
			}

			Util.SetHalf(ref Ram[index + offset], color, x % 2 == 0);
		}
	}
}