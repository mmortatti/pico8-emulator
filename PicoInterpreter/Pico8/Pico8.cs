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
using System.Diagnostics;

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
        private SpriteBatch spriteBatch;

        private Random random;

        private DateTime timeStart;

        // Function callback to retrieve if button was pressed
        private List<Func<int, bool>> BtnPressedCallbacks;
        // First Players keys
        private int[] BtnLeft, BtnRight, BtnUp, BtnDown, BtnA, BtnB;
        private bool[] BtnLeftLast, BtnRightLast, BtnUpLast, BtnDownLast, BtnALast, BtnBLast;
        private bool[] BtnLeftCurrent, BtnRightCurrent, BtnUpCurrent, BtnDownCurrent, BtnACurrent, BtnBCurrent;

        // Struct to hold all information about a running pico8 game.
        public struct Game
        {
            public MemoryUnit memory;
            public GraphicsUnit graphics;
            public Cartridge cartridge;
            public ILuaInterpreter interpreter;
        }

        public Game loadedGame;

        public PicoInterpreter(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
                
            screenTexture = new Texture2D(spriteBatch.GraphicsDevice, 128, 128, false, SurfaceFormat.Color);
            random = new Random();

            // Initialie controller variables
            BtnPressedCallbacks = new List<Func<int, bool>>();
            BtnLeft = new int[8];
            BtnRight = new int[8];
            BtnUp = new int[8];
            BtnDown = new int[8];
            BtnA = new int[8];
            BtnB = new int[8];

            BtnLeftLast = new bool[8];
            BtnRightLast = new bool[8];
            BtnUpLast = new bool[8];
            BtnDownLast = new bool[8];
            BtnALast = new bool[8];
            BtnBLast = new bool[8];

            BtnLeftCurrent = new bool[8];
            BtnRightCurrent = new bool[8];
            BtnUpCurrent = new bool[8];
            BtnDownCurrent = new bool[8];
            BtnACurrent = new bool[8];
            BtnBCurrent = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                BtnLeft[i] = -1;
                BtnRight[i] = -1;
                BtnUp[i] = -1;
                BtnDown[i] = -1;
                BtnA[i] = -1;
                BtnB[i] = -1;
            }
        }

        private void InitAPI(ref ILuaInterpreter interpreter)
        {
            // Graphics
            interpreter.AddFunction("line", (Action<int, int, int?, int?, byte?>)loadedGame.graphics.Line);
            interpreter.AddFunction("rect", (Action<int, int, int, int, byte?>)loadedGame.graphics.Rect);
            interpreter.AddFunction("rectfill", (Action<int, int, int, int, byte?>)loadedGame.graphics.Rectfill);
            interpreter.AddFunction("circ", (Action<int, int, double, byte?>)loadedGame.graphics.Circ);
            interpreter.AddFunction("circfill", (Action<int, int, double, byte?>)loadedGame.graphics.CircFill);
            interpreter.AddFunction("pset", (Action<int, int, byte?>)loadedGame.graphics.Pset);
            interpreter.AddFunction("pget", (Func<int, int, byte>)loadedGame.graphics.Pget);
            interpreter.AddFunction("sset", (Action<int, int, byte?>)loadedGame.graphics.Sset);
            interpreter.AddFunction("sget", (Func<int, int, byte>)loadedGame.graphics.Sget);
            interpreter.AddFunction("palt", (Action<int?, bool?>)loadedGame.graphics.Palt);
            interpreter.AddFunction("pal", (Action<int?, int?, int>)loadedGame.graphics.Pal);
            interpreter.AddFunction("clip", (Action<int?, int?, int?, int?>)loadedGame.graphics.Clip);
            interpreter.AddFunction("spr", (Action<int, int, int, int?, int?, bool?, bool?>)loadedGame.graphics.Spr);
            interpreter.AddFunction("sspr", (Action<int, int, int, int, int, int, int?, int?, bool?, bool?>)loadedGame.graphics.Sspr);
            interpreter.AddFunction("map", (Action<int, int, int, int, int, int, byte?>)loadedGame.graphics.Map);
            interpreter.AddFunction("map", (Action<int, int, int, int, int, int, byte?>)loadedGame.graphics.Map);
            interpreter.AddFunction("map", (Action<int, int, int, int, int, int, byte?>)loadedGame.graphics.Map);
            interpreter.AddFunction("mget", (Func<int, int, byte>)loadedGame.memory.Mget);
            interpreter.AddFunction("mset", (Action<int, int, byte>)loadedGame.memory.Mset);
            interpreter.AddFunction("fillp", (Action<int?>)loadedGame.memory.Fillp);

            // Memory related
            interpreter.AddFunction("cls", (Action)loadedGame.memory.Cls);
            interpreter.AddFunction("peek", (Func<int, byte>)loadedGame.memory.Peek);
            interpreter.AddFunction("poke", (Action<int, byte>)loadedGame.memory.Poke);
            interpreter.AddFunction("peek2", (Func<int, int>)loadedGame.memory.Peek2);
            interpreter.AddFunction("poke2", (Action<int, int>)loadedGame.memory.Poke2);
            interpreter.AddFunction("peek4", (Func<int, double>)loadedGame.memory.Peek4);
            interpreter.AddFunction("poke4", (Action<int, double>)loadedGame.memory.Poke4);
            interpreter.AddFunction("fget", (Func<int, byte?, object>)loadedGame.memory.Fget);
            interpreter.AddFunction("fset", (Action<int, byte?, bool?>)loadedGame.memory.Fset);
            interpreter.AddFunction("camera", (Action<int?, int?>)loadedGame.memory.Camera);
            interpreter.AddFunction("memcpy", (Action<int, int, int>)loadedGame.memory.Memcpy);
            interpreter.AddFunction("memset", (Action<int, byte, int>)loadedGame.memory.Memset);
            interpreter.AddFunction("reload", (Action<int, int, int, string>)loadedGame.memory.Reload);
            interpreter.AddFunction("cstore", (Action<int, int, int, string>)loadedGame.memory.Cstore);
            interpreter.AddFunction("cartdata", (Action<object>)loadedGame.memory.Cartdata);
            interpreter.AddFunction("dget", (Func<int, double>)loadedGame.memory.Dget);
            interpreter.AddFunction("dset", (Action<int, double>)loadedGame.memory.Dset);
            interpreter.AddFunction("color", (Action<byte>)loadedGame.memory.Color);

            // Math
            interpreter.AddFunction("max", (Func<double, double, double>)Math.Max);
            interpreter.AddFunction("min", (Func<double, double, double>)Math.Min);
            interpreter.AddFunction("mid", (Func<double, double, double, double>)((x, y, z) => Math.Max(Math.Min(Math.Max(x, y), z), Math.Min(x, y))));
            interpreter.AddFunction("flr", (Func<double, double>)Math.Floor);
            interpreter.AddFunction("ceil", (Func<double, double>)Math.Ceiling);
            interpreter.AddFunction("cos", (Func<double, double>)(x => Math.Cos(2 * x * Math.PI)));
            interpreter.AddFunction("sin", (Func<double, double>)(x => -Math.Sin(2 * x * Math.PI)));
            interpreter.AddFunction("atan2", (Func<double, double, double>)((dx, dy) => 1 - Math.Atan2(dy, dx) / (2 * Math.PI)));
            interpreter.AddFunction("sqrt", (Func<double, double>)Math.Sqrt);
            interpreter.AddFunction("abs", (Func<double, double>)Math.Abs);
            interpreter.AddFunction("rnd", (Func<double, double>)(x => random.NextDouble() * x));
            interpreter.AddFunction("srand", (Action<int>)(x => random = new Random(x)));
            interpreter.AddFunction("band", (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) & util.FloatToFixed(y))));
            interpreter.AddFunction("bor", (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) | util.FloatToFixed(y))));
            interpreter.AddFunction("bxor", (Func<double, double, double>)((x, y) => util.FixedToFloat(util.FloatToFixed(x) ^ util.FloatToFixed(y))));
            interpreter.AddFunction("bnot", (Func<double, double>)(x => util.FixedToFloat(~util.FloatToFixed(x))));
            interpreter.AddFunction("shl", (Func<double, int, double>)((x, n) => util.FixedToFloat(util.FloatToFixed(x) << n)));
            interpreter.AddFunction("shr", (Func<double, int, double>)((x, n) => util.FixedToFloat(util.FloatToFixed(x) >> n)));
            interpreter.AddFunction("lshr", (Func<double, int, double>)((x, n) => util.FixedToFloat((int)((uint)util.FloatToFixed(x)) >> n))); // Does Not Work I think

            // Controls
            interpreter.AddFunction("btn", (Func<int?, int?, object>)Btn);
            interpreter.AddFunction("btnp", (Func<int, int?, bool>)Btnp);

            // Misc
            interpreter.AddFunction("time", (Func<int>)(() => (DateTime.Now - timeStart).Seconds));

            interpreter.AddFunction("print", (Action<string>)((s) => s.Substring(0)));//Console.WriteLine);

            interpreter.RunScript(@"
                function all(collection)
                   if (collection == nil) then return function() end end
                   local index = 0
                   local count = #collection
                   return function ()
                      index = index + 1
                      if index <= count
                      then
                         return collection[index]
                      end
                   end
                end

                function tostr(x)
                    if type(x) == ""number"" then return tostring(math.floor(x*10000)/10000) end
                    return tostring(x)
                end

                function tonum(x)
                    return tonumber(x)
                end

                function add(t,v)
                    if t == nil then return end
                    table.insert(t,v)
                end

                function del(t,v)
                    if t == nil then return end
                    for i = 0,#t do
                        if t[i] == v then
                            table.remove(t,i)
                            return
                        end
                    end
                end

                function foreach(t,f)
                    for e in all(t) do
                        f(e)
                    end
                end

                cocreate = coroutine.create
                coresume = coroutine.resume
                costatus = coroutine.status
                yield = coroutine.yield
                sub = string.sub
                ");
        }

        // Load a game from path and run it. 
        // All paths are considered to be inside pico8/games folder
        public void LoadGame(string path, ILuaInterpreter interpreter)
        {
            loadedGame = new Game();
            loadedGame.memory = new MemoryUnit();
            loadedGame.cartridge = new Cartridge(path);
            loadedGame.graphics = new GraphicsUnit(ref loadedGame.memory, ref screenTexture, ref spriteBatch);
            loadedGame.interpreter = interpreter;

            InitAPI(ref loadedGame.interpreter);

            loadedGame.memory.LoadCartridgeData(loadedGame.cartridge.rom);
            loadedGame.interpreter.RunScript(loadedGame.cartridge.gameCode);

            timeStart = DateTime.Now;

            loadedGame.interpreter.CallIfDefined("_init");
        }

        // Call scripts update method
        public void Update()
        {
            UpdateControllerState();
            loadedGame.interpreter.CallIfDefined("_update");
        }

        // Call scripts draw method
        public void Draw()
        {
            loadedGame.interpreter.CallIfDefined("_draw");
            loadedGame.graphics.Flip();
        }

        public void SetSpriteBatch(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }

        public object Btn(int? i, int? p)
        {
            if (!p.HasValue)
            {
                p = 0;
            }

            switch(i.Value)
            {
                case 0:
                    return BtnLeftCurrent[p.Value];
                case 1:
                    return BtnRightCurrent[p.Value];
                case 2:
                    return BtnUpCurrent[p.Value];
                case 3:
                    return BtnDownCurrent[p.Value];
                case 4:
                    return BtnACurrent[p.Value];
                case 5:
                    return BtnBCurrent[p.Value];
            }

            return false;
        }

        public bool Btnp(int i, int? p)
        {
            if (!p.HasValue)
            {
                p = 0;
            }

            int pi = p.Value;
            switch (i)
            {
                case 0:
                    return BtnLeftCurrent[pi] && !BtnLeftLast[pi];
                case 1:
                    return BtnRightCurrent[pi] && !BtnRightLast[pi];
                case 2:
                    return BtnUpCurrent[pi] && !BtnUpLast[pi];
                case 3:
                    return BtnDownCurrent[pi] && !BtnDownLast[pi];
                case 4:
                    return BtnACurrent[pi] && !BtnALast[pi];
                case 5:
                    return BtnBCurrent[pi] && !BtnBLast[pi];
            }

            return false;
        }

        private void UpdateControllerState()
        {
            foreach (var f in BtnPressedCallbacks)
            {
                for (int i = 0; i < 8; i++)
                {
                    BtnLeftLast[i] = BtnLeftCurrent[i];
                    BtnRightLast[i] = BtnRightCurrent[i];
                    BtnUpLast[i] = BtnUpCurrent[i];
                    BtnDownLast[i] = BtnDownCurrent[i];
                    BtnALast[i] = BtnACurrent[i];
                    BtnBLast[i] = BtnBCurrent[i];


                    BtnLeftCurrent[i] = f(BtnLeft[i]);
                    BtnRightCurrent[i] = f(BtnRight[i]);
                    BtnUpCurrent[i] = f(BtnUp[i]);
                    BtnDownCurrent[i] = f(BtnDown[i]);
                    BtnACurrent[i] = f(BtnA[i]);
                    BtnBCurrent[i] = f(BtnB[i]);
                }
            }
        }

        public void SetBtnPressedCallback(Func<int, bool> callback)
        {
            BtnPressedCallbacks.Add(callback);
        }

        public void SetControllerKeys(int index, int Left, int Right, int Up, int Down, int A, int B)
        {
            Trace.Assert(index >= 0 && index <= 7);

            BtnLeft[index] = Left;
            BtnRight[index] = Right;
            BtnUp[index] = Up;
            BtnDown[index] = Down;
            BtnA[index] = A;
            BtnB[index] = B;
        }
    }
}
