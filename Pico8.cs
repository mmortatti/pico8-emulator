namespace Pico8_Emulator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the <see cref="PicoInterpreter{G}" />
    /// </summary>
    /// <typeparam name="G"></typeparam>
    public class Pico8<G>
    {
        private const int MAX_PLAYERS = 8;

        /// <summary>
        /// Defines a random object to use in PICO-8 random functions.
        /// </summary>
        public Random random;

        /// <summary>
        /// Defines the time where the cartridge started running (at cartridge load time).
        /// </summary>
        private DateTime timeStart;


        /// <summary>
        /// Defines an array to hold the last frame's values for down buttons.
        /// </summary>
        private bool[,] BtnLast;

        /// <summary>
        /// Defines an array to hold the current frame's values for down buttons.
        /// </summary>
        private bool[,] BtnCurrent;

        private List<Func<bool>>[,] _isDownFunctions;

        /// <summary>
        /// Defines the memory unit
        /// </summary>
        public MemoryUnit memory;

        /// <summary>
        /// Defines the graphics unit
        /// </summary>
        public GraphicsUnit<G> graphics;

        /// <summary>
        /// Defines the audio unit.
        /// </summary>
        public AudioUnit audio;

        public G[] screenColorData
        {
            get
            {
                return graphics.screenColorData;
            }
            set
            {
                graphics.screenColorData = value;
            }
        }

        public Func<int, int, int, G> rgbToColor
        {
            get
            {
                return graphics.rgbToColor;
            }
            set
            {
                graphics.rgbToColor = value;

            }
        }

        /// <summary>
        /// Defines the <see cref="Game" />
        /// Struct to hold all information about a running pico8 game.
        /// </summary>
        public struct Game
        {
            /// <summary>
            /// Defines the cartridge
            /// </summary>
            public Cartridge cartridge;

            /// <summary>
            /// Defines the interpreter
            /// </summary>
            public ILuaInterpreter interpreter;

            /// <summary>
            /// Defines the path
            /// </summary>
            public string path;

            /// <summary>
            /// Defines the cartdata_id
            /// </summary>
            public string cartdata_id;

            /// <summary>
            /// Defines the cartdata
            /// </summary>
            public Int32[] cartdata;
        }

        /// <summary>
        /// Defines the currently loaded game.
        /// </summary>
        public Game loadedGame;

        /// <summary>
        /// Defines the cartdata folder path.
        /// </summary>
        public string cartdataPath = "cartdata/";

        #region flattened API
        public Func<int, int, int?, int?, byte?, object> Line;
        public Func<int, int, int, int, byte?, object> Rect;
        public Func<int, int, int, int, byte?, object> Rectfill;
        public Func<int, int, double, byte?, object> Circ;
        public Func<int, int, double, byte?, object> Circfill;
        public Func<int, int, byte?, object> Pset;
        public Func<int, int, byte> Pget;
        public Func<int, int, byte?, object> Sset;
        public Func<int, int, byte> Sget;
        public Func<int?, bool?, object> Palt;
        public Func<int?, int?, int, object> Pal;
        public Func<int?, int?, int?, int?, object> Clip;
        public Func<int, int, int, int?, int?, bool?, bool?, object> Spr;
        public Func<int, int, int, int, int, int, int?, int?, bool?, bool?, object> Sspr;
        public Func<int, int, int, int, int, int, byte?, object> Map;
        public Func<int, int, byte> Mget;
        public Func<int, int, byte, object> Mset;
        public Func<double?, object> Fillp;
        public Func<object> Cls;
        public Func<int, byte> Peek;
        public Func<int, byte, object> Poke;
        public Func<int, int> Peek2;
        public Func<int, int, object> Poke2;
        public Func<int, double> Peek4;
        public Func<int, double, object> Poke4;
        public Func<int, byte?, object> Fget;
        public Func<int, byte?, bool?, object> Fset;
        public Func<int?, int?, object> Camera;
        public Func<int, int, int, object> Memcpy;
        public Func<int, byte, int, object> Memset;
        public Func<byte, object> Color;
        public Func<double, double, double> Max;
        public Func<double, double, double> Min;
        public Func<double, double, double, double> Mid;
        public Func<double, double> Floor;
        public Func<double, double> Ceiling;
        public Func<double, double> Cos;
        public Func<double, double> Sin;
        public Func<double, double, double> Atan2;
        public Func<double, double> Sqrt;
        public Func<double, double> Abs;
        public Func<double, double, double> Band;
        public Func<double, double, double> Bor;
        public Func<double, double, double> Bxor;
        public Func<double, double> Bnot;
        public Func<double, int, double> Shl;
        public Func<double, int, double> Shr;
        public Func<double, int, double> Lshr;
        public Action Flip;
        public Func<int?, int?, int?, object> Music;
        public Func<int, int?, int?, int?, object> Sfx;
        public Action<object, int?, int?, byte?> Print;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PicoInterpreter{G}"/> class.
        /// </summary>
        /// <param name="screenColorData">The screen color data reference.</param>
        /// <param name="rgbToColor">Function to convert RBG calues to an arbitrary color value.</param>
        public Pico8()
        {
            random = new Random();
            memory = new MemoryUnit();
            graphics = new GraphicsUnit<G>(ref memory);
            audio = new AudioUnit(ref memory);

            BtnLast = new bool[6, 8];
            BtnCurrent = new bool[6, 8];
            _isDownFunctions = new List<Func<bool>>[6, 8];
            for (int i = 0; i < _isDownFunctions.GetLength(0); i += 1)
            {
                for(int j = 0; j < _isDownFunctions.GetLength(1); j += 1)
                {
                    _isDownFunctions[i, j] = new List<Func<bool>>();
                }
            }

            (new FileInfo("cartdata/")).Directory.Create();
        }

        /// <summary>
        /// Initializes the PICO-8 API.
        /// </summary>
        /// <param name="interpreter">The interpreter to use when adding the functions. <see cref="ILuaInterpreter"/></param>
        private void InitAPI(ref ILuaInterpreter interpreter)
        {

            //
            // First, flatten out the API so that outside C# calls don't need to do something like pico8.graphics.print()
            // to call functions.
            //

            Flip = graphics.Flip;

            // Music
            Music = audio.Music;
            Sfx = audio.Sfx;

            Print = graphics.Print;

            // Math
            Max = Math.Max;
            Min = Math.Min;
            Mid = ((x, y, z) => Math.Max(Math.Min(Math.Max(x, y), z), Math.Min(x, y)));
            Floor = Math.Floor;
            Ceiling = Math.Ceiling;
            Cos = (x => Math.Cos(2 * x * Math.PI));
            Sin = (x => -Math.Sin(2 * x * Math.PI));
            Atan2 = ((dx, dy) => 1 - Math.Atan2(dy, dx) / (2 * Math.PI));
            Sqrt = Math.Sqrt;
            Abs = Math.Abs;
            Band = (x, y) => util.FixedToFloat(util.FloatToFixed(x) & util.FloatToFixed(y));
            Bor = (x, y) => util.FixedToFloat(util.FloatToFixed(x) | util.FloatToFixed(y));
            Bxor = (x, y) => util.FixedToFloat(util.FloatToFixed(x) ^ util.FloatToFixed(y));
            Bnot = x => util.FixedToFloat(~util.FloatToFixed(x));
            Shl = (x, n) => util.FixedToFloat(util.FloatToFixed(x) << n);
            Shr = (x, n) => util.FixedToFloat(util.FloatToFixed(x) >> n);
            Lshr = (x, n) => util.FixedToFloat((int)((uint)util.FloatToFixed(x)) >> n); // Does Not Work I think

            // Graphics
            Line = graphics.Line;
            Rect = graphics.Rect;
            Rectfill = graphics.Rectfill;
            Circ = graphics.Circ;
            Circfill = graphics.CircFill;
            Pset = graphics.Pset;
            Pget = graphics.Pget;
            Sset = graphics.Sset;
            Sget = graphics.Sget;
            Palt = graphics.Palt;
            Pal = graphics.Pal;
            Clip = graphics.Clip;
            Spr = graphics.Spr;
            Sspr = graphics.Sspr;
            Map = graphics.Map;
            Mget = memory.Mget;
            Mset = memory.Mset;
            Fillp = memory.Fillp;

            // Memory related
            Cls = memory.Cls;
            Peek = memory.Peek;
            Poke = memory.Poke;
            Peek2 = memory.Peek2;
            Poke2 = memory.Poke2;
            Peek4 = memory.Peek4;
            Poke4 = memory.Poke4;
            Fget = memory.Fget;
            Fset = memory.Fset;
            Camera = memory.Camera;
            Memcpy = memory.Memcpy;
            Memset = memory.Memset;
            Color = memory.Color;

            //
            // Now, fill the lua API properly.
            //

            // Graphics
            interpreter.AddFunction("line", Line);
            interpreter.AddFunction("rect", Rect);
            interpreter.AddFunction("rectfill", Rectfill);
            interpreter.AddFunction("circ", Circ);
            interpreter.AddFunction("circfill", Circfill);
            interpreter.AddFunction("pset", Pset);
            interpreter.AddFunction("pget", Pget);
            interpreter.AddFunction("sset", Sset);
            interpreter.AddFunction("sget", Sget);
            interpreter.AddFunction("palt", Palt);
            interpreter.AddFunction("pal", Pal);
            interpreter.AddFunction("clip", Clip);
            interpreter.AddFunction("spr", Spr);
            interpreter.AddFunction("sspr", Sspr);
            interpreter.AddFunction("map", Map);
            interpreter.AddFunction("mget", Mget);
            interpreter.AddFunction("mset", Mset);
            interpreter.AddFunction("fillp", Fillp);

            // Memory related
            interpreter.AddFunction("cls", Cls);
            interpreter.AddFunction("peek", Peek);
            interpreter.AddFunction("poke", Poke);
            interpreter.AddFunction("peek2", Peek2);
            interpreter.AddFunction("poke2", Poke2);
            interpreter.AddFunction("peek4", Peek4);
            interpreter.AddFunction("poke4", Poke4);
            interpreter.AddFunction("fget", Fget);
            interpreter.AddFunction("fset", Fset);
            interpreter.AddFunction("camera", Camera);
            interpreter.AddFunction("memcpy", Memcpy);
            interpreter.AddFunction("memset", Memset);
            interpreter.AddFunction("reload", (Func<int, int, int, string, object>)Reload);
            interpreter.AddFunction("cstore", (Func<int, int, int, string, object>)Cstore);
            interpreter.AddFunction("cartdata", (Func<string, object>)Cartdata);
            interpreter.AddFunction("dget", (Func<int, object>)Dget);
            interpreter.AddFunction("dset", (Func<int, double, object>)Dset);
            interpreter.AddFunction("color", Color);

            // Math
            interpreter.AddFunction("max", Max);
            interpreter.AddFunction("min", Min);
            interpreter.AddFunction("mid", Mid);
            interpreter.AddFunction("flr", Floor);
            interpreter.AddFunction("ceil", Ceiling);
            interpreter.AddFunction("cos", Cos);
            interpreter.AddFunction("sin", Sin);
            interpreter.AddFunction("atan2", Atan2);
            interpreter.AddFunction("sqrt", Sqrt);
            interpreter.AddFunction("abs", Abs);
            interpreter.AddFunction("rnd", (Func<double?, double>)Rnd);
            interpreter.AddFunction("srand", (Func<int, object>)Srand);
            interpreter.AddFunction("band", Band);
            interpreter.AddFunction("bor", Bor);
            interpreter.AddFunction("bxor", Bxor);
            interpreter.AddFunction("bnot", Bnot);
            interpreter.AddFunction("shl", Shl);
            interpreter.AddFunction("shr", Shr);
            interpreter.AddFunction("lshr", Lshr); // Does Not Work I think

            // Controls
            interpreter.AddFunction("btn", (Func<int?, int?, object>)Btn);
            interpreter.AddFunction("btnp", (Func<int?, int?, object>)Btnp);

            interpreter.AddFunction("flip", Flip);

            // Music
            interpreter.AddFunction("music", Music);
            interpreter.AddFunction("sfx", Sfx);

            // Misc
            interpreter.AddFunction("time", (Func<double>)Time);

            interpreter.AddFunction("print", Print);
            interpreter.AddFunction("printh", (Func<object, object>)Printh);

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

        /// <summary>
        /// Print a string to the console,
        /// </summary>
        /// <param name="s">The string to print</param>
        public object Printh(object s)
        {
            Console.WriteLine(String.Format("{0:####.####}", s));
            return null;
        }

        /// <summary>
        /// cartdata() opens a permanent data storage slot indexed by id, that can be
        /// 	used to store and retrieve up to 256 bytes(64 numbers) worth of data using 
        /// 	DSET() and DGET().
        /// 	
        /// 	CARTDATA("zep_dark_forest") -- can only be set once per session
        /// 	-- later in the program..
        ///     DSET(0, score)
        /// 
        /// 
        /// id is a string up to 64 characters long, and should be unusual enough that
        /// 
        /// other cartridges do not accidentally use the same id.
        /// 
        /// e.g.cartdata("zep_jelpi")
        /// 
        /// 
        /// legal characters are a..z, 0..9 and underscore(_)
        /// 
        /// 
        /// returns true if data was loaded, otherwise false
        /// 
        /// cartdata can not be called more than once per cartridge execution.
        /// 
        /// Once a cartdata id has been set, the area of memory 0x5e00..0x5eff is mapped
        /// to permanent storage, and can either be accessed directly or via dget/dset.
        /// </summary>
        /// <param name="id">The id for the cartdata file</param>
        public object Cartdata(string id)
        {
            Trace.Assert(loadedGame.cartdata_id.Length == 0, "cartdata() can only be called once");
            Trace.Assert(id.Length <= 64, "cart data id too long");
            Trace.Assert(id.Length != 0, "empty cart data id");

            Trace.Assert(Regex.IsMatch(id, "^[a-zA-Z0-9_]*$"), "cart data id: bad char");

            var fileName = cartdataPath + id;
            if (File.Exists(fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    for (int i = 0; i < loadedGame.cartdata.Length; i++)
                    {
                        loadedGame.cartdata[i] = reader.ReadInt32();
                    }
                }
            }
            else
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    for (int i = 0; i < loadedGame.cartdata.Length; i++)
                    {
                        writer.Write(0);
                        loadedGame.cartdata[i] = 0;
                    }
                }
            }

            loadedGame.cartdata_id = id;

            return null;
        }

        /// <summary>
        /// Get the number stored at index (0..63)
		/// Use this only after you have called cartdata()
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The value</returns>
        public object Dget(int index)
        {
            Trace.Assert(index < loadedGame.cartdata.Length, "bad index");
            return util.FixedToFloat(loadedGame.cartdata[index]);
        }

        /// <summary>
        /// Set the number stored at index (0..63)
		/// Use this only after you have called cartdata()
        ///
        /// There is no need to flush written data -- it is automatically
        /// saved to permanent storage even if POKE()'ed directly.
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="value">The value</param>
        public object Dset(int index, double value)
        {
            Trace.Assert(index < loadedGame.cartdata.Length, "bad index");
            loadedGame.cartdata[index] = util.FloatToFixed(value);
            SaveCartdata(cartdataPath + loadedGame.cartdata_id);

            return null;
        }

        /// <summary>
        /// Saves cartdata information to file.
        /// </summary>
        /// <param name="fileName">The fileName to write to.</param>
        private void SaveCartdata(string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                for (int i = 0; i < loadedGame.cartdata.Length; i++)
                {
                    writer.Write(loadedGame.cartdata[i]);
                }
            }
        }

        /// <summary>
        /// Returns a random number n, where 0 <= n < x
		/// If you want an integer, use flr(rnd(x))
        /// </summary>
        /// <param name="x">The x</param>
        /// <returns>The </returns>
        public double Rnd(double? x = null)
        {
            if (!x.HasValue) x = 1; return random.NextDouble() * x.Value;
        }

        /// <summary>
        /// Changes random seed.
        /// </summary>
        /// <param name="x">The new seed</param>
        /// <returns>Returns null everytime.</returns>
        public object Srand(int x)
        {
            random = new Random(x);
            return null;
        }

        /// <summary>
        /// Returns time passed since start.
        /// </summary>
        /// <returns>Returns time passed since start.</returns>
        public double Time()
        {
            return (DateTime.Now - timeStart).TotalSeconds;
        }

        /// <summary>
        /// Same as memcpy, but copies from cart rom
		/// The code section( >= 0x4300) is protected and can not be read.
        /// If filename specified, load data from a different cartridge.
        /// </summary>
        /// <param name="dest_addr">The destination address to write to.</param>
        /// <param name="source_addr">The source address to read from.</param>
        /// <param name="len">The length of data to read/write.</param>
        /// <param name="filename">The path to a different cartridge.</param>
        /// <returns>Returns null everytime.</returns>
        public object Reload(int dest_addr, int source_addr, int len, string filename = "")
        {
            Trace.Assert(dest_addr < 0x4300);
            Cartridge cart = filename.Length == 0 ? loadedGame.cartridge : new Cartridge(filename);

            memory.Memcpy(dest_addr, source_addr, len, cart.rom);
            return null;
        }

        /// <summary>
        /// Same as memcpy, but copies from base ram to cart rom
        /// cstore() is equivalent to cstore(0, 0, 0x4300)
        /// Can use for writing tools to construct carts or to visualize the state
        /// of the map / spritesheet using the map editor / gfx editor.
        /// The code section ( >= 0x4300) is protected and can not be written to.
        /// </summary>
        /// <param name="dest_addr">The destination address to write to.</param>
        /// <param name="source_addr">The source address to read from.</param>
        /// <param name="len">The length of data to read/write.</param>
        /// <param name="filename">The path to a different cartridge.</param>
        /// <returns>Returns null everytime.</returns>
        public object Cstore(int dest_addr, int source_addr, int len, string filename = null)
        {
            Trace.Assert(dest_addr < 0x4300);
            Cartridge cart = filename == null ? loadedGame.cartridge : new Cartridge(filename, true);

            Buffer.BlockCopy(memory.ram, source_addr, cart.rom, dest_addr, len);
            cart.SaveP8();
            return null;
        }

        /// <summary>
        /// Load a game from path and run it. 
        /// All paths are considered to be inside pico8/games folder
        /// </summary>
        /// <param name="path">The path to read cart from.</param>
        /// <param name="interpreter">The interpreter to use.<see cref="ILuaInterpreter"/></param>
        public void LoadGame(string path, ILuaInterpreter interpreter)
        {
            // Verify if all conditions to run a game are met.
            if (graphics.screenColorData == null)
                throw new ArgumentNullException("Pico8 must have a reference to the screenColorData array to fill it with pico8's screen data.");
            if (graphics.screenColorData.Length != 128 * 128)
                throw new ArgumentException($"screenColorData array must be of length 16.384 (128 * 128), but is of length {graphics.screenColorData.Length}.");

            loadedGame = new Game();

            loadedGame.path = path;
            loadedGame.cartridge = new Cartridge(path);
            loadedGame.interpreter = interpreter;
            loadedGame.cartdata_id = "";
            loadedGame.cartdata = new Int32[64];

            InitAPI(ref loadedGame.interpreter);

            memory.LoadCartridgeData(loadedGame.cartridge.rom);

            audio.Init();

            loadedGame.interpreter.RunScript(loadedGame.cartridge.gameCode);

            timeStart = DateTime.Now;

            loadedGame.interpreter.CallIfDefined("_init");
        }

        /// <summary>
        /// Call scripts update method
        /// </summary>
        public void Update()
        {
            UpdateControllerState();
            if (!loadedGame.interpreter.CallIfDefined("_update"))
            {
                loadedGame.interpreter.CallIfDefined("_update60");
            }
        }

        /// <summary>
        /// Call scripts draw method
        /// </summary>
        public void Draw()
        {
            loadedGame.interpreter.CallIfDefined("_draw");
            graphics.Flip();
        }

        /// <summary>
        /// get button i state for player p (default 0) 
		/// i: 0..5: left right up down button_o button_x
        ///
        /// p: player index 0..7
        ///
        ///
        /// Instead of using a number for i, it is also possible to use a button glyph.
        ///
        /// (In the coded editor, use Shift-L R U D O X)
        ///
        /// If no parameters supplied, returns a bitfield of all 12 button states for player 0 & 1
        ///	 P0: bits 0..5  P1: bits 8..13
        /// </summary>
        /// <param name="i">The index of the button</param>
        /// <param name="p">The player index.</param>
        /// <returns>The button state value.</returns>
        public object Btn(int? i = null, int? p = null)
        {
            if (i == null)
            {
                int bitMask = 0;
                for (int k = 0; k < BtnCurrent.GetLength(0); k += 1)
                {
                    for (int j = 0; j < 2; j += 1)
                    {
                        bitMask |= ((BtnCurrent[k, j] ? 1 : 0) << (6 * j + k));
                    }
                }
                return bitMask;
            }

            if (!p.HasValue)
            {
                p = 0;
            }

            return BtnCurrent[i.GetValueOrDefault(), p.GetValueOrDefault()];
        }

        /// <summary>
        /// btnp is short for "Button Pressed"; Instead of being true when a button is held down, 
		/// btnp returns true when a button is down AND it was not down the last frame.It also
        /// repeats after 15 frames, returning true every 4 frames after that(at 30fps -- double
        /// that at 60fp). This can be used for things like menu navigation or grid-wise player
        /// movement.
        /// </summary>
        /// <param name="i">The index of the button</param>
        /// <param name="p">The player index.</param>
        /// <returns>The button state value.</returns>
        public object Btnp(int? i = null, int? p = null)
        {
            if (i == null)
            {
                int bitMask = 0;
                for (int k = 0; k < BtnCurrent.GetLength(0); k += 1)
                {
                    for (int j = 0; j < 2; j += 1)
                    {
                        bitMask |= ((BtnCurrent[k, j] && BtnLast[k, j] ? 1 : 0) << (6 * j + k));
                    }
                }
                return bitMask;
            }

            if (!p.HasValue)
            {
                p = 0;
            }

            return BtnCurrent[i.GetValueOrDefault(), p.GetValueOrDefault()] && BtnLast[i.GetValueOrDefault(), p.GetValueOrDefault()];
        }

        /// <summary>
        /// Updates all controller state stuff.
        /// </summary>
        private void UpdateControllerState()
        {
            for(int i = 0; i < 6; i += 1)
            {
                for (int j = 0; j < MAX_PLAYERS; j += 1)
                {
                    BtnLast[i, j] = BtnCurrent[i, j];
                    BtnCurrent[i, j] = false;
                    foreach (var f in _isDownFunctions[i, j])
                    {
                        BtnCurrent[i,j] |= f();
                    }
                }
            }
        }

        /// <summary>
        /// Adds a function to be called to get the current state of the Left button.
        /// </summary>
        /// <param name="func"> The function to call to get button info. </param>
        /// <param name="playerIndex"> The player that you want to assign the button to. </param>
        public void AddLeftButtonDownFunction(Func<bool> func, int playerIndex)
        {
            _isDownFunctions[0, playerIndex].Add(func);
        }

        /// <summary>
        /// Adds a function to be called to get the current state of the Right button.
        /// </summary>
        /// <param name="func"> The function to call to get button info. </param>
        /// <param name="playerIndex"> The player that you want to assign the button to. </param>
        public void AddRightButtonDownFunction(Func<bool> func, int playerIndex)
        {
            _isDownFunctions[1, playerIndex].Add(func);
        }

        /// <summary>
        /// Adds a function to be called to get the current state of the Up button.
        /// </summary>
        /// <param name="func"> The function to call to get button info. </param>
        /// <param name="playerIndex"> The player that you want to assign the button to. </param>
        public void AddUpButtonDownFunction(Func<bool> func, int playerIndex)
        {
            _isDownFunctions[2, playerIndex].Add(func);
        }

        /// <summary>
        /// Adds a function to be called to get the current state of the Down button.
        /// </summary>
        /// <param name="func"> The function to call to get button info. </param>
        /// <param name="playerIndex"> The player that you want to assign the button to. </param>
        public void AddDownButtonDownFunction(Func<bool> func, int playerIndex)
        {
            _isDownFunctions[3, playerIndex].Add(func);
        }

        /// <summary>
        /// Adds a function to be called to get the current state of the O button.
        /// </summary>
        /// <param name="func"> The function to call to get button info. </param>
        /// <param name="playerIndex"> The player that you want to assign the button to. </param>
        public void AddOButtonDownFunction(Func<bool> func, int playerIndex)
        {
            _isDownFunctions[4, playerIndex].Add(func);
        }

        /// <summary>
        /// Adds a function to be called to get the current state of the X button.
        /// </summary>
        /// <param name="func"> The function to call to get button info. </param>
        /// <param name="playerIndex"> The player that you want to assign the button to. </param>
        public void AddXButtonDownFunction(Func<bool> func, int playerIndex)
        {
            _isDownFunctions[5, playerIndex].Add(func);
        }
    }
}
