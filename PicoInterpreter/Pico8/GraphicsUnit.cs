using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace pico8_interpreter.Pico8
{
    public class GraphicsUnit
    {

        public Color[] pico8Palette = {
            // Black
            new Color(0, 0, 0),
            // Dark-blue
            new Color(29, 43, 83),
            // Dark-purple
            new Color(126, 37, 83),
            // Dark-green
            new Color(0, 135, 81),
            // Brown
            new Color(171, 82, 54),
            // Dark-gray
            new Color(95, 87, 79),
            // Light-gray
            new Color(194, 195, 199),
            // White
            new Color(255, 241, 232),
            // Red
            new Color(255, 0, 77),
            // Orange
            new Color(255, 163, 0),
            // Yellow
            new Color(255, 236, 39),
            // Green
            new Color(0, 228, 54),
            // Blue
            new Color(41, 173, 255),
            // Indigo
            new Color(131, 118, 156),
            // Pink
            new Color(255, 119, 168),
            // Peach
            new Color(255, 204, 170),
            // Transparent
            new Color(0, 0, 0, 0)};

        MemoryUnit memory;
        Texture2D screenTexture;
        SpriteBatch spriteBatch;

        public GraphicsUnit(ref MemoryUnit memory, ref Texture2D screenTexture, ref SpriteBatch spriteBatch)
        {
            this.memory = memory;
            this.screenTexture = screenTexture;
            this.spriteBatch = spriteBatch;

            memory.DrawColor = 6;
        }

        #region TODO


        #endregion

        public void Map(int cel_x, int cel_y, int sx, int sy, int cel_w, int cel_h, byte? layer)
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
        }

        // TODO - Write to texture byte values and decode them in shader.
        public void Flip()
        {
            byte[] frameBuffer = memory.FrameBuffer;
            Color[] screenColorValues = new Color[128 * 128];
            for (int i = 0; i < 64 * 128; i++)
            {
                byte val = frameBuffer[i];
                byte left = (byte)(val & 0x0f);
                byte right = (byte)(val >> 4);

                screenColorValues[i * 2] = pico8Palette[memory.GetScreenColor(left)];
                screenColorValues[i * 2 + 1] = pico8Palette[memory.GetScreenColor(right)];
            }

            screenTexture.SetData(screenColorValues);
            spriteBatch.Draw(screenTexture, new Rectangle(0, 0, 128, 128), Color.White);
        }

        public void Spr(int n, int x, int y, int? w, int? h, bool? flip_x, bool? flip_y)
        {
            if (n < 0 || n > 255)
            {
                return;
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
                    Pset(x + (flipX ? 8 * width - i : i), y + (flipY ? 8 * height - j : j), sprColor);
                }
            }
        }

        public void Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int? dw, int? dh, bool? flip_x, bool? flip_y)
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
                    Pset((flip_x.Value ? dx + dw.Value - ((int)screenX - dx) : (int)screenX), 
                         (flip_y.Value ? dy + dh.Value - ((int)screenY - dy) : (int)screenY), 
                         sprColor);

                    y += ratioY;
                    screenY += 1;
                }
                x += ratioX;
                screenX += 1;
            }
        }

        public byte Sget(int x, int y)
        {
            return memory.GetPixel(x, y, MemoryUnit.ADDR_GFX);
        }

        public void Sset(int x, int y, byte? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            memory.WritePixel(x, y, memory.DrawColor,MemoryUnit.ADDR_GFX);
        }

        public void Pset(int x, int y, byte? col)
        {
            x -= memory.cameraX;
            y -= memory.cameraY;
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            memory.WritePixel((int)x, (int)y, memory.GetDrawColor(memory.DrawColor));
        }

        public byte Pget(int x, int y)
        {
            return memory.GetPixel((int)x, (int)y);
        }

        public void Palt(int? col, bool? t)
        {
            if (!col.HasValue || !t.HasValue)
            {
                memory.SetTransparent(0);
                for (byte i = 1; i < 16; i++)
                {
                    memory.ResetTransparent(i);
                }
                return;
            }

            if (t.Value)
            {
                memory.SetTransparent(col.Value);
            }
            else
            {
                memory.ResetTransparent(col.Value);
            }
        }

        public void Pal(int? c0, int? c1, int p = 0)
        {
            if (!c0.HasValue || !c1.HasValue)
            {
                Palt(null, null);
                for (byte i = 0; i < 16; i++)
                {
                    memory.SetDrawPalette(i, i);
                    memory.SetScreenPalette(i, i);
                }

                return;
            }

            if (p == 0)
            {
                memory.SetDrawPalette(c0.Value, c1.Value);
            }
            else if (p == 1)
            {
                memory.SetScreenPalette(c0.Value, c1.Value);
            }
        }

        public void Clip(int? x, int? y, int? w, int? h)
        {
            if (!x.HasValue || !y.HasValue || !w.HasValue || !h.HasValue)
            {
                return;
            }

            memory.clipX0 = (byte)x.Value;
            memory.clipY0 = (byte)y.Value;
            memory.clipX1 = (byte)(x.Value + w.Value);
            memory.clipY1 = (byte)(y.Value + h.Value);
        }

        public void Rect(int x0, int y0, int x1, int y1, byte? col)
        {
            Line(x0, y0, x1, y0, col);
            Line(x0, y0, x0, y1, col);
            Line(x1, y1, x1, y0, col);
            Line(x1, y1, x0, y1, col);
        }
        public void Rectfill(int x0, int y0, int x1, int y1, byte? col)
        {
            if (y0 > y1)
            {
                util.Swap(ref y0, ref y1);
            }

            for (int y = y0; y < y1; y++)
            {
                Line(x0, y, x1, y, col);
            }
        }

        public void Line(int x0, int y0, int? x1, int? y1, byte? col)
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
        }

        public void Circ(int x, int y, double r, byte? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)Math.Ceiling(r), false);
        }

        public void CircFill(int x, int y, double r, byte? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)Math.Ceiling(r), true);
        }

        private void plot8(int x, int y, int offX, int offY, bool fill = false)
        {
            if (fill)
            {
                Line(-x + offX, y + offY, x + offX, y + offY, memory.DrawColor);
                Line(-x + offX, -y + offY, x + offX, -y + offY, memory.DrawColor);
                Line(-y + offX, x + offY, y + offX, x + offY, memory.DrawColor);
                Line(-y + offX, -x + offY, y + offX, -x + offY, memory.DrawColor);
            }
            else
            {
                Pset(x + offX, y + offY, memory.DrawColor);
                Pset(-x + offX, y + offY, memory.DrawColor);
                Pset(x + offX, -y + offY, memory.DrawColor);
                Pset(-x + offX, -y + offY, memory.DrawColor);
                Pset(y + offX, x + offY, memory.DrawColor);
                Pset(-y + offX, x + offY, memory.DrawColor);
                Pset(y + offX, -x + offY, memory.DrawColor);
                Pset(-y + offX, -x + offY, memory.DrawColor);
            }
        }

        private void DrawCircle(int posX, int posY, int radius, bool fill = false)
        {
            int rs2 = radius * radius * 4; /* this could be folded into ycs2 */
            int xs2 = 0;
            int ys2m1 = rs2 - 2 * radius + 1;
            int x = 0;
            int y = radius;
            int ycs2;
            plot8(x, y, posX, posY, fill);
            while (x <= y)
            {
                /* advance to the right */
                xs2 = xs2 + 8 * x + 4;
                ++x;
                /* calculate new Yc */
                ycs2 = rs2 - xs2;
                if (ycs2 < ys2m1)
                {
                    ys2m1 = ys2m1 - 8 * y + 4;
                    --y;
                }
                plot8(x, y, posX, posY, fill);
            }
        }
    }
}
