namespace pico8_interpreter.Pico8
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the <see cref="util" /> class. Used to define various useful functions.
    /// </summary>
    public static class util
    {
        /// <summary>
        /// Defines memory sections of PICO-8 RAM.
        /// </summary>
        public static int   ADDR_GFX = 0x0,
                            ADDR_GFX_MAP = 0x1000,
                            ADDR_MAP = 0x2000,
                            ADDR_GFX_PROPS = 0x3000,
                            ADDR_SONG = 0x3100,
                            ADDR_SFX = 0x3200,
                            ADDR_USER = 0x4300,
                            ADDR_CART = 0x5e00,
                            ADDR_PALETTE_0 = 0x5f00,
                            ADDR_PALETTE_1 = 0x5f10,
                            ADDR_CLIP_X0 = 0x5f20,
                            ADDR_CLIP_Y0 = 0x5f21,
                            ADDR_CLIP_X1 = 0x5f22,
                            ADDR_CLIP_Y1 = 0x5f23,
                            ADDR_DRAW_COL = 0x5f25,
                            ADDR_CURSOR_X = 0x5f26,
                            ADDR_CURSOR_Y = 0x5f27,
                            ADDR_CAMERA_X = 0x5f28,
                            ADDR_CAMERA_Y = 0x5f2a,
                            ADDR_FILLP = 0x5f31,
                            ADDR_LINE_X = 0x5f3c,
                            ADDR_LINE_Y = 0x5f3e,
                            ADDR_SCREEN = 0x6000,
                            ADDR_END = 0x8000;

        /// <summary>
        /// Defines a 2^16 value;
        /// </summary>
        public static readonly int SHIFT_16 = 1 << 16;

        /// <summary>
        /// Returns half of a byte. Either rightmost 4 bits or leftmost.
        /// </summary>
        /// <param name="b">The byte to read from.<see cref="byte"/></param>
        /// <param name="rightmost">True if it should extract the rightmost 4 bits, false to extract leftmost 4 bits.</param>
        /// <returns>The final byte that was read<see cref="byte"/></returns>
        public static byte GetHalf(byte b, bool rightmost = true)
        {
            byte mask = (byte)(rightmost ? 0x0f : 0xf0);
            byte val = (byte)(b & mask);
            return (byte)(rightmost ? val : val >> 4);
        }

        /// <summary>
        /// Sets half of a byte, either rightmost of leftmost 4 bits.
        /// </summary>
        /// <param name="b">The byte to set</param>
        /// <param name="val">The value to set the byte to</param>
        /// <param name="rightmost">True if it should set the rightmost 4 bits, false to set leftmost 4 bits.<see cref="bool"/></param>
        public static void SetHalf(ref byte b, byte val, bool rightmost = true)
        {
            byte mask = (byte)(rightmost ? 0xf0 : 0x0f);
            val = (byte)(rightmost ? val & 0x0f : val << 4);
            b = (byte)((byte)(b & mask) | val);
        }

        /// <summary>
        /// Converts a floating point number to fixed point.
        /// </summary>
        /// <param name="x">The floating point value to turn to fixed point.</param>
        /// <returns>The fixed point number generated.</returns>
        public static Int32 FloatToFixed(double x)
        {
            return (Int32)(x * SHIFT_16);
        }

        /// <summary>
        /// Converts a fixed point number to a floating point.
        /// </summary>
        /// <param name="x">The fixed point number to convert.</param>
        /// <returns>The floating point number generated.</returns>
        public static double FixedToFloat(Int32 x)
        {
            return Math.Round((double)x / SHIFT_16, 4);
        }

        /// <summary>
        /// Swaps two numbers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs">First value to swap.<see cref="T"/></param>
        /// <param name="rhs">Second value to swap.<see cref="T"/></param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Converts PICO-8 code to plain lua code.
        /// </summary>
        /// <param name="picoCode">The PICO-8 code to convert.</param>
        /// <returns>The converted Lua code.</returns>
        public static string ProcessPico8Code(string picoCode)
        {
            // "if a != b" => "if a ~= b"
            picoCode = Regex.Replace(picoCode, @"!=", "~=");
            // Matches and replaces binary style numbers like "0b1010.101" to hex format.
            picoCode = Regex.Replace(picoCode, @"0b([0-1]+)(?:\.{0,1})([0-1]*){0,1}", ReplaceBinaryNumber, RegexOptions.Multiline);
            // Matches if statements with conditions sorrounded by parenthesis, followed by anything but
            // nothing, only whitespaces or 'then' statement. Example:
            // "if (a ~= b) a=b" => "if (a ~= b) then a=b end"
            picoCode = Regex.Replace(picoCode, @"[iI][fF]\s*(\((?!(?:.*then)).*\))((?!(?:\s*$)|(?:.*then)|(?:.*and)|(?:.*or)).+)$", ReplaceIfShorthand, RegexOptions.Multiline);
            // Matches <var> <op>= <exp> type expressions, like "a += b".
            picoCode = Regex.Replace(picoCode, @"([a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[.*\])?)\s*([+\-*\/%])=\s*(.*)$", ReplaceUnaryShorthand, RegexOptions.Multiline);

            Console.WriteLine(picoCode);
            return picoCode;
        }

        private static string ReplaceBinaryNumber(Match binaryMatch)
        {
            string integerPart = Convert.ToInt32(binaryMatch.Groups[1].ToString(), 2).ToString("X");
            string fracPart = binaryMatch.Groups[2].Success ? binaryMatch.Groups[2].ToString() : "0";

            return string.Format("0x{0}.{1}", integerPart, fracPart);
        }

        private static string ReplaceUnaryShorthand(Match unaryMatch)
        {
            // Replaces every possible "." with spaces after with only "." and then recursively calls
            // the same function looking for matches of the same unary shorthand.
            // This needs to be done before processing the shorthand because we might see another
            // shorthand in the expression area. For example, "a += b + foo(function(c) c += 10 end)", 
            // where we see another shorthand inside the expression on the right.
            string fixedExp = Regex.Replace(Regex.Replace(unaryMatch.Groups[3].ToString(), @"\.\s+", "."),
                                            @"([a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[.*\])?)\s*([+\-*\/%])=\s*(.*)$",
                                            ReplaceUnaryShorthand,
                                            RegexOptions.Multiline);


            var terms = Regex.Matches(fixedExp, @"(?:\-?[0-9.]+)|(?:\-?(?:0x)[0-9.A-Fa-f]+)|(?:\-?[a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[[^\]]\])*)");
            if (terms.Count <= 0) return unaryMatch.ToString();

            int currentChar = 0;
            int currentTermIndex = 0;
            bool expectTerm = true;

            while (currentChar < fixedExp.Length)
            {
                if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\s"))
                {
                    currentChar += 1;
                    continue;
                }

                if (currentTermIndex >= terms.Count)
                {
                    currentChar = fixedExp.Length;
                    break;
                }

                if (terms[currentTermIndex].Index > currentChar)
                {
                    if (currentChar < fixedExp.Length - 1)
                    {
                        var relationalOp = fixedExp.Substring(currentChar, 2);
                        if (Regex.IsMatch(relationalOp, @"(?:\<\=)|(?:\>\=)|(?:\~\=)|(?:\=\=)"))
                        {
                            currentChar += 2;
                            expectTerm = true;
                            continue;
                        }
                    }

                    if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"[\-\+\=\/\*\%\<\>\~]"))
                    {
                        currentChar += 1;
                        expectTerm = true;
                    }
                    else if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\(|\[|{"))
                    {
                        Stack<char> st = new Stack<char>();
                        st.Push(fixedExp[currentChar]);
                        while (st.Count > 0)
                        {
                            if (currentChar >= fixedExp.Length)
                            {
                                break;
                            }

                            if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\)|\]|}"))
                            {
                                st.Pop();
                            }
                            else if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\(|\[|{"))
                            {
                                st.Push(fixedExp[currentChar]);
                            }

                            currentChar += 1;
                        }

                        while (currentTermIndex < terms.Count && terms[currentTermIndex].Index <= currentChar)
                        {
                            currentTermIndex += 1;
                        }

                        expectTerm = false;
                    }
                }
                else
                {
                    if (terms[currentTermIndex].Value.StartsWith("-"))
                        expectTerm = true;

                    if (!expectTerm)
                    {
                        break;
                    }

                    expectTerm = false;
                    currentChar += terms[currentTermIndex].Length;
                    currentTermIndex += 1;
                }
            }

            string expression = fixedExp.Substring(0, currentChar);
            string rest = fixedExp.Substring(currentChar);

            return string.Format("{0} = {0} {1} ({2}) {3}", unaryMatch.Groups[1], unaryMatch.Groups[2], expression, rest);
        }

        private static string ReplaceIfShorthand(Match ifMatch)
        {
            return string.Format("if {0} then {1} end", ifMatch.Groups[1], ifMatch.Groups[2]);
        }

        public static float NoteToFrequency(float note)
        {
            return (float)(440.0 * Math.Pow(2, (note - 33) / 12.0f));
        }

        public static float Lerp(float a, float b, float t)
        {
            return (b - a) * t + a;
        }
    }
}
