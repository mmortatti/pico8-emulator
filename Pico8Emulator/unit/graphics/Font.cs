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
		public static Dictionary<char, byte[,]> dictionary;

		#region digit definitions
		public static byte[,] f_empty = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_exclamation = {
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 0, 0},
			{0, 1, 0},
		};

		public static byte[,] f_quotes = {
			{1, 0, 1},
			{1, 0, 1},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_hashtag = {
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
		};

		public static byte[,] f_dolar = {
			{1, 1, 1},
			{1, 1, 0},
			{0, 1, 1},
			{1, 1, 1},
			{0, 1, 0},
		};

		public static byte[,] f_percentage = {
			{1, 0, 1},
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
			{1, 0, 1},
		};

		public static byte[,] f_ampersand = {
			{1, 1, 0},
			{1, 1, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_tone = {
			{0, 1, 0},
			{1, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_par_open = {
			{0, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{0, 1, 0},
		};

		public static byte[,] f_par_close = {
			{0, 1, 0},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 1, 0},
		};

		public static byte[,] f_astherisc = {
			{1, 0, 1},
			{0, 1, 0},
			{1, 1, 1},
			{0, 1, 0},
			{1, 0, 1},
		};

		public static byte[,] f_plus = {
			{0, 0, 0},
			{0, 1, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 0, 0},
		};

		public static byte[,] f_comma = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 0},
		};

		public static byte[,] f_dash = {
			{0, 0, 0},
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_dot = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 1, 0},
		};

		public static byte[,] f_slash = {
			{0, 0, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 0, 0},
		};

		public static byte[,] f_0 = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_1 = {
			{1, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 1},
		};

		public static byte[,] f_2 = {
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_3 = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_4 = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
		};

		public static byte[,] f_5 = {
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_6 = {
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_7 = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
		};

		public static byte[,] f_8 = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_9 = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
		};

		public static byte[,] f_colon = {
			{0, 0, 0},
			{0, 1, 0},
			{0, 0, 0},
			{0, 1, 0},
			{0, 0, 0},
		};

		public static byte[,] f_semicolon = {
			{0, 0, 0},
			{0, 1, 0},
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 0},
		};

		public static byte[,] f_less = {
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
			{0, 1, 0},
			{0, 0, 1},
		};

		public static byte[,] f_equals = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 0},
		};

		public static byte[,] f_greater = {
			{1, 0, 0},
			{0, 1, 0},
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
		};

		public static byte[,] f_question = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 1, 1},
			{0, 0, 0},
			{0, 1, 0},
		};

		public static byte[,] f_at = {
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 0},
			{0, 1, 1},
		};

		public static byte[,] f_a = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
		};

		public static byte[,] f_b = {
			{0, 0, 0},
			{1, 1, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_c = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_d = {
			{0, 0, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
		};

		public static byte[,] f_e = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 1, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_f = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
		};

		public static byte[,] f_g = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_h = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
		};

		public static byte[,] f_i = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 1},
		};

		public static byte[,] f_j = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 0},
		};

		public static byte[,] f_k = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_l = {
			{0, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_m = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_n = {
			{0, 0, 0},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_o = {
			{0, 0, 0},
			{0, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
		};

		public static byte[,] f_p = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
		};

		public static byte[,] f_q = {
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 1},
			{1, 1, 0},
			{0, 1, 1},
		};

		public static byte[,] f_r = {
			{0, 0, 0},
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
		};

		public static byte[,] f_s = {
			{0, 0, 0},
			{0, 1, 1},
			{1, 0, 0},
			{0, 0, 1},
			{1, 1, 0},
		};

		public static byte[,] f_t = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
		};

		public static byte[,] f_u = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 1},
		};

		public static byte[,] f_v = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 1, 0},
		};

		public static byte[,] f_w = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 1, 1},
		};

		public static byte[,] f_x = {
			{0, 0, 0},
			{1, 0, 1},
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_y = {
			{0, 0, 0},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_z = {
			{0, 0, 0},
			{1, 1, 1},
			{0, 0, 1},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_bracket_open = {
			{1, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 0},
		};

		public static byte[,] f_backslash = {
			{1, 0, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 0, 1},
		};

		public static byte[,] f_bracket_close = {
			{0, 1, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 0, 1},
			{0, 1, 1},
		};

		public static byte[,] f_carat = {
			{0, 1, 0},
			{1, 0, 1},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_underscore = {
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_back_quote = {
			{0, 1, 0},
			{0, 0, 1},
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_A = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_B = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_C = {
			{0, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{0, 1, 1},
		};

		public static byte[,] f_D = {
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_E = {
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_F = {
			{1, 1, 1},
			{1, 0, 0},
			{1, 1, 0},
			{1, 0, 0},
			{1, 0, 0},
		};

		public static byte[,] f_G = {
			{0, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_H = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_I = {
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 1},
		};

		public static byte[,] f_J = {
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{1, 1, 0},
		};

		public static byte[,] f_K = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_L = {
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_M = {
			{1, 1, 1},
			{1, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_N = {
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_O = {
			{0, 1, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
		};

		public static byte[,] f_P = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
			{1, 0, 0},
		};

		public static byte[,] f_Q = {
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 0},
			{0, 1, 1},
		};

		public static byte[,] f_R = {
			{1, 1, 1},
			{1, 0, 1},
			{1, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_S = {
			{0, 1, 1},
			{1, 0, 0},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 0},
		};

		public static byte[,] f_T = {
			{1, 1, 1},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
		};

		public static byte[,] f_U = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 1},
		};

		public static byte[,] f_V = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 0},
		};

		public static byte[,] f_W = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{1, 1, 1},
		};

		public static byte[,] f_X = {
			{1, 0, 1},
			{1, 0, 1},
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
		};

		public static byte[,] f_Y = {
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
			{0, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_Z = {
			{1, 1, 1},
			{0, 0, 1},
			{0, 1, 0},
			{1, 0, 0},
			{1, 1, 1},
		};

		public static byte[,] f_brace_open = {
			{0, 1, 1},
			{0, 1, 0},
			{1, 1, 0},
			{0, 1, 0},
			{0, 1, 1},
		};

		public static byte[,] f_pipe = {
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
			{0, 1, 0},
		};

		public static byte[,] f_brace_close = {
			{1, 1, 0},
			{0, 1, 0},
			{0, 1, 1},
			{0, 1, 0},
			{1, 1, 0},
		};

		public static byte[,] f_tilde = {
			{0, 0, 0},
			{0, 0, 1},
			{1, 1, 1},
			{1, 0, 0},
			{0, 0, 0},
		};

		public static byte[,] f_nubbin = {
			{0, 0, 0},
			{0, 1, 0},
			{1, 0, 1},
			{1, 0, 1},
			{1, 1, 1},
		};

		public static byte[,] f_block = {
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 1, 1, 1, 1, 1, 1},
		};

		public static byte[,] f_semi_block = {
			{1, 0, 1, 0, 1, 0, 1},
			{0, 1, 0, 1, 0, 1, 0},
			{1, 0, 1, 0, 1, 0, 1},
			{0, 1, 0, 1, 0, 1, 0},
			{1, 0, 1, 0, 1, 0, 1},
		};

		public static byte[,] f_alien = {
			{1, 0, 0, 0, 0, 0, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 0, 1, 1, 1, 0, 1},
			{1, 0, 1, 1, 1, 0, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_downbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 1, 0, 1, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_quasi_block = {
			{1, 0, 0, 0, 1, 0, 0},
			{0, 0, 1, 0, 0, 0, 1},
			{1, 0, 0, 0, 1, 0, 0},
			{0, 0, 1, 0, 0, 0, 1},
			{1, 0, 0, 0, 1, 0, 0},
		};

		public static byte[,] f_shuriken = {
			{0, 0, 1, 0, 0, 0, 0},
			{0, 0, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 0, 0},
			{0, 0, 0, 0, 1, 0, 0},
		};

		public static byte[,] f_shiny_ball = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 0, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
		};

		public static byte[,] f_heart = {
			{0, 1, 1, 0, 1, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
		};

		public static byte[,] f_sauron = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 0, 1, 1, 0},
			{1, 1, 1, 0, 1, 1, 1},
			{0, 1, 1, 0, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
		};

		public static byte[,] f_human = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 1, 0, 1, 0, 0},
		};

		public static byte[,] f_house = {
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 1, 1, 1, 1, 1},
			{0, 1, 0, 1, 0, 1, 0},
			{0, 1, 0, 1, 1, 1, 0},
		};

		public static byte[,] f_leftbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 1, 0, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 1, 0, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_face = {
			{1, 1, 1, 1, 1, 1, 1},
			{1, 0, 1, 1, 1, 0, 1},
			{1, 1, 1, 1, 1, 1, 1},
			{1, 0, 0, 0, 0, 0, 1},
			{1, 1, 1, 1, 1, 1, 1},
		};

		public static byte[,] f_note = {
			{0, 0, 0, 1, 1, 1, 0},
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
			{0, 1, 1, 1, 0, 0, 0},
			{0, 1, 1, 1, 0, 0, 0},
		};

		public static byte[,] f_obutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 1, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_diamond = {
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
		};

		public static byte[,] f_dot_line = {
			{0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0},
			{1, 0, 1, 0, 1, 0, 1},
			{0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0},
		};

		public static byte[,] f_rightbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 0, 1, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 0, 1, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_star = {
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{1, 1, 1, 1, 1, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
			{0, 1, 0, 0, 0, 1, 0},
		};

		public static byte[,] f_hourclass = {
			{0, 1, 1, 1, 1, 1, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 0, 0, 1, 0, 0, 0},
			{0, 0, 1, 1, 1, 0, 0},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_upbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 1, 0, 1, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{1, 1, 0, 0, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_down_arrows = {
			{0, 0, 0, 0, 0, 0, 0},
			{1, 0, 1, 0, 0, 0, 0},
			{0, 1, 0, 0, 1, 0, 1},
			{0, 0, 0, 0, 0, 1, 0},
			{0, 0, 0, 0, 0, 0, 0},
		};

		public static byte[,] f_triangle_wave = {
			{0, 0, 0, 0, 0, 0, 0},
			{1, 0, 0, 0, 1, 0, 0},
			{0, 1, 0, 1, 0, 1, 0},
			{0, 0, 1, 0, 0, 0, 1},
			{0, 0, 0, 0, 0, 0, 0},
		};

		public static byte[,] f_xbutton = {
			{0, 1, 1, 1, 1, 1, 0},
			{1, 1, 0, 1, 0, 1, 1},
			{1, 1, 1, 0, 1, 1, 1},
			{1, 1, 0, 1, 0, 1, 1},
			{0, 1, 1, 1, 1, 1, 0},
		};

		public static byte[,] f_horizontal_lines = {
			{1, 1, 1, 1, 1, 1, 1},
			{0, 0, 0, 0, 0, 0, 0},
			{1, 1, 1, 1, 1, 1, 1},
			{0, 0, 0, 0, 0, 0, 0},
			{1, 1, 1, 1, 1, 1, 1},
		};

		public static byte[,] f_vertical_lines = {
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
			{1, 0, 1, 0, 1, 0, 1},
		};
		#endregion

		static Font() {
			dictionary = new Dictionary<char, byte[,]>();
			dictionary.Add(' ', f_empty);
			dictionary.Add('!', f_exclamation);
			dictionary.Add('"', f_quotes);
			dictionary.Add('#', f_hashtag);
			dictionary.Add('$', f_dolar);
			dictionary.Add('%', f_percentage);
			dictionary.Add('&', f_ampersand);
			dictionary.Add('\'', f_tone);
			dictionary.Add('(', f_par_open);
			dictionary.Add(')', f_par_close);
			dictionary.Add('*', f_astherisc);
			dictionary.Add('+', f_plus);
			dictionary.Add(',', f_comma);
			dictionary.Add('-', f_dash);
			dictionary.Add('.', f_dot);
			dictionary.Add('/', f_slash);
			dictionary.Add('0', f_0);
			dictionary.Add('1', f_1);
			dictionary.Add('2', f_2);
			dictionary.Add('3', f_3);
			dictionary.Add('4', f_4);
			dictionary.Add('5', f_5);
			dictionary.Add('6', f_6);
			dictionary.Add('7', f_7);
			dictionary.Add('8', f_8);
			dictionary.Add('9', f_9);
			dictionary.Add(':', f_colon);
			dictionary.Add(';', f_semicolon);
			dictionary.Add('<', f_less);
			dictionary.Add('=', f_equals);
			dictionary.Add('>', f_greater);
			dictionary.Add('?', f_question);
			dictionary.Add('@', f_at);
			dictionary.Add('a', f_a);
			dictionary.Add('b', f_b);
			dictionary.Add('c', f_c);
			dictionary.Add('d', f_d);
			dictionary.Add('e', f_e);
			dictionary.Add('f', f_f);
			dictionary.Add('g', f_g);
			dictionary.Add('h', f_h);
			dictionary.Add('i', f_i);
			dictionary.Add('j', f_j);
			dictionary.Add('k', f_k);
			dictionary.Add('l', f_l);
			dictionary.Add('m', f_m);
			dictionary.Add('n', f_n);
			dictionary.Add('o', f_o);
			dictionary.Add('p', f_p);
			dictionary.Add('q', f_q);
			dictionary.Add('r', f_r);
			dictionary.Add('s', f_s);
			dictionary.Add('t', f_t);
			dictionary.Add('u', f_u);
			dictionary.Add('v', f_v);
			dictionary.Add('w', f_w);
			dictionary.Add('x', f_x);
			dictionary.Add('y', f_y);
			dictionary.Add('z', f_z);
			dictionary.Add('[', f_bracket_open);
			dictionary.Add('\\', f_backslash);
			dictionary.Add(']', f_bracket_close);
			dictionary.Add('^', f_carat);
			dictionary.Add('_', f_underscore);
			dictionary.Add('`', f_back_quote);
			dictionary.Add('A', f_A);
			dictionary.Add('B', f_B);
			dictionary.Add('C', f_C);
			dictionary.Add('D', f_D);
			dictionary.Add('E', f_E);
			dictionary.Add('F', f_F);
			dictionary.Add('G', f_G);
			dictionary.Add('H', f_H);
			dictionary.Add('I', f_I);
			dictionary.Add('J', f_J);
			dictionary.Add('K', f_K);
			dictionary.Add('L', f_L);
			dictionary.Add('M', f_M);
			dictionary.Add('N', f_N);
			dictionary.Add('O', f_O);
			dictionary.Add('P', f_P);
			dictionary.Add('Q', f_Q);
			dictionary.Add('R', f_R);
			dictionary.Add('S', f_S);
			dictionary.Add('T', f_T);
			dictionary.Add('U', f_U);
			dictionary.Add('V', f_V);
			dictionary.Add('W', f_W);
			dictionary.Add('X', f_X);
			dictionary.Add('Y', f_Y);
			dictionary.Add('Z', f_Z);
			dictionary.Add('{', f_brace_open);
			dictionary.Add('|', f_pipe);
			dictionary.Add('}', f_brace_close);
			dictionary.Add('~', f_tilde);
			dictionary.Add((char) 127, f_nubbin);
			dictionary.Add((char) 128, f_block);
			dictionary.Add((char) 129, f_semi_block);
			dictionary.Add((char) 130, f_alien);
			dictionary.Add((char) 131, f_downbutton);
			dictionary.Add((char) 132, f_quasi_block);
			dictionary.Add((char) 133, f_shuriken);
			dictionary.Add((char) 134, f_shiny_ball);
			dictionary.Add((char) 135, f_heart);
			dictionary.Add((char) 136, f_sauron);
			dictionary.Add((char) 137, f_human);
			dictionary.Add((char) 138, f_house);
			dictionary.Add((char) 139, f_leftbutton);
			dictionary.Add((char) 140, f_face);
			dictionary.Add((char) 141, f_note);
			dictionary.Add((char) 142, f_obutton);
			dictionary.Add((char) 143, f_diamond);
			dictionary.Add((char) 144, f_dot_line);
			dictionary.Add((char) 145, f_rightbutton);
			dictionary.Add((char) 146, f_star);
			dictionary.Add((char) 147, f_hourclass);
			dictionary.Add((char) 148, f_upbutton);
			dictionary.Add((char) 149, f_down_arrows);
			dictionary.Add((char) 150, f_triangle_wave);
			dictionary.Add((char) 151, f_xbutton);
			dictionary.Add((char) 152, f_horizontal_lines);
			dictionary.Add((char) 153, f_vertical_lines);
		}
	}
}