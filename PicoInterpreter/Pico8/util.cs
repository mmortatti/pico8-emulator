using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
            return Math.Round((double)x / SHIFT_16, 4);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        #region PICO-8 Shorthand
        public static string ProcessPico8Code(string luaCode)
        {
            // "if a != b" => "if a ~= b"
            luaCode = Regex.Replace(luaCode, @"!=", "~=");
            // Matches if statements with conditions sorrounded by parenthesis, followed by anything but
            // nothing, only whitespaces or 'then' statement. Example:
            // "if (a ~= b) a=b" => "if (a ~= b) then a=b end"
            luaCode = Regex.Replace(luaCode, @"[iI][fF]\s*(\(.*\))((?!(?:\s*$)|(?:.*then)).+)$", ReplaceIfShorthand, RegexOptions.Multiline);
            // Matches <var> <op>= <exp> type expressions, like "a += b".
            luaCode = Regex.Replace(luaCode, @"([a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[.*\])?)\s*([+\-*\/%])=\s*(.*)$", ReplaceUnaryShorthand, RegexOptions.Multiline);

            return luaCode;
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


            var terms = Regex.Matches(fixedExp, @"(?:\-?(?:0x)?[0-9.]+)|(?:\-?[a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[[^\]]\])*)");
            if (terms.Count <= 0) return unaryMatch.ToString();

            int currentChar = 0;
            int currentTermIndex = 0;
            bool expectTerm = true;

            while(currentChar < fixedExp.Length)
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
                    else if (fixedExp[currentChar] == '(')
                    {
                        Stack<char> st = new Stack<char>();
                        st.Push('(');
                        while (st.Count > 0)
                        {
                            if (currentChar >= fixedExp.Length)
                            {
                                break;
                            }

                            if (fixedExp[currentChar] == ')')
                            {
                                st.Pop();
                            }
                            else if (fixedExp[currentChar] == '(')
                            {
                                st.Push(fixedExp[currentChar]);
                            }

                            currentChar += 1;
                        }

                        while(currentTermIndex < terms.Count && terms[currentTermIndex].Index <= currentChar)
                        {
                            currentTermIndex += 1;
                        }

                        expectTerm = false;
                    }
                } else
                {
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

        #endregion
    }
}
