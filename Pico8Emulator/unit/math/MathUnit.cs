using System;
using Pico8Emulator.lua;

namespace Pico8Emulator.unit.math {
	public class MathUnit : Unit {
		private Random random = new Random();
		
		public MathUnit(Emulator emulator) : base(emulator) {
			
		}

		public override void DefineApi(LuaInterpreter script) {
			script.AddFunction("rnd", (Func<double?, double>) Rnd);
			script.AddFunction("srand", (Action<int>) Srand);

			script.AddFunction("flr", (Func<double, double>) Flr);
			script.AddFunction("max", (Func<double, double, double>) Max);
			script.AddFunction("min", (Func<double, double, double>) Min);
			script.AddFunction("mid", (Func<double, double, double, double>) Mid);
			script.AddFunction("abs", (Func<double, double>) Abs);
			script.AddFunction("sqrt", (Func<double, double>) Sqrt);
			
			script.AddFunction("cos", (Func<double, double>) Cos);
			script.AddFunction("sin", (Func<double, double>) Sin);
			script.AddFunction("atan2", (Func<double, double, double>) Atan2);
			
			script.AddFunction("band", (Func<float, float, double>) Band);
			script.AddFunction("bor", (Func<float, float, double>) Bor);
			script.AddFunction("bxor", (Func<float, float, double>) Bxor);
			script.AddFunction("bnot", (Func<float, double>) Bnot);
			script.AddFunction("shl", (Func<float, float, double>) Shl);
			script.AddFunction("shr", (Func<float, float, double>) Shr);
		}
		
		public double Rnd(double? x = null) {
			if (!x.HasValue) {
				x = 1;
			}
			
			return random.NextDouble() * x.Value;
		}
		
		public void Srand(int x) {
			random = new Random(x);
		}

		public double Flr(double x) {
			return Math.Floor(x);
		}

		public double Max(double a, double b) {
			return a > b ? a : b;
		}

		public double Min(double a, double b) {
			return a < b ? a : b;
		}

		public double Mid(double a, double b, double c) {
			return Max(Min(Max(a, b), c), Min(a, b));
		}

		public double Abs(double a) {
			return a >= 0 ? a : -a;
		}

		public double Sqrt(double a) {
			return a >= 0 ? a : -a;
		}

		public double Cos(double a) {
			return Math.Cos(2 * a * Math.PI);
		}

		public double Sin(double a) {
			return -Math.Sin(2 * a * Math.PI);
		}

		public double Atan2(double dx, double dy) {
			return 1 - Math.Atan2(dy, dx) / (2 * Math.PI);
		}

		public double Band(float x, float y) {
			return Util.FixedToFloat(Util.FloatToFixed(x) & Util.FloatToFixed(y));
		}

		public double Bor(float x, float y) {
			return Util.FixedToFloat(Util.FloatToFixed(x) | Util.FloatToFixed(y));
		}

		public double Bxor(float x, float y) {
			return Util.FixedToFloat(Util.FloatToFixed(x) ^ Util.FloatToFixed(y));
		}

		public double Bnot(float x) {
			return Util.FixedToFloat(~Util.FloatToFixed(x));
		}

		public double Shl(float x, float y) {
			return Util.FixedToFloat(Util.FloatToFixed(x) << Util.FloatToFixed(y));
		}

		public double Shr(float x, float y) {
			return Util.FixedToFloat(Util.FloatToFixed(x) >> Util.FloatToFixed(y));
		}
	}
}