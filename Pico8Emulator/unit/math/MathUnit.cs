using System;
using Pico8Emulator.lua;

namespace Pico8Emulator.unit.math {
	public class MathUnit : Unit {
		private Random random = new Random();
		
		public MathUnit(Emulator emulator) : base(emulator) {
			
		}
		
		public double Rnd(double? x = null) {
			if (!x.HasValue) {
				x = 1;
			}
			
			return random.NextDouble() * x.Value;
		}
		
		public object Srand(int x) {
			random = new Random(x);
			return null;
		}

		public override void DefineApi(LuaInterpreter script) {
			// script.AddFunction("abs", (Func<int, object>) a => a);
		}
	}
}