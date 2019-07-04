using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    public static class util
    {
        public static readonly int SHIFT_16 = 1 << 16;

        public static Int32 FloatToFixed(double x)
        {
            return (Int32)(x * SHIFT_16);
        }

        public static double FixedToFloat(Int32 x)
        {
            return (double)x / SHIFT_16;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
