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
            new Color(255, 204, 170),
            // Transparent
            new Color(0, 0, 0, 0)};
        
        #endregion

        #region pico8_state

        private int col = 6;

        #endregion

        public Texture2D screenTexture;

        private string gameCode = "";
        private SpriteBatch spriteBatch;

        private Script gameScript;

        private MemoryUnit memory;
        private GraphicsUnit graphics;

        private Random random;

        public PicoInterpreter(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
                
            screenTexture = new Texture2D(spriteBatch.GraphicsDevice, 128, 128, false, SurfaceFormat.Color);
            memory = new MemoryUnit();
            graphics = new GraphicsUnit(ref memory);
            gameScript = new Script();
            random = new Random();

            // Init global functions
            gameScript.Globals["line"] = (Action<float, float, float, float, int?>)graphics.Line;
            gameScript.Globals["circ"] = (Action<float, float, float, int?>)graphics.Circ;
            gameScript.Globals["circfill"] = (Action<float, float, float, int?>)graphics.CircFill;
            gameScript.Globals["pset"] = (Action<float, float, int?>)graphics.Pset;
            gameScript.Globals["pget"] = (Func<float, float, byte>)graphics.Pget;

            // Memory related
            gameScript.Globals["cls"] = (Action)memory.Cls;
            gameScript.Globals["peek"] = (Func<int, byte>)memory.Peek;
            gameScript.Globals["poke"] = (Action<int, byte>)memory.Poke;
            gameScript.Globals["peek2"] = (Func<int, int>)memory.Peek2;
            gameScript.Globals["poke2"] = (Action<int, int>)memory.Poke2;
            gameScript.Globals["peek4"] = (Func<int, double>)memory.Peek4;
            gameScript.Globals["poke4"] = (Action<int, double>)memory.Poke4;

            // Math
            gameScript.Globals["max"] = (Func<double, double, double>)Math.Max;
            gameScript.Globals["min"] = (Func<double, double, double>)Math.Min;
            gameScript.Globals["min"] = (Func<double, double, double, double>)((x, y, z) => Math.Max(Math.Min(Math.Max(x, y), z), Math.Min(x, y)));
            gameScript.Globals["flr"] = (Func<double, double>)Math.Floor;
            gameScript.Globals["ceil"] = (Func<double, double>)Math.Ceiling;
            gameScript.Globals["cos"] = (Func<double, double>)(x => Math.Cos(2 * x * Math.PI));
            gameScript.Globals["sin"] = (Func<double, double>)(x => -Math.Sin(2 * x * Math.PI));
            gameScript.Globals["atan2"] = (Func<double, double, double>)((dx, dy) => 1 - Math.Atan2(dy, dx) / (2 * Math.PI));
            gameScript.Globals["sqrt"] = (Func<double, double>)Math.Sqrt;
            gameScript.Globals["abs"] = (Func<double, double>)Math.Abs;
            gameScript.Globals["rnd"] = (Func<double, double>) (x => random.NextDouble() * x);
            gameScript.Globals["srand"] = (Action<int>)(x => random = new Random(x));
            gameScript.Globals["band"] = (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) & util.FloatToFixed(y)));
            gameScript.Globals["bor"] = (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) | util.FloatToFixed(y)));
            gameScript.Globals["bxor"] = (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) ^ util.FloatToFixed(y)));
            gameScript.Globals["bnot"] = (Func<double, double>)(x => util.FixedToFloat(~util.FloatToFixed(x)));
            gameScript.Globals["shl"] = (Func<double, int, double>)((x, n) => util.FixedToFloat(util.FloatToFixed(x) << n));
            gameScript.Globals["shr"] = (Func<double, int, double>)((x, n) => util.FixedToFloat(util.FloatToFixed(x) >> n));
            gameScript.Globals["lshr"] = (Func<double, int, double>)((x, n) => util.FixedToFloat((int)((uint)util.FloatToFixed(x)) >> n)); // Does Not Work I think

            gameScript.Globals["print"] = (Action<string>)Console.WriteLine;

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
    }
}
