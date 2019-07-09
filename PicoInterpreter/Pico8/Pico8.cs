using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MoonSharp.Interpreter;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace pico8_interpreter.Pico8
{
    public class PicoInterpreter
    {
        #region pico8_constants
        // Pico8 defines
        public const int WIDTH = 128;
        public const int HEIGHT = 128;
        
        #endregion

        public Texture2D screenTexture;

        private string gameCode = "";
        private SpriteBatch spriteBatch;

        private ILuaInterpreter interpreter;

        private MemoryUnit memory;
        private GraphicsUnit graphics;
        private Cartridge cartridge;

        private Random random;

        public PicoInterpreter(SpriteBatch spriteBatch, ILuaInterpreter interpreter)
        {
            this.spriteBatch = spriteBatch;
                
            screenTexture = new Texture2D(spriteBatch.GraphicsDevice, 128, 128, false, SurfaceFormat.Color);
            memory = new MemoryUnit();
            graphics = new GraphicsUnit(ref memory, ref screenTexture, ref spriteBatch);
            this.interpreter = interpreter;
            random = new Random();

            // Graphics
            interpreter.AddFunction("line", (Action<int, int, int?, int?, int?>)graphics.Line);
            interpreter.AddFunction("rect" , (Action<int, int, int, int, int?>)graphics.Rect);
            interpreter.AddFunction("rectfill", (Action<int, int, int, int, int?>)graphics.Rectfill);
            interpreter.AddFunction("circ", (Action<int, int, int, int?>)graphics.Circ);
            interpreter.AddFunction("circfill", (Action<int, int, int, int?>)graphics.CircFill);
            interpreter.AddFunction("pset", (Action<int, int, int?>)graphics.Pset);
            interpreter.AddFunction("pget", (Func<int, int, byte>)graphics.Pget);
            interpreter.AddFunction("sset", (Action<int, int, int?>)graphics.Sset);
            interpreter.AddFunction("sget", (Func<int, int, byte>)graphics.Sget);
            interpreter.AddFunction("palt", (Action<int, bool>)graphics.Palt);
            interpreter.AddFunction("pal", (Action<int, int, int>)graphics.Pal);
            interpreter.AddFunction("clip", (Action<int?, int?, int?, int?>)graphics.Clip);
            interpreter.AddFunction("spr", (Action<int, int, int, int?, int?, bool?, bool?>)graphics.Spr);

            // Memory related
            interpreter.AddFunction("cls", (Action)memory.Cls);
            interpreter.AddFunction("peek", (Func<int, byte>)memory.Peek);
            interpreter.AddFunction("poke", (Action<int, byte>)memory.Poke);
            interpreter.AddFunction("peek2", (Func<int, int>)memory.Peek2);
            interpreter.AddFunction("poke2", (Action<int, int>)memory.Poke2);
            interpreter.AddFunction("peek4", (Func<int, double>)memory.Peek4);
            interpreter.AddFunction("poke4", (Action<int, double>)memory.Poke4);
            interpreter.AddFunction("camera", (Action<int? , int?>)memory.Camera);

            // Math
            interpreter.AddFunction("max", (Func<double, double, double>)Math.Max);
            interpreter.AddFunction("min", (Func<double, double, double>)Math.Min);
            interpreter.AddFunction("min", (Func<double, double, double, double>)((x, y, z) => Math.Max(Math.Min(Math.Max(x, y), z), Math.Min(x, y))));
            interpreter.AddFunction("flr", (Func<double, double>)Math.Floor);
            interpreter.AddFunction("ceil", (Func<double, double>)Math.Ceiling);
            interpreter.AddFunction("cos", (Func<double, double>)(x => Math.Cos(2 * x * Math.PI)));
            interpreter.AddFunction("sin", (Func<double, double>)(x => -Math.Sin(2 * x * Math.PI)));
            interpreter.AddFunction("atan2", (Func<double, double, double>)((dx, dy) => 1 - Math.Atan2(dy, dx) / (2 * Math.PI)));
            interpreter.AddFunction("sqrt", (Func<double, double>)Math.Sqrt);
            interpreter.AddFunction("abs", (Func<double, double>)Math.Abs);
            interpreter.AddFunction("rnd", (Func<double, double>) (x => random.NextDouble() * x));
            interpreter.AddFunction("srand", (Action<int>)(x => random = new Random(x)));
            interpreter.AddFunction("band", (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) & util.FloatToFixed(y))));
            interpreter.AddFunction("bor", (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) | util.FloatToFixed(y))));
            interpreter.AddFunction("bxor", (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) ^ util.FloatToFixed(y))));
            interpreter.AddFunction("bnot", (Func<double, double>)(x => util.FixedToFloat(~util.FloatToFixed(x))));
            interpreter.AddFunction("shl", (Func<double, int, double>)((x, n) => util.FixedToFloat(util.FloatToFixed(x) << n)));
            interpreter.AddFunction("shr", (Func<double, int, double>)((x, n) => util.FixedToFloat(util.FloatToFixed(x) >> n)));
            interpreter.AddFunction("lshr", (Func<double, int, double>)((x, n) => util.FixedToFloat((int)((uint)util.FloatToFixed(x)) >> n))); // Does Not Work I think

            // Controls
            interpreter.AddFunction("btn", (Func<int, bool>)this.Btn);

            interpreter.AddFunction("print", (Action<string>)Console.WriteLine);

            interpreter.RunScript(@"
                function all(collection)

                   local index = 0
                   local count = #collection
	
                   -- The closure function is returned
	
                   return function ()
                      index = index + 1
		
                      if index <= count
                      then
                         -- return the current element of the iterator
                         return collection[index]
                      end
		
                   end
	
                end
                ");
        }

        // Load a game from path and run it. 
        // All paths are considered to be inside pico8/games folder
        public void LoadGameAndRun(string path)
        {
            cartridge = new Cartridge(path);
            memory.LoadCartridgeData(cartridge.rom);
            interpreter.RunScript(cartridge.gameCode);
        }

        // Call scripts update method
        public void Update()
        {
            interpreter.CallIfDefined("_update");
        }

        // Call scripts draw method
        public void Draw()
        {
            interpreter.CallIfDefined("_draw");
            graphics.Flip();
        }

        public void SetSpriteBatch(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        public bool Btn(int b)
        {
            switch(b)
            {
                case 0:
                    return Keyboard.GetState().IsKeyDown(Keys.Left);
                case 1:
                    return Keyboard.GetState().IsKeyDown(Keys.Right);
                case 2:
                    return Keyboard.GetState().IsKeyDown(Keys.Up);
                case 3:
                    return Keyboard.GetState().IsKeyDown(Keys.Down);
            }

            return false;
        }
    }
}
