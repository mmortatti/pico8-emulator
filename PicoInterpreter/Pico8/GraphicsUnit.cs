using System;

namespace pico8_interpreter.Pico8
{
    public class GraphicsUnit<G>
    {

        public int[,] pico8Palette = {
            // Black
            { 0, 0, 0 },
            // Dark-blue
            { 29, 43, 83 },
            // Dark-purple
            { 126, 37, 83 },
            // Dark-green
            { 0, 135, 81 },
            // Brown
            { 171, 82, 54 },
            // Dark-gray
            { 95, 87, 79 },
            // Light-gray
            { 194, 195, 199 },
            // White
            { 255, 241, 232 },
            // Red
            { 255, 0, 77 },
            // Orange
            { 255, 163, 0 },
            // Yellow
            { 255, 236, 39 },
            // Green
            { 0, 228, 54 },
            // Blue
            { 41, 173, 255 },
            // Indigo
            { 131, 118, 156 },
            // Pink
            { 255, 119, 168 },
            // Peach
            { 255, 204, 170 } };

        MemoryUnit memory;
        G[] screenColorData;
        Func<int, int, int, G> rgbToColor;

        public GraphicsUnit(ref MemoryUnit memory, ref G[] screenColorData, Func<int, int, int, G> rgbToColor)
        {
            this.memory = memory;
            this.screenColorData = screenColorData;
            this.rgbToColor = rgbToColor;

            memory.DrawColor = 6;
        }

        #region TODO


        #endregion

        public object Map(int cel_x, int cel_y, int sx, int sy, int cel_w, int cel_h, byte? layer = null)
        {
            for (int h = 0; h < cel_h; h++) 
            {
                for (int w = 0; w < cel_w; w++)
                {

                    int addr = (cel_y + h) < 32 ? MemoryUnit.ADDR_MAP : MemoryUnit.ADDR_GFX_MAP;
                    byte spr_index = memory.Peek(addr + (cel_y + h) % 32 * 128 + cel_x + w);
                    byte flags = (byte)memory.Fget(spr_index, null);

                    // Spr index 0 is reserved for empty tiles
                    if (spr_index == 0)
                    {
                        continue;
                    }

                    // IF layer has not been specified, draw regardless
                    if (!layer.HasValue || (flags & layer.Value) != 0)
                    {
                        Spr(spr_index, sx + 8 * w, sy + 8 * h, 1, 1, false, false);
                    }
                }
            }

            return null;
        }

        public void Flip()
        {
            byte[] frameBuffer = memory.FrameBuffer;
            for (int i = 0; i < 64 * 128; i++)
            {
                byte val = frameBuffer[i];
                byte left = (byte)(val & 0x0f);
                byte right = (byte)(val >> 4);

                int rl = pico8Palette[memory.GetScreenColor(left), 0],
                    gl = pico8Palette[memory.GetScreenColor(left), 1],
                    bl = pico8Palette[memory.GetScreenColor(left), 2];
                int rr = pico8Palette[memory.GetScreenColor(right), 0],
                    gr = pico8Palette[memory.GetScreenColor(right), 1],
                    br = pico8Palette[memory.GetScreenColor(right), 2];
                screenColorData[i * 2] = rgbToColor(rl, gl, bl);
                screenColorData[i * 2 + 1] = rgbToColor(rr, gr, br);
            }
        }

        public object Spr(int n, int x, int y, int? w = null, int? h = null, bool? flip_x = null, bool? flip_y = null)
        {
            if (n < 0 || n > 255)
            {
                return null;
            }

            int sprX = (n % 16) * 8, sprY = (n / 16) * 8;

            int width = 1, height = 1;
            if (w.HasValue)
            {
                width = w.Value;
            }
            if (h.HasValue)
            {
                height = h.Value;
            }

            bool flipX = false, flipY = false;
            if (flip_x.HasValue)
            {
                flipX = flip_x.Value;
            }
            if (flip_y.HasValue)
            {
                flipY = flip_y.Value;
            }

            for (int i = 0; i < 8 * width; i++)
            {
                for (int j = 0; j < 8 * height; j++)
                {
                    byte sprColor = Sget(i + sprX, j + sprY);
                    Psett(x + (flipX ? 8 * width - i : i), y + (flipY ? 8 * height - j : j), sprColor);
                }
            }

            return null;
        }

        public object Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int? dw = null, int? dh = null, bool? flip_x = null, bool? flip_y = null)
        {
            if (!dw.HasValue)
            {
                dw = sw;
            }
            if (!dh.HasValue)
            {
                dh = sh;
            }
            if (!flip_x.HasValue)
            {
                flip_x = false;
            }
            if (!flip_y.HasValue)
            {
                flip_y = false;
            }

            float ratioX = (float)sw / (float)dw.Value;
            float ratioY = (float)sh / (float)dh.Value;
            float x = sx;
            float y = sy;
            float screenX = dx;
            float screenY = dy;

            while(x < sx + sw && screenX < dx + dw)
            {
                y = sy;
                screenY = dy;
                while (y < sy + sh && screenY < dy + dh)
                {
                    byte sprColor = Sget((int)x, (int)y);
                    Psett((flip_x.Value ? dx + dw.Value - ((int)screenX - dx) : (int)screenX), 
                          (flip_y.Value ? dy + dh.Value - ((int)screenY - dy) : (int)screenY), 
                          sprColor);

                    y += ratioY;
                    screenY += 1;
                }
                x += ratioX;
                screenX += 1;
            }

            return null;
        }

        public byte Sget(int x, int y)
        {
            return memory.GetPixel(x, y, MemoryUnit.ADDR_GFX);
        }

        public object Sset(int x, int y, byte? col = null)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            memory.WritePixel(x, y, memory.DrawColor,MemoryUnit.ADDR_GFX);

            return null;
        }

        public object Pset(int x, int y, byte? col = null)
        {
            x -= memory.cameraX;
            y -= memory.cameraY;
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            int f = memory.getFillPBit(x, y);
            if (f == 0)
            {
                // Do not consider transparency bit for this operation.
                memory.WritePixel(x, y, (byte)(memory.GetDrawColor(memory.DrawColor & 0x0f) & 0x0f));
            }
            else if (!memory.fillpTransparent)
            {
                // Do not consider transparency bit for this operation.
                memory.WritePixel(x, y, (byte)(memory.GetDrawColor(memory.DrawColor >> 4) & 0x0f));
            }

            return null;
        }

        // Set pixel considering transparency value. Used for spr, sspr and map.
        public object Psett(int x, int y, byte? col = null)
        {
            x -= memory.cameraX;
            y -= memory.cameraY;
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            int f = memory.getFillPBit(x, y);
            if (f == 0)
            {
                memory.WritePixel(x, y, memory.GetDrawColor(memory.DrawColor & 0x0f));
            }
            else if (!memory.fillpTransparent)
            {
                memory.WritePixel(x, y, (memory.GetDrawColor(memory.DrawColor >> 4)));
            }

            return null;
        }

        public byte Pget(int x, int y)
        {
            return memory.GetPixel((int)x, (int)y);
        }

        public object Palt(int? col = null, bool? t = null)
        {
            if (!col.HasValue || !t.HasValue)
            {
                memory.SetTransparent(0);
                for (byte i = 1; i < 16; i++)
                {
                    memory.ResetTransparent(i);
                }
                return null;
            }

            if (t.Value)
            {
                memory.SetTransparent(col.Value);
            }
            else
            {
                memory.ResetTransparent(col.Value);
            }

            return null;
        }

        public object Pal(int? c0 = null, int? c1 = null, int p = 0)
        {
            if (!c0.HasValue || !c1.HasValue)
            {
                for (byte i = 0; i < 16; i++)
                {
                    memory.SetDrawPalette(i, i);
                    memory.SetScreenPalette(i, i);
                }
                Palt(null, null);

                return null;
            }

            if (p == 0)
            {
                memory.SetDrawPalette(c0.Value, c1.Value);
            }
            else if (p == 1)
            {
                memory.SetScreenPalette(c0.Value, c1.Value);
            }

            return null;
        }

        public object Clip(int? x = null, int? y = null, int? w = null, int? h = null)
        {
            if (!x.HasValue || !y.HasValue || !w.HasValue || !h.HasValue)
            {
                return null;
            }

            memory.clipX0 = (byte)x.Value;
            memory.clipY0 = (byte)y.Value;
            memory.clipX1 = (byte)(x.Value + w.Value);
            memory.clipY1 = (byte)(y.Value + h.Value);

            return null;
        }

        public object Rect(int x0, int y0, int x1, int y1, byte? col = null)
        {
            Line(x0, y0, x1, y0, col);
            Line(x0, y0, x0, y1, col);
            Line(x1, y1, x1, y0, col);
            Line(x1, y1, x0, y1, col);

            return null;
        }
        public object Rectfill(int x0, int y0, int x1, int y1, byte? col = null)
        {
            if (y0 > y1)
            {
                util.Swap(ref y0, ref y1);
            }

            for (int y = y0; y < y1; y++)
            {
                Line(x0, y, x1, y, col);
            }

            return null;
        }

        public object Line(int x0, int y0, int? x1 = null, int? y1 = null, byte? col = null)
        {
            if (x1.HasValue)
            {
                memory.lineX = x1.Value;
            }

            if (y1.HasValue)
            {
                memory.lineY = y1.Value;
            }

            int x0_screen = x0;
            int y0_screen = y0;
            int x1_screen = memory.lineX;
            int y1_screen = memory.lineY;

            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }
            
            bool steep = false;
            if (Math.Abs(x1_screen - x0_screen) < Math.Abs(y1_screen - y0_screen))
            {
                util.Swap(ref x0_screen, ref y0_screen);
                util.Swap(ref x1_screen, ref y1_screen);
                steep = true;
            }
            if (x0_screen > x1_screen)
            {
                util.Swap(ref x0_screen, ref x1_screen);
                util.Swap(ref y0_screen, ref y1_screen);
            }

            int dx = (int)(x1_screen - x0_screen);
            int dy = (int)(y1_screen - y0_screen);
            int d_err = 2 * Math.Abs(dy);
            int err = 0;
            int y = (int)y0_screen;

            for (int x = (int)x0_screen; x <= x1_screen; x++)
            {
                if (steep)
                {
                    Pset(y, x, null);
                }
                else
                {
                    Pset(x, y, null);
                }

                err += d_err;

                if (err > dx)
                {
                    y += y1_screen > y0_screen ? 1 : -1;
                    err -= dx * 2;
                }
            }

            return null;
        }

        public object Circ(int x, int y, double r, byte? col = null)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)Math.Ceiling(r), false);

            return null;
        }

        public object CircFill(int x, int y, double r, byte? col = null)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)r, true);

            return null;
        }

        private void plot4(int x, int y, int offX, int offY, bool fill)
        {
            if (fill)
            {
                Line((x - offX), (y + offY), (x + offX), (y + offY), null);
                if (offY != 0)
                {
                    Line((x - offX), (y - offY), (x + offX), (y - offY), null);
                }
            }
            else
            {
                Pset((x - offX), (y + offY), null);
                Pset((x + offX), (y + offY), null);
                if (offY != 0)
                {
                    Pset((x - offX), (y - offY), null);
                    Pset((x + offX), (y - offY), null);
                }
            }
        }

        private void DrawCircle(int posX, int posY, int r, bool fill)
        {
            int cx = posX, cy = posY;
            int x = r;
            int y = 0;
            double err = 1 - r;

            while (y <= x)
            {

                plot4(posX, posY, x, y, fill);

                if (err < 0)
                {
                    err = err + 2 * y + 3;
                }
                else
                {
                    if (x != y)
                    {
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
