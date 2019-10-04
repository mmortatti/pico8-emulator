using System.Collections.Generic;

namespace Pico8Emulator.unit.graphics {
	/*
	 * TODO: use hex instead of this
	 * 000 = 0x0
	 * 111 = 0x9
	 * etc
	 *
	 * new pico8 chars
	 */
	public static class Font {
		public static Dictionary<char, byte[,]> Dictionary;

		#region digit definitions
		private static byte[,] empty = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] exclamation = {
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 0, 0},
			{0, 1, 0},
		};

		private static byte[,] quotes = {
			{1, 0, 1},
			{1, 0, 1},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] hashtag = {
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
		};

		private static byte[,] dolar = {
			{1, 1, 1},
			{1, 1, 0},
			{0, 1, 1},
			{1, 1, 1},
			{0, 1, 0},
		};

		private static byte[,] percentage = {
			{1, 0, 1},
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
			{1, 0, 1},
		};

		private static byte[,] ampersand = {
			{1, 1, 0},
			{1, 1, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] tone = {
			{0, 1, 0},
			{1, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] par_open = {
			{0, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{0, 1, 0},
		};

		private static byte[,] par_close = {
			{0, 1, 0},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 1, 0},
		};

		private static byte[,] astherisc = {
			{1, 0, 1},
			{0, 1, 0},
			{1, 1, 1},
			{0, 1, 0},
			{1, 0, 1},
		};

		private static byte[,] plus = {
			{0, 0, 0},
			{0, 1, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 0, 0},
		};

		private static byte[,] comma = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 0},
		};

		private static byte[,] dash = {
			{0, 0, 0},
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] dot = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 1, 0},
		};

		private static byte[,] slash = {
			{0, 0, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 0, 0},
		};

		private static byte[,] digit0 = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] digit1 = {
			{1, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 1},
		};

		private static byte[,] digit2 = {
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] digit3 = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] digit4 = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
		};

		private static byte[,] digit5 = {
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] digit6 = {
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] digit7 = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
		};

		private static byte[,] digit8 = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] digit9 = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
		};

		private static byte[,] colon = {
			{0, 0, 0},
			{0, 1, 0},
			{0, 0, 0},
			{0, 1, 0},
			{0, 0, 0},
		};

		private static byte[,] semicolon = {
			{0, 0, 0},
			{0, 1, 0},
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 0},
		};

		private static byte[,] less = {
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
			{0, 1, 0},
			{0, 0, 1},
		};

		private static byte[,] equals = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 0},
		};

		private static byte[,] greater = {
			{1, 0, 0},
			{0, 1, 0},
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
		};

		private static byte[,] question = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 1, 1},
			{0, 0, 0},
			{0, 1, 0},
		};

		private static byte[,] at = {
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 0},
			{0, 1, 1},
		};

		private static byte[,] a = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
		};

		private static byte[,] b = {
			{0, 0, 0},
			{1, 1, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] c = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] d = {
			{0, 0, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
		};

		private static byte[,] e = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 1, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] f = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
		};

		private static byte[,] g = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] h = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
		};

		private static byte[,] i = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 1},
		};

		private static byte[,] j = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 0},
		};

		private static byte[,] k = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] l = {
			{0, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] m = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] n = {
			{0, 0, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] o = {
			{0, 0, 0},
			{0, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
		};

		private static byte[,] p = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
		};

		private static byte[,] q = {
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 1},
			{1, 1, 0},
			{0, 1, 1},
		};

		private static byte[,] r = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
		};

		private static byte[,] s = {
			{0, 0, 0},
			{0, 1, 1},
			{1, 0, 0},
			{0, 0, 1},
			{1, 1, 0},
		};

		private static byte[,] t = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
		};

		private static byte[,] u = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 1},
		};

		private static byte[,] v = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 1, 0},
		};

		private static byte[,] w = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 1, 1},
		};

		private static byte[,] x = {
			{0, 0, 0},
			{1, 0, 1},
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] y = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] z = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 1},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] bracket_open = {
			{1, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 0},
		};

		private static byte[,] backslash = {
			{1, 0, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 0, 1},
		};

		private static byte[,] bracket_close = {
			{0, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 1, 1},
		};

		private static byte[,] carat = {
			{0, 1, 0},
			{1, 0, 1},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] underscore = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] back_quote = {
			{0, 1, 0},
			{0, 0, 1},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] A = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] B = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] C = {
			{0, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{0, 1, 1},
		};

		private static byte[,] D = {
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] E = {
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] F = {
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
		};

		private static byte[,] G = {
			{0, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] H = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] I = {
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 1},
		};

		private static byte[,] J = {
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 0},
		};

		private static byte[,] K = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] L = {
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] M = {
			{1, 1, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] N = {
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] O = {
			{0, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
		};

		private static byte[,] P = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
		};

		private static byte[,] Q = {
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
			{0, 1, 1},
		};

		private static byte[,] R = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] S = {
			{0, 1, 1},
			{1, 0, 0},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 0},
		};

		private static byte[,] T = {
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
		};

		private static byte[,] U = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 1},
		};

		private static byte[,] V = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 0},
		};

		private static byte[,] W = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 1, 1},
		};

		private static byte[,] X = {
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		private static byte[,] Y = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] Z = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		private static byte[,] brace_open = {
			{0, 1, 1},
			{0, 1, 0},
			{1, 1, 0},
			{0, 1, 0},
			{0, 1, 1},
		};

		private static byte[,] pipe = {
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
		};

		private static byte[,] brace_close = {
			{1, 1, 0},
			{0, 1, 0},
			{0, 1, 1},
			{0, 1, 0},
			{1, 1, 0},
		};

		private static byte[,] tilde = {
			{0, 0, 0},
			{0, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
			{0, 0, 0},
		};

		private static byte[,] nubbin = {
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		private static byte[,] block = {
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
		};

		private static byte[,] semi_block = {
			{1, 0, 1, 0, 1, 0, 1},
			{0, 1, 0, 1, 0, 1, 0},
			{1, 0, 1, 0, 1, 0, 1},
			{0, 1, 0, 1, 0, 1, 0},
			{1, 0, 1, 0, 1, 0, 1},
		};

		private static byte[,] alien = {
			{1, 0, 0, 0, 0, 0, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 0, 1, 1, 1, 0, 1},
			{1, 0, 1, 1, 1, 0, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] downbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 1, 0, 1, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] quasi_block = {
			{1, 0, 0, 0, 1, 0, 0},
			{0, 0, 1, 0, 0, 0, 1},
			{1, 0, 0, 0, 1, 0, 0},
			{0, 0, 1, 0, 0, 0, 1},
			{1, 0, 0, 0, 1, 0, 0},
		};

		private static byte[,] shuriken = {
			{0, 0, 1, 0, 0, 0, 0},
			{0, 0, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 0, 0},
			{0, 0, 0, 0, 1, 0, 0},
		};

		private static byte[,] shiny_ball = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 0, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
		};

		private static byte[,] heart = {
			{0, 1, 1, 0, 1, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
		};

		private static byte[,] sauron = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 0, 1, 1, 0},
			{1, 1, 1, 0, 1, 1, 1},
			{0, 1, 1, 0, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
		};

		private static byte[,] human = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 1, 0, 1, 0, 0},
		};

		private static byte[,] house = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 1, 1, 1, 1, 1},
			{0, 1, 0, 1, 0, 1, 0},
			{0, 1, 0, 1, 1, 1, 0},
		};

		private static byte[,] leftbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 1, 0, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 1, 0, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] face = {
			{1, 1, 1, 1, 1, 1, 1},
			{1, 0, 1, 1, 1, 0, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 0, 0, 0, 0, 0, 1},
			{1, 1, 1, 1, 1, 1, 1},
		};

		private static byte[,] note = {
			{0, 0, 0, 1, 1, 1, 0},
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
			{0, 1, 1, 1, 0, 0, 0},
			{0, 1, 1, 1, 0, 0, 0},
		};

		private static byte[,] obutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 1, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] diamond = {
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
		};

		private static byte[,] dot_line = {
			{0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0},
			{1, 0, 1, 0, 1, 0, 1},
			{0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0},
		};

		private static byte[,] rightbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 0, 1, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 0, 1, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] star = {
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{1, 1, 1, 1, 1, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 1, 0, 0, 0, 1, 0},
		};

		private static byte[,] hourclass = {
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] upbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 1, 0, 1, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] down_arrows = {
			{0, 0, 0, 0, 0, 0, 0},
			{1, 0, 1, 0, 0, 0, 0},
			{0, 1, 0, 0, 1, 0, 1},
			{0, 0, 0, 0, 0, 1, 0},
			{0, 0, 0, 0, 0, 0, 0},
		};

		private static byte[,] triangle_wave = {
			{0, 0, 0, 0, 0, 0, 0},
			{1, 0, 0, 0, 1, 0, 0},
			{0, 1, 0, 1, 0, 1, 0},
			{0, 0, 1, 0, 0, 0, 1},
			{0, 0, 0, 0, 0, 0, 0},
		};

		private static byte[,] xbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 1, 0, 1, 1},
			{1, 1, 1, 0, 1, 1, 1},
			{1, 1, 0, 1, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		private static byte[,] horizontal_lines = {
			{1, 1, 1, 1, 1, 1, 1},
			{0, 0, 0, 0, 0, 0, 0},
			{1, 1, 1, 1, 1, 1, 1},
			{0, 0, 0, 0, 0, 0, 0},
			{1, 1, 1, 1, 1, 1, 1},
		};

		private static byte[,] vertical_lines = {
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
		};
		#endregion

		static Font() {
			Dictionary = new Dictionary<char, byte[,]>();
			Dictionary.Add(' ', empty);
			Dictionary.Add('!', exclamation);
			Dictionary.Add('"', quotes);
			Dictionary.Add('#', hashtag);
			Dictionary.Add('$', dolar);
			Dictionary.Add('%', percentage);
			Dictionary.Add('&', ampersand);
			Dictionary.Add('\'', tone);
			Dictionary.Add('(', par_open);
			Dictionary.Add(')', par_close);
			Dictionary.Add('*', astherisc);
			Dictionary.Add('+', plus);
			Dictionary.Add(',', comma);
			Dictionary.Add('-', dash);
			Dictionary.Add('.', dot);
			Dictionary.Add('/', slash);
			Dictionary.Add('0', digit0);
			Dictionary.Add('1', digit1);
			Dictionary.Add('2', digit2);
			Dictionary.Add('3', digit3);
			Dictionary.Add('4', digit4);
			Dictionary.Add('5', digit5);
			Dictionary.Add('6', digit6);
			Dictionary.Add('7', digit7);
			Dictionary.Add('8', digit8);
			Dictionary.Add('9', digit9);
			Dictionary.Add(':', colon);
			Dictionary.Add(';', semicolon);
			Dictionary.Add('<', less);
			Dictionary.Add('=', equals);
			Dictionary.Add('>', greater);
			Dictionary.Add('?', question);
			Dictionary.Add('@', at);
			Dictionary.Add('a', a);
			Dictionary.Add('b', b);
			Dictionary.Add('c', c);
			Dictionary.Add('d', d);
			Dictionary.Add('e', e);
			Dictionary.Add('f', f);
			Dictionary.Add('g', g);
			Dictionary.Add('h', h);
			Dictionary.Add('i', i);
			Dictionary.Add('j', j);
			Dictionary.Add('k', k);
			Dictionary.Add('l', l);
			Dictionary.Add('m', m);
			Dictionary.Add('n', n);
			Dictionary.Add('o', o);
			Dictionary.Add('p', p);
			Dictionary.Add('q', q);
			Dictionary.Add('r', r);
			Dictionary.Add('s', s);
			Dictionary.Add('t', t);
			Dictionary.Add('u', u);
			Dictionary.Add('v', v);
			Dictionary.Add('w', w);
			Dictionary.Add('x', x);
			Dictionary.Add('y', y);
			Dictionary.Add('z', z);
			Dictionary.Add('[', bracket_open);
			Dictionary.Add('\\', backslash);
			Dictionary.Add(']', bracket_close);
			Dictionary.Add('^', carat);
			Dictionary.Add('_', underscore);
			Dictionary.Add('`', back_quote);
			Dictionary.Add('A', A);
			Dictionary.Add('B', B);
			Dictionary.Add('C', C);
			Dictionary.Add('D', D);
			Dictionary.Add('E', E);
			Dictionary.Add('F', F);
			Dictionary.Add('G', G);
			Dictionary.Add('H', H);
			Dictionary.Add('I', I);
			Dictionary.Add('J', J);
			Dictionary.Add('K', K);
			Dictionary.Add('L', L);
			Dictionary.Add('M', M);
			Dictionary.Add('N', N);
			Dictionary.Add('O', O);
			Dictionary.Add('P', P);
			Dictionary.Add('Q', Q);
			Dictionary.Add('R', R);
			Dictionary.Add('S', S);
			Dictionary.Add('T', T);
			Dictionary.Add('U', U);
			Dictionary.Add('V', V);
			Dictionary.Add('W', W);
			Dictionary.Add('X', X);
			Dictionary.Add('Y', Y);
			Dictionary.Add('Z', Z);
			Dictionary.Add('{', brace_open);
			Dictionary.Add('|', pipe);
			Dictionary.Add('}', brace_close);
			Dictionary.Add('~', tilde);
			Dictionary.Add((char) 127, nubbin);
			Dictionary.Add((char) 128, block);
			Dictionary.Add((char) 129, semi_block);
			Dictionary.Add((char) 130, alien);
			Dictionary.Add((char) 131, downbutton);
			Dictionary.Add((char) 132, quasi_block);
			Dictionary.Add((char) 133, shuriken);
			Dictionary.Add((char) 134, shiny_ball);
			Dictionary.Add((char) 135, heart);
			Dictionary.Add((char) 136, sauron);
			Dictionary.Add((char) 137, human);
			Dictionary.Add((char) 138, house);
			Dictionary.Add((char) 139, leftbutton);
			Dictionary.Add((char) 140, face);
			Dictionary.Add((char) 141, note);
			Dictionary.Add((char) 142, obutton);
			Dictionary.Add((char) 143, diamond);
			Dictionary.Add((char) 144, dot_line);
			Dictionary.Add((char) 145, rightbutton);
			Dictionary.Add((char) 146, star);
			Dictionary.Add((char) 147, hourclass);
			Dictionary.Add((char) 148, upbutton);
			Dictionary.Add((char) 149, down_arrows);
			Dictionary.Add((char) 150, triangle_wave);
			Dictionary.Add((char) 151, xbutton);
			Dictionary.Add((char) 152, horizontal_lines);
			Dictionary.Add((char) 153, vertical_lines);
		}
	}
}