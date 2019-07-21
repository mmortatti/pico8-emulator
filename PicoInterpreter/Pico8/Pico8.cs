﻿using System;
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
            interpreter.AddFunction("line", (Action<int, int, int?, int?, byte?>)graphics.Line);
            interpreter.AddFunction("rect", (Action<int, int, int, int, byte?>)graphics.Rect);
            interpreter.AddFunction("rectfill", (Action<int, int, int, int, byte?>)graphics.Rectfill);
            interpreter.AddFunction("circ", (Action<int, int, double, byte?>)graphics.Circ);
            interpreter.AddFunction("circfill", (Action<int, int, double, byte?>)graphics.CircFill);
            interpreter.AddFunction("pset", (Action<int, int, byte?>)graphics.Pset);
            interpreter.AddFunction("pget", (Func<int, int, byte>)graphics.Pget);
            interpreter.AddFunction("sset", (Action<int, int, byte?>)graphics.Sset);
            interpreter.AddFunction("sget", (Func<int, int, byte>)graphics.Sget);
            interpreter.AddFunction("palt", (Action<int?, bool?>)graphics.Palt);
            interpreter.AddFunction("pal", (Action<int?, int?, int>)graphics.Pal);
            interpreter.AddFunction("clip", (Action<int?, int?, int?, int?>)graphics.Clip);
            interpreter.AddFunction("spr", (Action<int, int, int, int?, int?, bool?, bool?>)graphics.Spr);
            interpreter.AddFunction("sspr", (Action<int, int, int, int, int, int, int?, int?, bool?, bool?>)graphics.Sspr);
            interpreter.AddFunction("map", (Action<int, int, int, int, int, int, byte?>)graphics.Map);
            interpreter.AddFunction("map", (Action<int, int, int, int, int, int, byte?>)graphics.Map);
            interpreter.AddFunction("map", (Action<int, int, int, int, int, int, byte?>)graphics.Map);
            interpreter.AddFunction("mget", (Func<int, int, byte>)memory.Mget);
            interpreter.AddFunction("mset", (Action<int, int, byte>)memory.Mset);
            interpreter.AddFunction("fillp", (Action<double?>)memory.Fillp);

            // Memory related
            interpreter.AddFunction("cls", (Action)memory.Cls);
            interpreter.AddFunction("peek", (Func<int, byte>)memory.Peek);
            interpreter.AddFunction("poke", (Action<int, byte>)memory.Poke);
            interpreter.AddFunction("peek2", (Func<int, int>)memory.Peek2);
            interpreter.AddFunction("poke2", (Action<int, int>)memory.Poke2);
            interpreter.AddFunction("peek4", (Func<int, double>)memory.Peek4);
            interpreter.AddFunction("poke4", (Action<int, double>)memory.Poke4);
            interpreter.AddFunction("fget", (Func<int, byte?, object>)memory.Fget);
            interpreter.AddFunction("fset", (Action<int, byte?, bool?>)memory.Fset);
            interpreter.AddFunction("camera", (Action<int?, int?>)memory.Camera);
            interpreter.AddFunction("memcpy", (Action<int, int, int>)memory.Memcpy);
            interpreter.AddFunction("memset", (Action<int, byte, int>)memory.Memset);
            interpreter.AddFunction("reload", (Action<int, int, int, string>)memory.Reload);
            interpreter.AddFunction("cstore", (Action<int, int, int, string>)memory.Cstore);
            interpreter.AddFunction("cartdata", (Action<object>)memory.Cartdata);
            interpreter.AddFunction("dget", (Func<int, double>)memory.Dget);
            interpreter.AddFunction("dset", (Action<int, double>)memory.Dset);
            interpreter.AddFunction("color", (Action<byte>)memory.Color);

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
