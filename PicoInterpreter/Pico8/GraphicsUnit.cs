using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace pico8_interpreter.Pico8
{
    class GraphicsUnit
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

        public void Pset(float x, float y, int? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            //spriteBatch.DrawPoint(x, y, pico8Palette[this.col]);
            memory.WritePixel((int)x, (int)y, memory.GetDrawColor(memory.DrawColor));
        }

        public byte Pget(float x, float y)
        {
            return memory.GetPixel((int)x, (int)y);
        }

        public void Rect(float x0, float y0, float x1, float y1, int? col)
        {
            Line(x0, y0, x1, y0, col);
            Line(x0, y0, x0, y1, col);
            Line(x1, y1, x1, y0, col);
            Line(x1, y1, x0, y1, col);
        }
        public void Rectfill(float x0, float y0, float x1, float y1, int? col)
        {
            if (y0 > y1)
            {
                util.Swap(ref y0, ref y1);
            }

            for (float y = y0; y < y1; y++)
            {
                Line(x0, y, x1, y, col);
            }
        }

        public void Line(float x0, float y0, float x1, float y1, int? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }
            
            bool steep = false;
            if (Math.Abs(x1 - x0) < Math.Abs(y1 - y0))
            {
                util.Swap(ref x0, ref y0);
                util.Swap(ref x1, ref y1);
                steep = true;
            }
            if (x0 > x1)
            {
                util.Swap(ref x0, ref x1);
                util.Swap(ref y0, ref y1);
            }

            int dx = (int)(x1 - x0);
            int dy = (int)(y1 - y0);
            int d_err = 2 * Math.Abs(dy);
            int err = 0;
            int y = (int)y0;

            for (int x = (int)x0; x <= x1; x++)
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
                    y += y1 > y0 ? 1 : -1;
                    err -= dx * 2;
                }
            }
        }

        public void Circ(float x, float y, float r, int? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)r, false);
        }

        public void CircFill(float x, float y, float r, int? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)r, true);
        }

        private void plot8(float x, float y, float offX, float offY, bool fill = false)
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

        private void DrawCircle(float posX, float posY, int radius, bool fill = false)
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
