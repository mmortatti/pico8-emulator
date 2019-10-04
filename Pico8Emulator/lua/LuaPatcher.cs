using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pico8Emulator.lua {
	public static class LuaPatcher {
		public static string PatchCode(string picoCode) {
			// "if a != b" => "if a ~= b"
			picoCode = Regex.Replace(picoCode, @"!=", "~=");
			// Matches and replaces binary style numbers like "0b1010.101" to hex format.
			picoCode = Regex.Replace(picoCode, @"0b([0-1]+)(?:\.{0,1})([0-1]*){0,1}", ReplaceBinaryNumber, RegexOptions.Multiline);
			// Matches if statements with conditions sorrounded by parenthesis, followed by anything but
			// nothing, only whitespaces or 'then' statement. Example:
			// "if (a ~= b) a=b" => "if (a ~= b) then a=b end"
			picoCode = Regex.Replace(picoCode, @"[iI][fF]\s*(\(.*)$", ReplaceIfShorthand, RegexOptions.Multiline);
			// Matches <var> <op>= <exp> type expressions, like "a += b".
			picoCode = Regex.Replace(picoCode, @"([a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[.*\])?)\s*([+\-*\/%])=\s*(.*)$", ReplaceUnaryShorthand, RegexOptions.Multiline);

			return picoCode;
		}

		private static string ReplaceBinaryNumber(Match binaryMatch) {
			string integerPart = Convert.ToInt32(binaryMatch.Groups[1].ToString(), 2).ToString("X");
			string fracPart = binaryMatch.Groups[2].Success ? binaryMatch.Groups[2].ToString() : "0";

			return string.Format("0x{0}.{1}", integerPart, fracPart);
		}

		private static string ReplaceUnaryShorthand(Match unaryMatch) {
			// Replaces every possible "." with spaces after with only "." and then recursively calls
			// the same function looking for matches of the same unary shorthand.
			// This needs to be done before processing the shorthand because we might see another
			// shorthand in the expression area. For example, "a += b + foo(function(c) c += 10 end)", 
			// where we see another shorthand inside the expression on the right.
			string fixedExp = Regex.Replace(Regex.Replace(unaryMatch.Groups[3].ToString(), @"\.\s+", "."),
				@"([a-zA-Z_](?:[a-zA-Z0-9_]|(?:\.\s*))*(?:\[.*\])?)\s*([+\-*\/%])=\s*(.*)$",
				ReplaceUnaryShorthand,
				RegexOptions.Multiline);


			var terms = Regex.Matches(fixedExp, @"(?:\-?[0-9.]+)|(?:\-?(?:0x)[0-9.A-Fa-f]+)|(?:\-?[a-zA-Z_\]\[](?:[a-zA-Z0-9_\[\]]|(?:\.\s*))*(?:\[[^\]]\])*)");
			if (terms.Count <= 0) return unaryMatch.ToString();

			int currentChar = 0;
			int currentTermIndex = 0;
			bool expectTerm = true;

			while (currentChar < fixedExp.Length) {
				if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\s")) {
					currentChar += 1;
					continue;
				}

				if (currentTermIndex >= terms.Count) {
					currentChar = fixedExp.Length;
					break;
				}

				if (terms[currentTermIndex].Index > currentChar) {
					if (currentChar < fixedExp.Length - 1) {
						var relationalOp = fixedExp.Substring(currentChar, 2);
						if (Regex.IsMatch(relationalOp, @"(?:\<\=)|(?:\>\=)|(?:\~\=)|(?:\=\=)")) {
							currentChar += 2;
							expectTerm = true;
							continue;
						}
					}

					if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"[\-\+\=\/\*\%\<\>\~]")) {
						currentChar += 1;
						expectTerm = true;
					} else if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\(|\[|{")) {
						var st = new Stack<char>();
						st.Push(fixedExp[currentChar]);
						currentChar += 1;
						while (st.Count > 0) {
							if (currentChar >= fixedExp.Length) {
								break;
							}

							if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\)|\]|}")) {
								st.Pop();
							} else if (Regex.IsMatch(fixedExp[currentChar].ToString(), @"\(|\[|{")) {
								st.Push(fixedExp[currentChar]);
							}

							currentChar += 1;
						}

						while (currentTermIndex < terms.Count && terms[currentTermIndex].Index < currentChar) {
							currentTermIndex += 1;
						}

						expectTerm = false;
					}
				} else {
					if (terms[currentTermIndex].Value.StartsWith("-")) expectTerm = true;

					if (!expectTerm) {
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

		private static string ReplaceIfShorthand(Match ifMatch) {
			string ifLine = ifMatch.Groups[1].ToString();

			// Remove the parenthesis from string.
			Stack<char> st = new Stack<char>();
			st.Push(ifLine[0]);
			int currentChar = 1;
			while (st.Count > 0) {
				if (currentChar >= ifLine.Length) {
					break;
				}

				if (Regex.IsMatch(ifLine[currentChar].ToString(), @"\)|\]|}")) {
					st.Pop();
				} else if (Regex.IsMatch(ifLine[currentChar].ToString(), @"\(|\[|{")) {
					st.Push(ifLine[currentChar]);
				}

				currentChar += 1;
			}

			string expression = ifLine.Substring(currentChar);
			string condition = ifLine.Substring(0, currentChar);

			if (!Regex.IsMatch(expression, @"(?:(?!(?:\s*$)|(?:\s*then)|(?:\s*and.*)|(?:\s*or.*)|(?:\s*not.*)))^.*$")) return ifMatch.Groups[0].ToString();

			return string.Format("if {0} then {1} end", condition, expression);
		}
	}
}