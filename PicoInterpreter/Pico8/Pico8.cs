using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
//using MonoGame.Extended;

namespace pico8_interpreter.Pico8
{
    public class PicoInterpreter
    {
        #region pico8_constants
        // Pico8 defines
        public const int WIDTH = 128;
        public const int HEIGHT = 128;

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
            new Color(255, 204, 170)};
        #endregion

        #region pico8_state

        private int col = 6;

        #endregion

        private string gameCode = "";
        private SpriteBatch spriteBatch;

        private Script gameScript;
        private MemoryUnit memory;

        public Texture2D screenTexture;

        public PicoInterpreter(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
                
            screenTexture = new Texture2D(spriteBatch.GraphicsDevice, 128, 128, false, SurfaceFormat.Color);
            memory = new MemoryUnit();
            gameScript = new Script();

            // Init global functions
            gameScript.Globals["line"] = (Action<float, float, float, float, int>)Line;
            gameScript.Globals["circ"] = (Action<float, float, float, int>)Circ;
            gameScript.Globals["circfill"] = (Action<float, float, float, int>)CircFill;
            gameScript.Globals["pset"] = (Action<float, float, int>)Pset;
            gameScript.Globals["cls"] = (Action)Cls;

            // Init default values
            this.col = 6;
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

                screenColorValues[i * 2] = pico8Palette[left];
                screenColorValues[i * 2 + 1] = pico8Palette[right];
            }

            screenTexture.SetData(screenColorValues);
            spriteBatch.Draw(screenTexture, new Rectangle(0, 0, 128, 128), Color.White);
        }

        // Load a game from path and run it. 
        // All paths are considered to be inside pico8/games folder
        public void LoadGameAndRun(string path)
        {
            string completePath = "Pico8/Games/" + path;
            var streamReader = new StreamReader(TitleContainer.OpenStream(completePath));

            // Read stream to string and run script
            gameCode = streamReader.ReadToEnd();
            gameScript.DoString(gameCode);
        }

        // Call scripts update method
        public void Update()
        {
            if (gameScript.Globals["_update"] != null)
                gameScript.Call(gameScript.Globals["_update"]);
        }

        // Call scripts draw method
        public void Draw()
        {
            if (gameScript.Globals["_draw"] != null)
                gameScript.Call(gameScript.Globals["_draw"]);
            Flip();
        }

        public void SetSpriteBatch(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        private void Pset(float x, float y, int col = -1)
        {
            if (col != -1)
            {
                this.col = col;
            }

            //spriteBatch.DrawPoint(x, y, pico8Palette[this.col]);
            memory.WritePixel((int)x, (int)y, (byte)col);
        }

        private void Cls()
        {
            memory.ClearFrameBuffer();
        }

        private void Line(float x0, float y0, float x1, float y1, int col = -1)
        {
            if (col != -1)
            {
                this.col = col;
            }

            //spriteBatch.DrawLine(x0, y0, x1, y1, pico8Palette[this.col]);
        }

        private void Circ(float x, float y, float r, int col = -1)
        {
            if (col != -1)
            {
                this.col = col;
            }

            DrawCircle(x, y, (int)r, false);
        }

        private void CircFill(float x, float y, float r, int col = -1)
        {
            if (col != -1)
            {
                this.col = col;
            }

            DrawCircle(x, y, (int)r, true);
        }

        void plot8(float x, float y, float offX, float offY, bool fill = false)
        {
            if (fill)
            {
                Line(-x + offX, y + offY, x + offX, y + offY, this.col);
                Line(-x + offX, -y + offY, x + offX, -y + offY, this.col);
                Line(-y + offX, x + offY, y + offX, x + offY, this.col);
                Line(-y + offX, -x + offY, y + offX, -x + offY, this.col);
            }
            else
            {   
                Pset(x + offX, y + offY, this.col);
                Pset(-x + offX, y + offY, this.col);
                Pset(x + offX, -y + offY, this.col);
                Pset(-x + offX, -y + offY, this.col);
                Pset(y + offX, x + offY, this.col);
                Pset(-y + offX, x + offY, this.col);
                Pset(y + offX, -x + offY, this.col);
                Pset(-y + offX, -x + offY, this.col);
            }
        }

        void DrawCircle(float posX, float posY, int radius, bool fill = false)
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
