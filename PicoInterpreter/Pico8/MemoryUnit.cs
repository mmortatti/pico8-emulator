using System;

namespace pico8_interpreter.Pico8
{
    public class MemoryUnit
    {
        public const int ADDR_GFX = 0x0,
                         ADDR_GFX_MAP = 0x1000,
                         ADDR_MAP = 0x2000,
                         ADDR_GFX_PROPS = 0x3000,
                         ADDR_SONG = 0x3100,
                         ADDR_SFX = 0x3200,
                         ADDR_USER = 0x4300,
                         ADDR_CART = 0x5e00,
                         ADDR_PALETTE_0 = 0x5f00,
                         ADDR_PALETTE_1 = 0x5f10,
                         ADDR_CLIP_X0 = 0x5f20,
                         ADDR_CLIP_Y0 = 0x5f21,
                         ADDR_CLIP_X1 = 0x5f22,
                         ADDR_CLIP_Y1 = 0x5f23,
                         ADDR_DRAW_COL = 0x5f25,
                         ADDR_CURSOR_X = 0x5f26,
                         ADDR_CURSOR_Y = 0x5f27,
                         ADDR_CAMERA_X = 0x5f28,
                         ADDR_CAMERA_Y = 0x5f2a,
                         ADDR_FILLP = 0x5f31,
                         ADDR_LINE_X = 0x5f3c,
                         ADDR_LINE_Y = 0x5f3e,
                         ADDR_SCREEN = 0x6000,
                         ADDR_END = 0x8000;
        private byte[] screen;
        public byte[] ram { get; }
        public byte[] FrameBuffer
        {
            get
            {
                Buffer.BlockCopy(ram, 0x6000, screen, 0, 0x2000);
                return screen;
            }
        }

        public byte DrawColor
        {
            get
            {
                return ram[ADDR_DRAW_COL];
            }
            set
            {
                ram[ADDR_DRAW_COL] = (byte)(value & 0xff);
            }
        }

        public int cameraX
        {
            get => ((sbyte)(ram[ADDR_CAMERA_X + 1]) << 8) | ram[ADDR_CAMERA_X];
            set
            {
                ram[ADDR_CAMERA_X] = (byte)(value & 0xff);
                ram[ADDR_CAMERA_X + 1] = (byte)(value >> 8);
            }
        }
        public int cameraY
        {
            get => ((sbyte)(ram[ADDR_CAMERA_Y + 1]) << 8) | ram[ADDR_CAMERA_Y];
            set
            {
                ram[ADDR_CAMERA_Y] = (byte)(value & 0xff);
                ram[ADDR_CAMERA_Y + 1] = (byte)(value >> 8);
            }
        }

        public int lineX
        {
            get => ((sbyte)(ram[ADDR_LINE_X + 1]) << 8) | ram[ADDR_LINE_X];
            set
            {
                ram[ADDR_LINE_X] = (byte)(value & 0xff);
                ram[ADDR_LINE_X + 1] = (byte)(value >> 8);
            }
        }

        public int lineY
        {
            get => ((sbyte)(ram[ADDR_LINE_Y + 1]) << 8) | ram[ADDR_LINE_Y];
            set
            {
                ram[ADDR_LINE_Y] = (byte)(value & 0xff);
                ram[ADDR_LINE_Y + 1] = (byte)(value >> 8);
            }
        }

        public byte clipX0 { get { return (byte)(ram[ADDR_CLIP_X0] & 0x7f); } set { ram[ADDR_CLIP_X0] = value; } }
        public byte clipY0 { get { return (byte)(ram[ADDR_CLIP_Y0] & 0x7f); } set { ram[ADDR_CLIP_Y0] = value; } }
        public byte clipX1 { get { return (byte)(ram[ADDR_CLIP_X1] & 0x7f); } set { ram[ADDR_CLIP_X1] = value; } }
        public byte clipY1 { get { return (byte)(ram[ADDR_CLIP_Y1] & 0x7f); } set { ram[ADDR_CLIP_Y1] = value; } }

        public int screenX
        {
            get
            {
                byte i = Peek(0x5F2C);
                switch(i)
                {
                    case 0:
                    case 4:
                        return 128;
                }

                return 128;
            }
        }

        public int screenY
        {
            get
            {
                byte i = Peek(0x5F2C);
                switch (i)
                {
                    case 0:
                    case 4:
                        return 128;
                }

                return 128;
            }
        }

        public int fillPattern
        {
            get => (ram[ADDR_FILLP + 1] << 8) | ram [ADDR_FILLP];
            set
            {
                ram[ADDR_FILLP] = (byte)(value & 0xff);
                ram[ADDR_FILLP + 1] = (byte)(value >> 8 & 0xff);
            }
        }

        public bool fillpTransparent
        {
            get => ram[ADDR_FILLP + 2] != 0;
            set
            {
                ram[ADDR_FILLP + 2] = (byte)(value ? 1 : 0);
            }
        }

        public MemoryUnit()
        {
            ram = new byte[ADDR_END];
            screen = new byte[0x2000];

            Init_ramState();
        }

        public void LoadCartridgeData(byte[] cartridgeRom)
        {
            Buffer.BlockCopy(cartridgeRom, 0x0, ram, 0, 0x4300);
        }

        private void Init_ramState()
        {
            ram[ADDR_PALETTE_0] = 0x10;
            ram[ADDR_PALETTE_1] = 0x0;

            for (int i = 1; i < 16; i++)
            {
                ram[ADDR_PALETTE_0 + i] = (byte)i;
                ram[ADDR_PALETTE_1 + i] = (byte)i;
            }

            ram[ADDR_CLIP_X0] = 0;
            ram[ADDR_CLIP_Y0] = 0;
            ram[ADDR_CLIP_X1] = 127;
            ram[ADDR_CLIP_Y1] = 127;
        }

        #region TODO

        public object Reload(int dest_addr, int source_addr, int len, string filename = "") { return null; }
        public object Cstore(int dest_addr, int source_addr, int len, string filename = "") { return null; }
        public object Cartdata(object id) { return null;  }
        public double Dget(int index) { return 0; }
        public object Dset(int index, double value) { return null; }

        #endregion

        public object Fillp(double? p = null)
        {
            if (!p.HasValue)
            {
                p = 0;
            }

            fillPattern = (int)p.Value;
            fillpTransparent = Math.Floor(p.Value) < p.Value;

            return null;
        }

        public int getFillPBit(int x, int y)
        {
            x %= 4;
            y %= 4;
            int i = y * 4 + x;
            int mask = (1 << 15) >> i;
            return (fillPattern & mask) >> (15 - i);
        }

        public object Memset(int dest_addr, byte val, int len)
        {
            for (int i = 0; i < len; i++)
            {
                ram[dest_addr + i] = val;
            }

            return null;
        }

        public object Memcpy(int dest_addr, int source_addr, int len)
        {
            Buffer.BlockCopy(ram, source_addr, ram, dest_addr, len);

            return null;
        }

        public object Memcpy(int dest_addr, int source_addr, int len, byte[] source)
        {
            Buffer.BlockCopy(source, source_addr, ram, dest_addr, len);

            return null;
        }

        public object Color(byte col)
        {
            this.DrawColor = col;

            return null;
        }

        public byte GetDrawColor(int color)
        {
            if (color < 0 || color > 15) return 0;
            return ram[ADDR_PALETTE_0 + color];
        }

        public int GetScreenColor(int color)
        {
            if (color < 0 || color > 15) return 0;
            return ram[ADDR_PALETTE_1 + color];
        }

        public void SetTransparent(int col)
        {
            if (col >= 0 && col <= 15)
            {
                ram[ADDR_PALETTE_0 + col] &= 0x0f;
                ram[ADDR_PALETTE_0 + col] |= 0x10;
            }
        }

        public void ResetTransparent(int col)
        {
            if (col >= 0 && col <= 15)
                ram[ADDR_PALETTE_0 + col] &= 0x0f;
        }

        public void SetDrawPalette(int c0, int c1)
        {
            if (c0 >= 0 && c0 <= 15 && c1 >=0 && c1 <= 15)
                ram[ADDR_PALETTE_0 + c0] = (byte)c1;
        }

        public void SetScreenPalette(int c0, int c1)
        {
            if (c0 >= 0 && c0 <= 15 && c1 >= 0 && c1 <= 15)
                ram[ADDR_PALETTE_1 + c0] = (byte)c1;
        }

        public object Camera(int? x = null, int? y = null)
        {
            if (!x.HasValue && !y.HasValue)
            {
                cameraX = 0;
                cameraY = 0;
                return null;
            }

            if (x.HasValue)
            {
                cameraX = x.Value;
            }

            if (!y.HasValue)
            {
                y = 0;
            }

            cameraY = y.Value;

            return null;
        }

        public object Cls()
        {
            for (int i = 0; i < 0x2000; i++)
            {
                ram[ADDR_SCREEN + i] = 0;
            }

            return null;
        }

        public byte Peek(int addr)
        {
            // TODO throw BAD MEMORY ACCESS exception
            if (addr < 0 || addr >= 0x8000) return 0;
            return ram[addr];
        }

        public object Poke(int addr, byte val)
        {
            // TODO throw BAD MEMORY ACCESS exception
            if (addr < 0 || addr >= 0x8000) return null;

            ram[addr] = val;

            return null;
        }

        public int Peek2(int addr)
        {
            // TODO throw BAD MEMORY ACCESS exception
            if (addr < 0 || addr >= 0x8000 - 1) return 0;
            return ram[addr] | (ram[addr + 1] << 8);
        }

        public object Poke2(int addr, int val)
        {
            // TODO throw BAD MEMORY ACCESS exception
            if (addr < 0 || addr >= 0x8000 - 1) return null;

            ram[addr] = (byte)(val & 0xff);
            ram[addr + 1] = (byte)((val >> 8) & 0xff);

            return null;
        }

        public double Peek4(int addr)
        {
            // TODO throw BAD MEMORY ACCESS exception
            if (addr < 0 || addr >= 0x8000 - 3) return 0;
            int left = ram[addr] | (ram[addr + 1] << 8);
            int right = ((ram[addr + 2] << 16) | (ram[addr + 3] << 24));

            return util.FixedToFloat(left + right);
        }

        public object Poke4(int addr, double val)
        {
            // TODO throw BAD MEMORY ACCESS exception
            if (addr < 0 || addr >= 0x8000 - 3) return null;

            Int32 f = util.FloatToFixed(val);

            ram[addr] = (byte)(f & 0xff);
            ram[addr + 1] = (byte)((f >> 8) & 0xff);
            ram[addr + 2] = (byte)((f >> 16) & 0xff);
            ram[addr + 3] = (byte)((f >> 24) & 0xff);

            return null;
        }

        public object Fget(int n, byte? f = null)
        {
            if (f.HasValue)
            {
                return (Peek(ADDR_GFX_PROPS + n) & (1 << f)) != 0;
            }

            return Peek(ADDR_GFX_PROPS + n);
        }

        public object Fset(int n, byte? f = null, bool? v = null)
        {
            if (!f.HasValue)
            {
                return null;
            }

            if (v.HasValue)
            {
                if (v.Value)
                {
                    Poke(ADDR_GFX_PROPS + n, (byte)(Peek(ADDR_GFX_PROPS + n) | (1<<f)));
                }
                else
                {
                    Poke(ADDR_GFX_PROPS + n, (byte)(Peek(ADDR_GFX_PROPS + n) & ~(1<<f)));
                }
            }
            else
            {
                Poke(ADDR_GFX_PROPS + n, (byte)(Peek(ADDR_GFX_PROPS + n) | f));
            }

            return null;
        }

        public byte Mget(int x, int y)
        {
            int addr = (y < 32 ? ADDR_MAP : ADDR_GFX_MAP);
            y = y % 32;
            int index = (y * 128 + x);

            if (index < 0 || index > 32 * 128 - 1)
            {
                return 0x0;
            }

            return ram[index + addr];
        }

        public object Mset(int x, int y, byte v)
        {
            int addr = (y < 32 ? ADDR_MAP : ADDR_GFX_MAP);
            y = y % 32;
            int index = (y * 128 + x);

            if (index < 0 || index > 32 * 128 - 1)
            {
                return null;
            }

            ram[index + addr] = v;

            return null;
        }

        #region Helper Functions

        public byte GetPixel(int x, int y, int offset = ADDR_SCREEN)
        {
            int index = (y * 128 + x) / 2;

            if (index < 0 || index > 64 * 128 - 1)
            {
                return 0x10;
            }

            return util.GetHalf(ram, index + offset, x % 2 == 0);
        }

        public void WritePixel(int x, int y, byte color, int offset = ADDR_SCREEN)
        {
            int index = (y * 128 + x) / 2;

            if (x < clipX0 || y < clipY0 || x > clipX1 || y > clipY1 || color > 15 || color < 0)
            {
                return;
            }

            util.SetHalf(ram, index + offset, color, x % 2 == 0);
        }
    }
#endregion
}
