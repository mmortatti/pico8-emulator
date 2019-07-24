using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace pico8_interpreter.Pico8
{
    public class PicoInterpreter
    {
        private Random random;

        private DateTime timeStart;

        // Function callback to retrieve if button was pressed
        private List<Func<int, bool>> BtnPressedCallbacks;
        // First Players keys
        private int[] BtnLeft, BtnRight, BtnUp, BtnDown, BtnA, BtnB;
        private bool[] BtnLeftLast, BtnRightLast, BtnUpLast, BtnDownLast, BtnALast, BtnBLast;
        private bool[] BtnLeftCurrent, BtnRightCurrent, BtnUpCurrent, BtnDownCurrent, BtnACurrent, BtnBCurrent;

        public MemoryUnit memory;
        public GraphicsUnit graphics;

        // Struct to hold all information about a running pico8 game.
        public struct Game
        {
            public Cartridge cartridge;
            public ILuaInterpreter interpreter;
        }

        public Game loadedGame;

        public PicoInterpreter()
        {       
            random = new Random();
            memory = new MemoryUnit();
            graphics = new GraphicsUnit(ref memory);

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
            interpreter.AddFunction("line", (Func<int, int, int?, int?, byte?, object>)graphics.Line);
            interpreter.AddFunction("rect", (Func<int, int, int, int, byte?, object>)graphics.Rect);
            interpreter.AddFunction("rectfill", (Func<int, int, int, int, byte?, object>)graphics.Rectfill);
            interpreter.AddFunction("circ", (Func<int, int, double, byte?, object>)graphics.Circ);
            interpreter.AddFunction("circfill", (Func<int, int, double, byte?, object>)graphics.CircFill);
            interpreter.AddFunction("pset", (Func<int, int, byte?, object>)graphics.Pset);
            interpreter.AddFunction("pget", (Func<int, int, byte>)graphics.Pget);
            interpreter.AddFunction("sset", (Func<int, int, byte?, object>)graphics.Sset);
            interpreter.AddFunction("sget", (Func<int, int, byte>)graphics.Sget);
            interpreter.AddFunction("palt", (Func<int?, bool?, object>)graphics.Palt);
            interpreter.AddFunction("pal", (Func<int?, int?, int, object>)graphics.Pal);
            interpreter.AddFunction("clip", (Func<int?, int?, int?, int?, object>)graphics.Clip);
            interpreter.AddFunction("spr", (Func<int, int, int, int?, int?, bool?, bool?, object>)graphics.Spr);
            interpreter.AddFunction("sspr", (Func<int, int, int, int, int, int, int?, int?, bool?, bool?, object>)graphics.Sspr);
            interpreter.AddFunction("map", (Func<int, int, int, int, int, int, byte?, object>)graphics.Map);
            interpreter.AddFunction("map", (Func<int, int, int, int, int, int, byte?, object>)graphics.Map);
            interpreter.AddFunction("map", (Func<int, int, int, int, int, int, byte?, object>)graphics.Map);
            interpreter.AddFunction("mget", (Func<int, int, byte>)memory.Mget);
            interpreter.AddFunction("mset", (Func<int, int, byte, object>)memory.Mset);
            interpreter.AddFunction("fillp", (Func<double?, object>)memory.Fillp);

            // Memory related
            interpreter.AddFunction("cls", (Func<object>)memory.Cls);
            interpreter.AddFunction("peek", (Func<int, byte>)memory.Peek);
            interpreter.AddFunction("poke", (Func<int, byte, object>)memory.Poke);
            interpreter.AddFunction("peek2", (Func<int, int>)memory.Peek2);
            interpreter.AddFunction("poke2", (Func<int, int, object>)memory.Poke2);
            interpreter.AddFunction("peek4", (Func<int, double>)memory.Peek4);
            interpreter.AddFunction("poke4", (Func<int, double, object>)memory.Poke4);
            interpreter.AddFunction("fget", (Func<int, byte?, object>)memory.Fget);
            interpreter.AddFunction("fset", (Func<int, byte?, bool?, object>)memory.Fset);
            interpreter.AddFunction("camera", (Func<int?, int?, object>)memory.Camera);
            interpreter.AddFunction("memcpy", (Func<int, int, int, object>)memory.Memcpy);
            interpreter.AddFunction("memset", (Func<int, byte, int, object>)memory.Memset);
            interpreter.AddFunction("reload", (Func<int, int, int, string, object>)memory.Reload);
            interpreter.AddFunction("cstore", (Func<int, int, int, string, object>)memory.Cstore);
            interpreter.AddFunction("cartdata", (Func<object, object>)memory.Cartdata);
            interpreter.AddFunction("dget", (Func<int, double>)memory.Dget);
            interpreter.AddFunction("dset", (Func<int, double, object>)memory.Dset);
            interpreter.AddFunction("color", (Func<byte, object>)memory.Color);

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
            interpreter.AddFunction("rnd", (Func<double?, double>)Rnd);
            interpreter.AddFunction("srand", (Func<int, object>)(x => random = new Random(x)));
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

            // Music
            interpreter.AddFunction("music", (Func<int?, int?, int?, object>)Music);
            interpreter.AddFunction("sfx", (Func<int?, int?, int?, int?, object>)Sfx);

            // Misc
            interpreter.AddFunction("time", (Func<int>)(() => (DateTime.Now - timeStart).Seconds));

            interpreter.AddFunction("print", (Action<object, int?, int?, int?>)((s, x, y, c) => Console.WriteLine(s)));

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

                function string:split(sep)
                   local sep, fields = sep or "":"", {}
                   local pattern = string.format(""([^%s]+)"", sep)
                   self: gsub(pattern, function(c) fields[#fields+1] = c end)
                   return fields
                end

                function b(s)
                    local o = 0
                    local n = s:split('.')
                    for d in n[1]:gmatch('[01]') do
                        o = o*2 + tonumber(d)
                    end
                    if n[2] then
                        div = 2
                        for d in n[2]:gmatch('[01]') do
                            o = o + tonumber(d) / div
                            div = div * 2
                        end
                    end

                    return o
                end

                t = type
                function type(f)
                    if not f then
                        return 'nil'
                    end
                    return t(f)
                end

                cocreate = coroutine.create
                coresume = coroutine.resume
                costatus = coroutine.status
                yield = coroutine.yield
                sub = string.sub
                ");
        }

        public object Music(int? n = null, int? fade_len = null, int? channel_mask = null) { return null; }
        public object Sfx(int? n = null, int? channel = null, int? offset = null, int? length = null) { return null; }
        public double Rnd(double? x = null) { if (!x.HasValue) x = 1; return random.NextDouble() * x.Value; }

        // Load a game from path and run it. 
        // All paths are considered to be inside pico8/games folder
        public void LoadGame(string path, ILuaInterpreter interpreter)
        {
            loadedGame = new Game();
            
            loadedGame.cartridge = new Cartridge(path);
            loadedGame.interpreter = interpreter;

            InitAPI(ref loadedGame.interpreter);

            memory.LoadCartridgeData(loadedGame.cartridge.rom);
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
        }

        public object Btn(int? i = null, int? p = null)
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

        public bool Btnp(int i, int? p = null)
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
