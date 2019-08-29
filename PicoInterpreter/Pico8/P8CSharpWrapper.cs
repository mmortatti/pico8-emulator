using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    public class P8CSharpWrapper<G, A>
    {
        public PicoInterpreter<G, A> pico8;

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
        public Func<int, int, int, string, object> Reload;
        public Func<int, int, int, string, object> Cstore;
        public Action<string> Cartdata;
        public Func<int, object> Dget;
        public Action<int, double> Dset;
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
        public Func<double?, double> Rnd;
        public Func<int, object> Srand;
        public Func<double, double, double> Band;
        public Func<double, double, double> Bor;
        public Func<double, double, double> Bxor;
        public Func<double, double> Bnot;
        public Func<double, int, double> Shl;
        public Func<double, int, double> Shr;
        public Func<double, int, double> Lshr;
        public Func<int?, int?, object> Btn;
        public Func<int, int?, bool> Btnp;
        public Action Flip;
        public Func<int?, int?, int?, object> Music;
        public Func<int, int?, int?, int?, object> Sfx;
        public Func<double> Time;
        public Action<object, int?, int?, byte?> Print;
        public Action<object> Printh;

        public P8CSharpWrapper(ref PicoInterpreter<G, A> _pico8)
        {
            pico8 = _pico8;

            // Controls
            Btn = pico8.Btn;
            Btnp = pico8.Btnp;

            Flip = pico8.graphics.Flip;

            // Music
            Music = pico8.audio.Music;
            Sfx = pico8.audio.Sfx;

            // Misc
            Time = pico8.Time;

            Print = pico8.graphics.Print;
            Printh = pico8.Printh;

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
            Rnd = pico8.Rnd;
            Srand = pico8.Srand;
            Band = (x, y) => util.FixedToFloat(util.FloatToFixed(x) & util.FloatToFixed(y));
            Bor = (x, y) => util.FixedToFloat(util.FloatToFixed(x) | util.FloatToFixed(y));
            Bxor = (x, y) => util.FixedToFloat(util.FloatToFixed(x) ^ util.FloatToFixed(y));
            Bnot = x => util.FixedToFloat(~util.FloatToFixed(x));
            Shl = (x, n) => util.FixedToFloat(util.FloatToFixed(x) << n);
            Shr = (x, n) => util.FixedToFloat(util.FloatToFixed(x) >> n);
            Lshr = (x, n) => util.FixedToFloat((int)((uint)util.FloatToFixed(x)) >> n); // Does Not Work I think

            // Graphics
            Line =  pico8.graphics.Line;
            Rect = pico8.graphics.Rect;
            Rectfill = pico8.graphics.Rectfill;
            Circ = pico8.graphics.Circ;
            Circfill = pico8.graphics.CircFill;
            Pset = pico8.graphics.Pset;
            Pget = pico8.graphics.Pget;
            Sset = pico8.graphics.Sset;
            Sget = pico8.graphics.Sget;
            Palt = pico8.graphics.Palt;
            Pal = pico8.graphics.Pal;
            Clip = pico8.graphics.Clip;
            Spr = pico8.graphics.Spr;
            Sspr = pico8.graphics.Sspr;
            Map = pico8.graphics.Map;
            Mget = pico8.memory.Mget;
            Mset = pico8.memory.Mset;
            Fillp = pico8.memory.Fillp;

            // Memory related
            Cls = pico8.memory.Cls;
            Peek = pico8.memory.Peek;
            Poke = pico8.memory.Poke;
            Peek2 = pico8.memory.Peek2;
            Poke2 = pico8.memory.Poke2;
            Peek4 = pico8.memory.Peek4;
            Poke4 = pico8.memory.Poke4;
            Fget = pico8.memory.Fget;
            Fset = pico8.memory.Fset;
            Camera = pico8.memory.Camera;
            Memcpy = pico8.memory.Memcpy;
            Memset = pico8.memory.Memset;
            Reload = pico8.Reload;
            Cstore = pico8.Cstore;
            Cartdata = pico8.Cartdata;
            Dget = pico8.Dget;
            Dset = pico8.Dset;
            Color = pico8.memory.Color;
        }
    }
}
