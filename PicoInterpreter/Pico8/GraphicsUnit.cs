namespace pico8_interpreter.Pico8
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the PICO-8 <see cref="GraphicsUnit{G}" />
    /// </summary>
    /// <typeparam name="G"></typeparam>
    public class GraphicsUnit<G>
    {
        #region digit definitions

        public byte[,] f_empty = {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_exclamation = {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_quotes = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_hashtag = {
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_dolar = {
            { 1, 1, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 },
        };

        public byte[,] f_percentage = {
            { 1, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 1 },
        };

        public byte[,] f_ampersand = {
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_tone = {
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_par_open = {
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_par_close = {
            { 0, 1, 0 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
        };

        public byte[,] f_astherisc = {
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 },
        };

        public byte[,] f_plus = {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_comma = {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_dash = {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_dot = {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_slash = {
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_0 = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_1 = {
            { 1, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_2 = {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_3 = {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_4 = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
        };

        public byte[,] f_5 = {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_6 = {
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_7 = {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
        };

        public byte[,] f_8 = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_9 = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
        };

        public byte[,] f_colon = {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_semicolon = {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_less = {
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 },
        };

        public byte[,] f_equals = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
        };

        public byte[,] f_greater = {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_question = {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
            { 0, 0, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_at = {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 0 },
            { 0, 1, 1 },
        };

        public byte[,] f_a= {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_b = {
            { 0, 0, 0 },
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_c = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_d = {
            { 0, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
        };

        public byte[,] f_e = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_f = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_g = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_h = {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_i = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_j = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 0 },
        };

        public byte[,] f_k = {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_l = {
            { 0, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_m = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_n= {
            { 0, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_o = {
            { 0, 0, 0 },
            { 0, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
        };

        public byte[,] f_p = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
        };

        public byte[,] f_q = {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 },
        };

        public byte[,] f_r = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
        };

        public byte[,] f_s = {
            { 0, 0, 0 },
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 0, 0, 1 },
            { 1, 1, 0 },
        };

        public byte[,] f_t = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_u = {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 1 },
        };

        public byte[,] f_v = {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 },
        };

        public byte[,] f_w = {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_x= {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_y = {
            { 0, 0, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_z = {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_bracket_open = {
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 0 },
        };

        public byte[,] f_backslash = {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 },
        };

        public byte[,] f_bracket_close = {
            { 0, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
        };

        public byte[,] f_carat = {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_underscore = {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_back_quote = {
            { 0, 1, 0 },
            { 0, 0, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_A = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_B = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_C = {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 0, 1, 1 },
        };

        public byte[,] f_D = {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_E = {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_F = {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_G = {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_H = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_I = {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_J = {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 0 },
        };

        public byte[,] f_K = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_L = {
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_M = {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_N = {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_O = {
            { 0, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
        };

        public byte[,] f_P = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
        };

        public byte[,] f_Q = {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 },
        };

        public byte[,] f_R = {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_S = {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 0 },
        };

        public byte[,] f_T = {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_U = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 1 },
        };

        public byte[,] f_V = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 0 },
        };

        public byte[,] f_W = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_X = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
        };

        public byte[,] f_Y = {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_Z = {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
        };

        public byte[,] f_brace_open = {
            { 0, 1, 1 },
            { 0, 1, 0 },
            { 1, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 1 },
        };

        public byte[,] f_pipe = {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
        };

        public byte[,] f_brace_close = {
            { 1, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 1 },
            { 0, 1, 0 },
            { 1, 1, 0 },
        };

        public byte[,] f_tilde = {
            { 0, 0, 0 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 0, 0, 0 },
        };

        public byte[,] f_nubbin = {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
        };

        public byte[,] f_block = {
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
        };

        public byte[,] f_semi_block = {
            { 1, 0, 1, 0, 1, 0, 1 },
            { 0, 1, 0, 1, 0, 1, 0 },
            { 1, 0, 1, 0, 1, 0, 1 },
            { 0, 1, 0, 1, 0, 1, 0 },
            { 1, 0, 1, 0, 1, 0, 1 },
        };

        public byte[,] f_alien = {
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 1, 1, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 0, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_downbutton = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 1, 1, 1, 0, 1, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_quasi_block = {
            { 1, 0, 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 1, 0, 0 },
        };

        public byte[,] f_shuriken = {
            { 0, 0, 1, 0, 0, 0, 0 },
            { 0, 0, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0 },
        };

        public byte[,] f_shiny_ball = {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
        };

        public byte[,] f_heart = {
            { 0, 1, 1, 0, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0 },
        };

        public byte[,] f_sauron = {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 0, 1, 1, 0 },
            { 1, 1, 1, 0, 1, 1, 1 },
            { 0, 1, 1, 0, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
        };

        public byte[,] f_human = {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 0, 1, 0, 0 },
        };

        public byte[,] f_house = {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 0, 1, 0, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 1, 1, 0 },
        };

        public byte[,] f_leftbutton = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 1, 0, 0, 1, 1 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 1, 1, 1, 0, 0, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_face = {
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 1, 1, 1, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
        };

        public byte[,] f_note = {
            { 0, 0, 0, 1, 1, 1, 0 },
            { 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0 },
            { 0, 1, 1, 1, 0, 0, 0 },
            { 0, 1, 1, 1, 0, 0, 0 },
        };

        public byte[,] f_obutton = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 1, 1, 0, 1, 0, 1, 1 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_diamond = {
            { 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0 },
        };

        public byte[,] f_dot_line = {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0 },
            { 1, 0, 1, 0, 1, 0, 1 },
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0 },
        };
        public byte[,] f_rightbutton = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 0, 0, 1, 1, 1 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 1, 1, 0, 0, 1, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };
        public byte[,] f_star = {
            { 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 0, 0, 0, 1, 0 },
        };

        public byte[,] f_hourclass = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_upbutton = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 1, 0, 1, 1, 1 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 1, 1, 0, 0, 0, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_down_arrows = {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 1, 0, 1, 0, 0, 0, 0 },
            { 0, 1, 0, 0, 1, 0, 1 },
            { 0, 0, 0, 0, 0, 1, 0 },
            { 0, 0, 0, 0, 0, 0, 0 },
        };

        public byte[,] f_triangle_wave = {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 1, 0, 0, 0, 1, 0, 0 },
            { 0, 1, 0, 1, 0, 1, 0 },
            { 0, 0, 1, 0, 0, 0, 1 },
            { 0, 0, 0, 0, 0, 0, 0 },
        };

        public byte[,] f_xbutton = {
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 0, 1, 0, 1, 1 },
            { 1, 1, 1, 0, 1, 1, 1 },
            { 1, 1, 0, 1, 0, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
        };

        public byte[,] f_horizontal_lines = {
            { 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1 },
        };

        public byte[,] f_vertical_lines = {
            { 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1 },
        };


        #endregion

        Dictionary<char, byte[,]> dictionary;
            
        /// <summary>
        /// Defines RGB values for the PICO-8 palette.
        /// </summary>
        public int[,] pico8Palette = {
            // Black
            { 0, 0, 0 },
            // Dark-blue
            { 29, 43, 83 },
            // Dark-purple
            { 126, 37, 83 },
            // Dark-green
            { 0, 135, 81 },
            // Brown
            { 171, 82, 54 },
            // Dark-gray
            { 95, 87, 79 },
            // Light-gray
            { 194, 195, 199 },
            // White
            { 255, 241, 232 },
            // Red
            { 255, 0, 77 },
            // Orange
            { 255, 163, 0 },
            // Yellow
            { 255, 236, 39 },
            // Green
            { 0, 228, 54 },
            // Blue
            { 41, 173, 255 },
            // Indigo
            { 131, 118, 156 },
            // Pink
            { 255, 119, 168 },
            // Peach
            { 255, 204, 170 } };

        /// <summary>
        /// Reference to PICO-8s memory unit.
        /// </summary>
        internal MemoryUnit memory;

        /// <summary>
        /// Defines the array of color values for PICO-8 screen.
        /// </summary>
        internal G[] screenColorData;

        /// <summary>
        /// Defines a function to translate RGB values to a color value.
        /// </summary>
        internal Func<int, int, int, G> rgbToColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsUnit{G}"/> class.
        /// </summary>
        /// <param name="memory">The <see cref="MemoryUnit"/> reference</param>
        /// <param name="screenColorData">The screenColorData reference</param>
        /// <param name="rgbToColor">The rgbToColor function to use</param>
        public GraphicsUnit(ref MemoryUnit memory, ref G[] screenColorData, Func<int, int, int, G> rgbToColor)
        {
            this.memory = memory;
            this.screenColorData = screenColorData;
            this.rgbToColor = rgbToColor;

            memory.DrawColor = 6;

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
            dictionary.Add((char)127, f_nubbin);
            dictionary.Add((char)128, f_block);
            dictionary.Add((char)129, f_semi_block);
            dictionary.Add((char)130, f_alien);
            dictionary.Add((char)131, f_downbutton);
            dictionary.Add((char)132, f_quasi_block);
            dictionary.Add((char)133, f_shuriken);
            dictionary.Add((char)134, f_shiny_ball);
            dictionary.Add((char)135, f_heart);
            dictionary.Add((char)136, f_sauron);
            dictionary.Add((char)137, f_human);
            dictionary.Add((char)138, f_house);
            dictionary.Add((char)139, f_leftbutton);
            dictionary.Add((char)140, f_face);
            dictionary.Add((char)141, f_note);
            dictionary.Add((char)142, f_obutton);
            dictionary.Add((char)143, f_diamond);
            dictionary.Add((char)144, f_dot_line);
            dictionary.Add((char)145, f_rightbutton);
            dictionary.Add((char)146, f_star);
            dictionary.Add((char)147, f_hourclass);
            dictionary.Add((char)148, f_upbutton);
            dictionary.Add((char)149, f_down_arrows);
            dictionary.Add((char)150, f_triangle_wave);
            dictionary.Add((char)151, f_xbutton);
            dictionary.Add((char)152, f_horizontal_lines);
            dictionary.Add((char)153, f_vertical_lines);
        }

        public void Print(object s, int? x = null, int? y = null, byte? c = null)
        {
            if (x.HasValue)
            {
                memory.cursorX = x.Value;
            } else
            {
                x = memory.cursorX;
            }

            if (y.HasValue)
            {
                memory.cursorY = y.Value;
            }
            else
            {
                y = memory.cursorY;
                memory.cursorY += 6;
            }

            int xOrig = x.Value;
            string prtStr = s.ToString();
            foreach(char l in prtStr)
            {
                if (l == '\n')
                {
                    y += 6;
                    x = xOrig;
                    continue;
                }

                if (dictionary.ContainsKey(l))
                {

                    byte[,] digit = dictionary[l];
                    for(int i = 0; i < digit.GetLength(0); i += 1)
                    {
                        for (int j = 0; j < digit.GetLength(1); j += 1)
                        {
                            if (digit[i,j] == 1)
                            {
                                Pset(x.Value + j, y.Value + i, c);
                            }
                        }
                    }

                    x += digit.GetLength(1) + 1;
                }
            }
        }

        /// <summary>
        /// Draw section of map (in cels) at screen position sx, sy (pixels)
        /// if layer is specified, only cels with the same flag number set are drawn
        /// </summary>
        /// <param name="cel_x">The starting x axis cell position</param>
        /// <param name="cel_y">The starting y axis cell position</param>
        /// <param name="sx">The screen position to draw to in the x axis</param>
        /// <param name="sy">The screen position to draw to in the y axis</param>
        /// <param name="cel_w">The width of the map to draw</param>
        /// <param name="cel_h">The height of the map to draw</param>
        /// <param name="layer">The layer to use for drawing</param>
        /// <returns>Returns null everytime</returns>
        public object Map(int cel_x, int cel_y, int sx, int sy, int cel_w, int cel_h, byte? layer = null)
        {
            for (int h = 0; h < cel_h; h++)
            {
                for (int w = 0; w < cel_w; w++)
                {

                    int addr = (cel_y + h) < 32 ? MemoryUnit.ADDR_MAP : MemoryUnit.ADDR_GFX_MAP;
                    byte spr_index = memory.Peek(addr + (cel_y + h) % 32 * 128 + cel_x + w);
                    byte flags = (byte)memory.Fget(spr_index, null);

                    // Spr index 0 is reserved for empty tiles
                    if (spr_index == 0)
                    {
                        continue;
                    }

                    // IF layer has not been specified, draw regardless
                    if (!layer.HasValue || (flags & layer.Value) != 0)
                    {
                        Spr(spr_index, sx + 8 * w, sy + 8 * h, 1, 1, false, false);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Turns current screen data into color data.
        /// </summary>
        public void Flip()
        {
            byte[] frameBuffer = memory.FrameBuffer;
            for (int i = 0; i < 64 * 128; i++)
            {
                byte val = frameBuffer[i];
                byte left = (byte)(val & 0x0f);
                byte right = (byte)(val >> 4);

                int rl = pico8Palette[memory.GetScreenColor(left), 0],
                    gl = pico8Palette[memory.GetScreenColor(left), 1],
                    bl = pico8Palette[memory.GetScreenColor(left), 2];
                int rr = pico8Palette[memory.GetScreenColor(right), 0],
                    gr = pico8Palette[memory.GetScreenColor(right), 1],
                    br = pico8Palette[memory.GetScreenColor(right), 2];
                screenColorData[i * 2] = rgbToColor(rl, gl, bl);
                screenColorData[i * 2 + 1] = rgbToColor(rr, gr, br);
            }
        }

        /// <summary>
        /// Draw sprite n (0..255) at position x,y.
		/// Width and height are 1,1 by default and specify how many sprites wide to blit.
        /// Colour 0 drawn as transparent by default (see palt())
        /// </summary>
        /// <param name="n">The number of the sprite to draw</param>
        /// <param name="x">The x position to draw the sprite to</param>
        /// <param name="y">The y position to draw the sprite to</param>
        /// <param name="w">The width of the spritesheet to draw from</param>
        /// <param name="h">The height of the spritesheet to draw from</param>
        /// <param name="flip_x">If it should flip horizontally</param>
        /// <param name="flip_y">If it should flip vertically</param>
        /// <returns>Returns null everytime</returns>
        public object Spr(int n, int x, int y, int? w = null, int? h = null, bool? flip_x = null, bool? flip_y = null)
        {
            if (n < 0 || n > 255)
            {
                return null;
            }

            int sprX = (n % 16) * 8, sprY = (n / 16) * 8;

            int width = 1, height = 1;
            if (w.HasValue)
            {
                width = w.Value;
            }
            if (h.HasValue)
            {
                height = h.Value;
            }

            bool flipX = false, flipY = false;
            if (flip_x.HasValue)
            {
                flipX = flip_x.Value;
            }
            if (flip_y.HasValue)
            {
                flipY = flip_y.Value;
            }

            for (int i = 0; i < 8 * width; i++)
            {
                for (int j = 0; j < 8 * height; j++)
                {
                    byte sprColor = Sget(i + sprX, j + sprY);
                    Psett(x + (flipX ? 8 * width - i : i), y + (flipY ? 8 * height - j : j), sprColor);
                }
            }

            return null;
        }

        /// <summary>
        /// Stretch rectangle from sprite sheet (sx, sy, sw, sh) (given in pixels)
		/// and draw in rectangle(dx, dy, dw, dh)
        /// Colour 0 drawn as transparent by default (see palt())
        /// </summary>
        /// <param name="sx">The starting pixel x position in the spritesheet to draw from.</param>
        /// <param name="sy">The starting pixel y position in the spritesheet to draw from.</param>
        /// <param name="sw">The width to draw from the spritesheet.</param>
        /// <param name="sh">The height to draw from the spritesheet.</param>
        /// <param name="dx">The x position to draw the spritesheet on the screen.</param>
        /// <param name="dy">The y position to draw the spritesheet on the screen.</param>
        /// <param name="dw">The width of the screen rectangle that the spritesheet section is drawn to. Defaults to sw.</param>
        /// <param name="dh">The height of the screen rectangle that the spritesheet section is drawn to. Defaults to sh.</param>
        /// <param name="flip_x">If it should flip horizontally</param>
        /// <param name="flip_y">If it should flip vertically</param>
        /// <returns>Returns null everytime</returns>
        public object Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int? dw = null, int? dh = null, bool? flip_x = null, bool? flip_y = null)
        {
            if (!dw.HasValue)
            {
                dw = sw;
            }
            if (!dh.HasValue)
            {
                dh = sh;
            }
            if (!flip_x.HasValue)
            {
                flip_x = false;
            }
            if (!flip_y.HasValue)
            {
                flip_y = false;
            }

            float ratioX = (float)sw / (float)dw.Value;
            float ratioY = (float)sh / (float)dh.Value;
            float x = sx;
            float y = sy;
            float screenX = dx;
            float screenY = dy;

            while (x < sx + sw && screenX < dx + dw)
            {
                y = sy;
                screenY = dy;
                while (y < sy + sh && screenY < dy + dh)
                {
                    byte sprColor = Sget((int)x, (int)y);
                    Psett((flip_x.Value ? dx + dw.Value - ((int)screenX - dx) : (int)screenX),
                          (flip_y.Value ? dy + dh.Value - ((int)screenY - dy) : (int)screenY),
                          sprColor);

                    y += ratioY;
                    screenY += 1;
                }
                x += ratioX;
                screenX += 1;
            }

            return null;
        }

        /// <summary>
        /// Get color of a spritesheet pixel.
        /// </summary>
        /// <param name="x">The x position in the spritesheet to extract the pixel from.</param>
        /// <param name="y">The y position in the spritesheet to extract the pixel from.</param>
        /// <returns>The color value.</returns>
        public byte Sget(int x, int y)
        {
            return memory.GetPixel(x, y, MemoryUnit.ADDR_GFX);
        }

        /// <summary>
        /// Sets color of a spritesheet pixel.
        /// </summary>
        /// <param name="x">The x position in the spritesheet where the pixel will be set.</param>
        /// <param name="y">The y position in the spritesheet where the pixel will be set.</param>
        /// <param name="col">The color value to set the pixel to.</param>
        /// <returns>Returns null everytime.</returns>
        public object Sset(int x, int y, byte? col = null)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            memory.WritePixel(x, y, memory.DrawColor, MemoryUnit.ADDR_GFX);

            return null;
        }

        /// <summary>
        /// Sets a pixel value to the screen.
        /// </summary>
        /// <param name="x">The x position on the screen.</param>
        /// <param name="y">The y position on the screen.</param>
        /// <param name="col">The color value to set the pixel to.</param>
        /// <returns>Returns null everytime.</returns>
        public object Pset(int x, int y, byte? col = null)
        {
            x -= memory.cameraX;
            y -= memory.cameraY;
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            int f = memory.getFillPBit(x, y);
            if (f == 0)
            {
                // Do not consider transparency bit for this operation.
                memory.WritePixel(x, y, (byte)(memory.GetDrawColor(memory.DrawColor & 0x0f) & 0x0f));
            }
            else if (!memory.fillpTransparent)
            {
                // Do not consider transparency bit for this operation.
                memory.WritePixel(x, y, (byte)(memory.GetDrawColor(memory.DrawColor >> 4) & 0x0f));
            }

            return null;
        }

        /// <summary>
        /// Set pixel considering transparency value. Used for spr, sspr and map.
        /// </summary>
        /// <param name="x">The x position on the screen.</param>
        /// <param name="y">The y position on the screen.</param>
        /// <param name="col">The color value to set the pixel to.</param>
        /// <returns>Returns null everytime.</returns>
        private object Psett(int x, int y, byte? col = null)
        {
            x -= memory.cameraX;
            y -= memory.cameraY;
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            int f = memory.getFillPBit(x, y);
            if (f == 0)
            {
                memory.WritePixel(x, y, memory.GetDrawColor(memory.DrawColor & 0x0f));
            }
            else if (!memory.fillpTransparent)
            {
                memory.WritePixel(x, y, (memory.GetDrawColor(memory.DrawColor >> 4)));
            }

            return null;
        }

        /// <summary>
        /// Gets the color of a pixel.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>Returns null everytime.</returns>
        public byte Pget(int x, int y)
        {
            return memory.GetPixel((int)x, (int)y);
        }

        /// <summary>
        /// The Palt
        /// </summary>
        /// <param name="col">The col</param>
        /// <param name="t">The t</param>
        /// <returns>The </returns>
        public object Palt(int? col = null, bool? t = null)
        {
            if (!col.HasValue || !t.HasValue)
            {
                memory.SetTransparent(0);
                for (byte i = 1; i < 16; i++)
                {
                    memory.ResetTransparent(i);
                }
                return null;
            }

            if (t.Value)
            {
                memory.SetTransparent(col.Value);
            }
            else
            {
                memory.ResetTransparent(col.Value);
            }

            return null;
        }

        /// <summary>
        /// Draw all instances of colour c0 as c1 in subsequent draw calls.
        /// pal() to reset to system defaults (including transparency values and fill pattern).
        /// Two types of palette (p; defaults to 0):
        ///     0 draw palette   : colours are remapped on draw    (e.g. to re-colour sprites)
        ///     1 screen palette : colours are remapped on display (e.g. for fades)
        /// </summary>
        /// <param name="c0">The color to change.</param>
        /// <param name="c1">The color to change to.</param>
        /// <param name="p">The palette to make the change to. Defaults to 0.</param>
        /// <returns>Returns null everytime.</returns>
        public object Pal(int? c0 = null, int? c1 = null, int p = 0)
        {
            if (!c0.HasValue || !c1.HasValue)
            {
                for (byte i = 0; i < 16; i++)
                {
                    memory.SetDrawPalette(i, i);
                    memory.SetScreenPalette(i, i);
                }
                Palt(null, null);

                return null;
            }

            if (p == 0)
            {
                memory.SetDrawPalette(c0.Value, c1.Value);
            }
            else if (p == 1)
            {
                memory.SetScreenPalette(c0.Value, c1.Value);
            }

            return null;
        }

        /// <summary>
        /// Sets the screen's clipping region in pixels.
        /// clip() to reset.
        /// </summary>
        /// <param name="x">The x position of the clipping rectangle</param>
        /// <param name="y">The y position of the clipping rectangle</param>
        /// <param name="w">The width of the clipping rectangle</param>
        /// <param name="h">The height of the clipping rectangle</param>
        /// <returns>Returns null everytime.</returns>
        public object Clip(int? x = null, int? y = null, int? w = null, int? h = null)
        {
            if (!x.HasValue || !y.HasValue || !w.HasValue || !h.HasValue)
            {
                return null;
            }

            memory.clipX0 = (byte)x.Value;
            memory.clipY0 = (byte)y.Value;
            memory.clipX1 = (byte)(x.Value + w.Value);
            memory.clipY1 = (byte)(y.Value + h.Value);

            return null;
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="x0">The x0 position of the rectangle</param>
        /// <param name="y0">The y0 position of the rectangle</param>
        /// <param name="x1">The x1 position of the rectangle</param>
        /// <param name="y1">The y1 position of the rectangle</param>
        /// <param name="col">The color of the rectangle</param>
        /// <returns>returns null everytime.</returns>
        public object Rect(int x0, int y0, int x1, int y1, byte? col = null)
        {
            Line(x0, y0, x1, y0, col);
            Line(x0, y0, x0, y1, col);
            Line(x1, y1, x1, y0, col);
            Line(x1, y1, x0, y1, col);

            return null;
        }

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="x0">The x0 position of the rectangle</param>
        /// <param name="y0">The y0 position of the rectangle</param>
        /// <param name="x1">The x1 position of the rectangle</param>
        /// <param name="y1">The y1 position of the rectangle</param>
        /// <param name="col">The color of the rectangle</param>
        /// <returns>returns null everytime.</returns>
        public object Rectfill(int x0, int y0, int x1, int y1, byte? col = null)
        {
            if (y0 > y1)
            {
                util.Swap(ref y0, ref y1);
            }

            for (int y = y0; y < y1; y++)
            {
                Line(x0, y, x1, y, col);
            }

            return null;
        }

        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="x0">The x0 position of the line</param>
        /// <param name="y0">The y0 position of the line</param>
        /// <param name="x1">The x1 position of the line</param>
        /// <param name="y1">The y1 position of the line</param>
        /// <param name="col">The color of the line</param>
        /// <returns>returns null everytime.</returns>
        public object Line(int x0, int y0, int? x1 = null, int? y1 = null, byte? col = null)
        {
            if (x1.HasValue)
            {
                memory.lineX = x1.Value;
            }

            if (y1.HasValue)
            {
                memory.lineY = y1.Value;
            }

            int x0_screen = x0;
            int y0_screen = y0;
            int x1_screen = memory.lineX;
            int y1_screen = memory.lineY;

            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            bool steep = false;
            if (Math.Abs(x1_screen - x0_screen) < Math.Abs(y1_screen - y0_screen))
            {
                util.Swap(ref x0_screen, ref y0_screen);
                util.Swap(ref x1_screen, ref y1_screen);
                steep = true;
            }
            if (x0_screen > x1_screen)
            {
                util.Swap(ref x0_screen, ref x1_screen);
                util.Swap(ref y0_screen, ref y1_screen);
            }

            int dx = (int)(x1_screen - x0_screen);
            int dy = (int)(y1_screen - y0_screen);
            int d_err = 2 * Math.Abs(dy);
            int err = 0;
            int y = (int)y0_screen;

            for (int x = (int)x0_screen; x <= x1_screen; x++)
            {
                if (steep)
                {
                    Pset(y, x, null);
                }
                else
                {
                    Pset(x, y, null);
                }

                err += d_err;

                if (err > dx)
                {
                    y += y1_screen > y0_screen ? 1 : -1;
                    err -= dx * 2;
                }
            }

            return null;
        }

        /// <summary>
        /// Draws a circle
        /// </summary>
        /// <param name="x">The x position of the circle.</param>
        /// <param name="y">The y position of the circle.</param>
        /// <param name="r">The radius of the circle</param>
        /// <param name="col">The color to draw the circle.</param>
        /// <returns>Returns null everytime.</returns>
        public object Circ(int x, int y, double r, byte? col = null)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)Math.Ceiling(r), false);

            return null;
        }

        /// <summary>
        /// Draws a filled circle.
        /// </summary>
        /// <param name="x">The x position of the circle.</param>
        /// <param name="y">The y position of the circle.</param>
        /// <param name="r">The radius of the circle</param>
        /// <param name="col">The color to draw the circle.</param>
        /// <returns>Returns null everytime.</returns>
        public object CircFill(int x, int y, double r, byte? col = null)
        {
            if (col.HasValue)
            {
                memory.DrawColor = col.Value;
            }

            DrawCircle(x, y, (int)r, true);

            return null;
        }

        private void plot4(int x, int y, int offX, int offY, bool fill)
        {
            if (fill)
            {
                Line((x - offX), (y + offY), (x + offX), (y + offY), null);
                if (offY != 0)
                {
                    Line((x - offX), (y - offY), (x + offX), (y - offY), null);
                }
            }
            else
            {
                Pset((x - offX), (y + offY), null);
                Pset((x + offX), (y + offY), null);
                if (offY != 0)
                {
                    Pset((x - offX), (y - offY), null);
                    Pset((x + offX), (y - offY), null);
                }
            }
        }

        private void DrawCircle(int posX, int posY, int r, bool fill)
        {
            int cx = posX, cy = posY;
            int x = r;
            int y = 0;
            double err = 1 - r;

            while (y <= x)
            {

                plot4(posX, posY, x, y, fill);

                if (err < 0)
                {
                    err = err + 2 * y + 3;
                }
                else
                {
                    if (x != y)
                    {
                        plot4(posX, posY, y, x, fill);
                    }
                    x = x - 1;
                    err = err + 2 * (y - x) + 3;
                }

                y = y + 1;
            }
        }
    }
}
