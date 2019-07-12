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

        public static byte GetHalf(byte[] arr, int index, bool rightmost = true)
        {
            byte mask = (byte)(rightmost ? 0x0f : 0xf0);
            byte val = (byte)(arr[index] & mask);
            return (byte)(rightmost ? val : val >> 4);
        }

        public static void SetHalf(byte[] arr, int index, byte val, bool rightmost = true)
        {
            byte mask = (byte)(rightmost ? 0xf0 : 0x0f);
            val = (byte)(rightmost ? val & 0x0f : val << 4);
            arr[index] = (byte)((byte)(arr[index] & mask) | val);
        }

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
