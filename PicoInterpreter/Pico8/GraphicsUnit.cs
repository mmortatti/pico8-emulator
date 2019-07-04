using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    class GraphicsUnit
    {

        MemoryUnit memory;

        public GraphicsUnit(ref MemoryUnit memory)
        {
            this.memory = memory;

            memory.DrawColor = 6;
        }

        public void Pset(float x, float y, int? col)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            //spriteBatch.DrawPoint(x, y, pico8Palette[this.col]);
            memory.WritePixel((int)x, (int)y, memory.GetColor(memory.DrawColor));
        }

        public byte Pget(float x, float y)
        {
            return memory.GetPixel((int)x, (int)y);
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
