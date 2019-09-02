namespace Pico8_Emulator
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines PICO-8 <see cref="MemoryUnit" />
    /// </summary>
    public class MemoryUnit
    {
        /// <summary>
        /// Defines memory sections of PICO-8 RAM.
        /// </summary>
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

        /// <summary>
        /// Defines the screen buffer.
        /// </summary>
        private byte[] screen;

        /// <summary>
        /// Defines the ram block.
        /// </summary>
        public byte[] ram { get; private set; }

        /// <summary>
        /// Public accessor for the screen buffer.
        /// </summary>
        public byte[] FrameBuffer
        {
            get
            {
                Buffer.BlockCopy(ram, 0x6000, screen, 0, 0x2000);
                return screen;
            }
        }

        /// <summary>
        /// Gets or sets the DrawColor. Used to keep track of the current default color value to use for drawing.
        /// </summary>
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

        public int cursorX
        {
            get => ram[ADDR_CURSOR_X];
            set => ram[ADDR_CURSOR_X] = (byte)value;
        }

        public int cursorY
        {
            get => ram[ADDR_CURSOR_Y];
            set => ram[ADDR_CURSOR_Y] = (byte)value;
        }

        /// <summary>
        /// X position of the camera.
        /// </summary>
        public int cameraX
        {
            get => ((sbyte)(ram[ADDR_CAMERA_X + 1]) << 8) | ram[ADDR_CAMERA_X];
            set
            {
                ram[ADDR_CAMERA_X] = (byte)(value & 0xff);
                ram[ADDR_CAMERA_X + 1] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// Y position of the camera.
        /// </summary>
        public int cameraY
        {
            get => ((sbyte)(ram[ADDR_CAMERA_Y + 1]) << 8) | ram[ADDR_CAMERA_Y];
            set
            {
                ram[ADDR_CAMERA_Y] = (byte)(value & 0xff);
                ram[ADDR_CAMERA_Y + 1] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// Default X position for line drawing.
        /// </summary>
        public int lineX
        {
            get => ((sbyte)(ram[ADDR_LINE_X + 1]) << 8) | ram[ADDR_LINE_X];
            set
            {
                ram[ADDR_LINE_X] = (byte)(value & 0xff);
                ram[ADDR_LINE_X + 1] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// Default Y position for line drawing.
        /// </summary>
        public int lineY
        {
            get => ((sbyte)(ram[ADDR_LINE_Y + 1]) << 8) | ram[ADDR_LINE_Y];
            set
            {
                ram[ADDR_LINE_Y] = (byte)(value & 0xff);
                ram[ADDR_LINE_Y + 1] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// X0 position for the clipping rectangle.
        /// </summary>
        public byte clipX0
        {
            get { return (byte)(ram[ADDR_CLIP_X0] & 0x7f); }
            set { ram[ADDR_CLIP_X0] = value; }
        }

        /// <summary>
        /// Y0 position for the clipping rectangle.
        /// </summary>
        public byte clipY0
        {
            get { return (byte)(ram[ADDR_CLIP_Y0] & 0x7f); }
            set { ram[ADDR_CLIP_Y0] = value; }
        }

        /// <summary>
        /// X1 position for the clipping rectangle.
        /// </summary>
        public byte clipX1
        {
            get { return (byte)(ram[ADDR_CLIP_X1] & 0x7f); }
            set { ram[ADDR_CLIP_X1] = value; }
        }

        /// <summary>
        /// Y1 position for the clipping rectangle.
        /// </summary>
        public byte clipY1
        {
            get { return (byte)(ram[ADDR_CLIP_Y1] & 0x7f); }
            set { ram[ADDR_CLIP_Y1] = value; }
        }

        /// <summary>
        /// Gets the screenX
        /// </summary>
        public int screenX
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

        /// <summary>
        /// Gets the screenY
        /// </summary>
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

        /// <summary>
        /// Current fill pattern.
        /// </summary>
        public int fillPattern
        {
            get => (ram[ADDR_FILLP + 1] << 8) | ram[ADDR_FILLP];
            set
            {
                ram[ADDR_FILLP] = (byte)(value & 0xff);
                ram[ADDR_FILLP + 1] = (byte)(value >> 8 & 0xff);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the second fill pattern color is transparent or not.
        /// </summary>
        public bool fillpTransparent
        {
            get => ram[ADDR_FILLP + 2] != 0;
            set
            {
                ram[ADDR_FILLP + 2] = (byte)(value ? 1 : 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryUnit"/> class.
        /// </summary>
        public MemoryUnit()
        {
            ram = new byte[ADDR_END];
            screen = new byte[0x2000];

            Init_ramState();
        }

        /// <summary>
        /// Loads ROM contents to RAM.
        /// </summary>
        /// <param name="cartridgeRom">The cartridge ROM to copy from.</param>
        public void LoadCartridgeData(byte[] cartridgeRom)
        {
            Buffer.BlockCopy(cartridgeRom, 0x0, ram, 0, 0x4300);
        }

        /// <summary>
        /// Initializes RAM.
        /// </summary>
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

        /// <summary>
        /// The PICO-8 fill pattern is a 4x4 2-colour tiled pattern observed by:
        /// circ() circfill() rect() rectfill() pset() line()
        ///
        /// p is a bitfield in reading order starting from the highest bit.To calculate the value
        ///
        /// of p for a desired pattern, add the bit values together:
		///
		///	.-----------------------.
		///	|32768|16384| 8192| 4096|
		///	|-----|-----|-----|-----|
		///	| 2048| 1024| 512 | 256 |
		///	|-----|-----|-----|-----|
		///	| 128 |  64 |  32 |  16 |
		///	|-----|-----|-----|-----|
		///	|  8  |  4  |  2  |  1  |
		///	'-----------------------'
		///
		/// For example, FILLP(4 + 8 + 64 + 128 + 256 + 512 + 4096 + 8192) would create a checkerboard pattern.
        /// This can be more neatly expressed in binary: FILLP(0b0011001111001100)
        ///
        /// The default fill pattern is 0, which means a single solid colour is drawn.
        ///
        /// To specify a second colour for the pattern, use the high bits of any colour parameter:
		///
		///    FILLP(0b0011010101101000)
        ///    CIRCFILL(64,64,20, 0x4E) -- brown and pink
        ///
        /// An additional bit 0b0.1 can be set to indicate that the second colour is not drawn.
        ///
        ///    FILLP(0b0011001111001100.1) -- checkboard with transparent squares
        /// </summary>
        /// <param name="p">The fill pattern.</param>
        /// <returns>Returns null everytime.</returns>
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

        /// <summary>
        /// Gets the bit in a 4x4 grid defined by the fill pattern.
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        /// <returns>The bit at (x,y).</returns>
        public int getFillPBit(int x, int y)
        {
            x %= 4;
            y %= 4;
            int i = y * 4 + x;
            int mask = (1 << 15) >> i;
            return (fillPattern & mask) >> (15 - i);
        }

        /// <summary>
        /// Set len bytes to val.
        /// </summary>
        /// <param name="dest_addr">The address to write to.</param>
        /// <param name="val">The value to write</param>
        /// <param name="len">The length in bytes. Writes val to positions from dest_addr to dest_addr + len - 1.</param>
        /// <returns>Returns null everytime.</returns>
        public object Memset(int dest_addr, byte val, int len)
        {
            for (int i = 0; i < len; i++)
            {
                ram[dest_addr + i] = val;
            }

            return null;
        }

        /// <summary>
        /// Copy len bytes of base ram from source to dest.
		/// Sections can be overlapping.
        /// </summary>
        /// <param name="dest_addr">The address to write to.</param>
        /// <param name="source_addr">The address to read from.r</param>
        /// <param name="len">The length of the block to copy.</param>
        /// <returns>Returns null everytime.</returns>
        public object Memcpy(int dest_addr, int source_addr, int len)
        {
            Buffer.BlockCopy(ram, source_addr, ram, dest_addr, len);

            return null;
        }

        /// <summary>
        /// Copy len bytes from given source to RAM.
        /// </summary>
        /// <param name="dest_addr">The address to write to.</param>
        /// <param name="source_addr">The address to read from.r</param>
        /// <param name="len">The length of the block to copy.</param>
        /// <param name="source">The source array of bytes to read from.</param>
        /// <returns>Returns null everytime.</returns>
        public object Memcpy(int dest_addr, int source_addr, int len, byte[] source)
        {
            Buffer.BlockCopy(source, source_addr, ram, dest_addr, len);

            return null;
        }

        /// <summary>
        /// Set the cursor position and carriage return margin. If col is specified, also set the current colour.
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The Y position</param>
        /// <param name="col">The color to set default for</param>
        /// <returns>Returns null everytime</returns>
        public object Cursor(int x, int y, byte? col = null)
        {
            cursorX = x;
            cursorY = y;
            if (col.HasValue)
            {
                DrawColor = col.Value;
            }

            return null;
        }

        /// <summary>
        /// Set the current colour to be used by drawing functions
        /// </summary>
        /// <param name="col">The color to set to.</param>
        /// <returns>Returns null everytime.</returns>
        public object Color(byte col)
        {
            this.DrawColor = col;

            return null;
        }

        /// <summary>
        /// Gets the actual color that the color index is set to in the Draw palette (changed with pal()).
        /// </summary>
        /// <param name="color">The color to check.</param>
        /// <returns>The actual color index value for the given color.</returns>
        public byte GetDrawColor(int color)
        {
            if (color < 0 || color > 15) return 0;
            return ram[ADDR_PALETTE_0 + color];
        }

        /// <summary>
        /// Gets the actual color that the color index is set to in the Screen palette (changed with pal()).
        /// </summary>
        /// <param name="color">The color to check.</param>
        /// <returns>The actual color index value for the given color.</returns>
        public int GetScreenColor(int color)
        {
            if (color < 0 || color > 15) return 0;
            return ram[ADDR_PALETTE_1 + color];
        }

        /// <summary>
        /// Sets transparency value for a given color.
        /// </summary>
        /// <param name="col">The color value to set transparency of.</param>
        public void SetTransparent(int col)
        {
            if (col >= 0 && col <= 15)
            {
                ram[ADDR_PALETTE_0 + col] &= 0x0f;
                ram[ADDR_PALETTE_0 + col] |= 0x10;
            }
        }

        /// <summary>
        /// Returns whether or not that color was set to transparent.
        /// </summary>
        /// <param name="col"> The color to check. </param>
        /// <returns>If the color is set to transparent or not.</returns>
        public bool IsTransparent(int col)
        {
            return (ram[ADDR_PALETTE_0 + col] & 0x10) != 0;
        }

        /// <summary>
        /// Reset transparency for a given color.
        /// </summary>
        /// <param name="col">The color value to reset transparency of.</param>
        public void ResetTransparent(int col)
        {
            if (col >= 0 && col <= 15)
                ram[ADDR_PALETTE_0 + col] &= 0x0f;
        }

        /// <summary>
        /// Set draw palette for c0 to c1.
        /// </summary>
        /// <param name="c0">Color to change palette of.</param>
        /// <param name="c1">Color to change palette to.</param>
        public void SetDrawPalette(int c0, int c1)
        {
            if (c0 >= 0 && c0 <= 15 && c1 >= 0 && c1 <= 15)
                ram[ADDR_PALETTE_0 + c0] = (byte)c1;
        }

        /// <summary>
        /// Set screen palette for c0 to c1.
        /// </summary>
        /// <param name="c0">Color to change palette of.</param>
        /// <param name="c1">Color to change palette to.</param>
        public void SetScreenPalette(int c0, int c1)
        {
            if (c0 >= 0 && c0 <= 15 && c1 >= 0 && c1 <= 15)
                ram[ADDR_PALETTE_1 + c0] = (byte)c1;
        }

        /// <summary>
        /// Set a screen offset of -x, -y for all drawing operations
		/// camera() to reset
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>Returns null everytime.</returns>
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

        /// <summary>
        /// Clear the screen and reset the clipping rectangle
        /// </summary>
        /// <returns>Returns null everytime.</returns>
        public object Cls()
        {
            for (int i = 0; i < 0x2000; i++)
            {
                ram[ADDR_SCREEN + i] = 0;
            }

            return null;
        }

        /// <summary>
        /// Read one byte to an address in base ram.
		/// Legal addresses are 0x0..0x7fff
		/// Reading out of range returns 0
        /// </summary>
        /// <param name="addr">The address to read from.</param>
        /// <returns>The value at address position</returns>
        public byte Peek(int addr)
        {
            if (addr < 0 || addr >= 0x8000) return 0;
            return ram[addr];
        }

        /// <summary>
        /// Writes one byte to an address in base ram.
		/// Legal addresses are 0x0..0x7fff
		/// Writing out of range causes a fault
        /// </summary>
        /// <param name="addr">The addr</param>
        /// <param name="val">The val</param>
        /// <returns>The </returns>
        public object Poke(int addr, byte val)
        {
            Trace.Assert(addr >= 0 && addr < 0x8000, "bad memory access");

            ram[addr] = val;

            return null;
        }

        /// <summary>
        /// 16-bit version of Peek. Read one number (16-bit integer) in little-endian format:
		///	16 bit: 0xffff.0000
        /// addr does not need to be aligned to 2-byte boundaries.
        /// </summary>
        /// <param name="addr">The address to read from.</param>
        /// <returns>integer where the first 8 bits are ram[addr] and the next 8 bits are ram[addr + 1]</returns>
        public int Peek2(int addr)
        {
            if (addr < 0 || addr >= 0x8000 - 1) return 0;
            return ram[addr] | (ram[addr + 1] << 8);
        }

        /// <summary>
        /// 16-bit version of Poke. Write one number (val, 16-bit integer) in little-endian format:
		///	16 bit: 0xffff.0000
        /// addr does not need to be aligned to 2-byte boundaries.
        /// </summary>
        /// <param name="addr">The address to write to.</param>
        /// <param name="val">The 16-bit integer value to write</param>
        /// <returns>Returns null everytime.</returns>
        public object Poke2(int addr, int val)
        {
            Trace.Assert(addr >= 0 && addr < 0x8000, "bad memory access");

            ram[addr] = (byte)(val & 0xff);
            ram[addr + 1] = (byte)((val >> 8) & 0xff);

            return null;
        }

        /// <summary>
        /// 32-bit version of Peek. Read one number (32-bit fixed point) in little-endian format:
		///	32 bit: 0xffff.ffff
        /// addr does not need to be aligned to 4-byte boundaries.
        /// </summary>
        /// <param name="addr">The address to read from.</param>
        /// <returns>Fixed point value turned floating point created in the following bit format: 0x\<ram[addr+3]\>\<ram[addr+2]\>.\<ram[addr+1]\>\<ram[addr+0]\></ram></returns>
        public double Peek4(int addr)
        {
            if (addr < 0 || addr >= 0x8000 - 3) return 0;
            int right = ram[addr] | (ram[addr + 1] << 8);
            int left = ((ram[addr + 2] << 16) | (ram[addr + 3] << 24));

            return util.FixedToFloat(left + right);
        }

        /// <summary>
        /// 32-bit version of Poke. Write one number (val, 32-bit fixed point) in little-endian format:
		///	32 bit: 0xffff.ffff
        /// addr does not need to be aligned to 4-byte boundaries.
        /// </summary>
        /// <param name="addr">The address to write to.</param>
        /// <param name="val">The 32-bit integer value to write</param>
        /// <returns>Returns null everytime.</returns>
        public object Poke4(int addr, double val)
        {
            Trace.Assert(addr >= 0 && addr < 0x8000, "bad memory access");

            Int32 f = util.FloatToFixed(val);

            ram[addr] = (byte)(f & 0xff);
            ram[addr + 1] = (byte)((f >> 8) & 0xff);
            ram[addr + 2] = (byte)((f >> 16) & 0xff);
            ram[addr + 3] = (byte)((f >> 24) & 0xff);

            return null;
        }

        /// <summary>
        /// Get the value (v) of a sprite's flag
		/// f is the flag index 0..7
		/// v is boolean and can be true or false
        /// 
		/// The initial state of flags 0..7 are settable in the sprite editor,
		/// using the line of little colourful buttons.
        /// 
        /// The meaning of sprite flags is up to the user, or can be used to
        /// indicate which group ('layer') of sprites should be drawn by map.
        /// 
        /// If the flag index is omitted, all flags are retrieved/set as a bitfield
        /// fset(2, 1+2+8)   -- sets bits 0,1 and 3
        /// fset(2, 4, true) -- sets bit 4
        /// print(fget(2))   -- 27 (1+2+8+16)
        /// </summary>
        /// <param name="n">The sprite number</param>
        /// <param name="f">The flag index to get</param>
        /// <returns>The flag value</returns>
        public object Fget(int n, byte? f = null)
        {
            if (f.HasValue)
            {
                return (Peek(ADDR_GFX_PROPS + n) & (1 << f)) != 0;
            }

            return Peek(ADDR_GFX_PROPS + n);
        }

        /// <summary>
        /// Set the value (v) of a sprite's flag
		/// f is the flag index 0..7
		/// v is boolean and can be true or false
        /// 
		/// The initial state of flags 0..7 are settable in the sprite editor,
		/// using the line of little colourful buttons.
        /// 
        /// The meaning of sprite flags is up to the user, or can be used to
        /// indicate which group ('layer') of sprites should be drawn by map.
        /// 
        /// If the flag index is omitted, all flags are retrieved/set as a bitfield
        /// fset(2, 1+2+8)   -- sets bits 0,1 and 3
        /// fset(2, 4, true) -- sets bit 4
        /// print(fget(2))   -- 27 (1+2+8+16)
        /// </summary>
        /// <param name="n">The sprite number.</param>
        /// <param name="f">The flag index.</param>
        /// <param name="v">The value to set the flag to.</param>
        /// <returns>Returns null everytime.</returns>
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
                    Poke(ADDR_GFX_PROPS + n, (byte)(Peek(ADDR_GFX_PROPS + n) | (1 << f)));
                }
                else
                {
                    Poke(ADDR_GFX_PROPS + n, (byte)(Peek(ADDR_GFX_PROPS + n) & ~(1 << f)));
                }
            }
            else
            {
                Poke(ADDR_GFX_PROPS + n, (byte)(Peek(ADDR_GFX_PROPS + n) | f));
            }

            return null;
        }

        /// <summary>
        /// Get map value (v) at x,y
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        /// <returns>The map value at (x,y)</returns>
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

        /// <summary>
        /// Set map value (v) at x,y
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        /// <param name="v">The value to set map position to</param>
        /// <returns>Returns null everytime.</returns>
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

        /// <summary>
        /// Gets the pixel value at screen position (x,y).
        /// </summary>
        /// <param name="x">The x screen position</param>
        /// <param name="y">The y screen position</param>
        /// <param name="offset">The address offset to read from. You can use it to read from other ram sections (which is bad design because of the name, I know ...).</param>
        /// <returns>The color byte value.</returns>
        public byte GetPixel(int x, int y, int offset = ADDR_SCREEN)
        {
            int index = (y * 128 + x) / 2;

            if (index < 0 || index > 64 * 128 - 1)
            {
                return 0x10;
            }

            return util.GetHalf(ram[index + offset], x % 2 == 0);
        }

        /// <summary>
        /// Writes a color byte value at screen position (x,y)/
        /// </summary>
        /// <param name="x">The x screen position</param>
        /// <param name="y">The y screen position</param>
        /// <param name="color">The color byte to set screen position to</param>
        /// <param name="offset">The address offset to read from. You can use it to read from other ram sections (which is bad design because of the name, I know ...).</param>
        public void WritePixel(int x, int y, byte color, int offset = ADDR_SCREEN)
        {
            int index = (y * 128 + x) / 2;

            if (x < clipX0 || y < clipY0 || x > clipX1 || y > clipY1 || color > 15 || color < 0)
            {
                return;
            }

            util.SetHalf(ref ram[index + offset], color, x % 2 == 0);
        }
    }
}
