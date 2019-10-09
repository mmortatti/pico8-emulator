using System;
using Pico8Emulator.lua;

namespace Pico8Emulator.unit.input {
	/*
	 * TODO: I removed mask implementation for simplicity, get it back!
	 */
	public class InputUnit : Unit {
		public const int ButtonCount = 6;
		public const int MaxPlayers = 8;
		public const int StateSize = ButtonCount * MaxPlayers;

		private bool[] previousButtonState = new bool[StateSize];
		private bool[] currentButtonState = new bool[StateSize];
		
		public InputUnit(Emulator emulator) : base(emulator) {
			
		}

		public override void DefineApi(LuaInterpreter script) {
			base.DefineApi(script);
			
			script.AddFunction("btn", (Func<int?, int?, bool>) Btn);
			script.AddFunction("btnp", (Func<int?, int?, bool>) Btnp);
		}

		public override void Update() {
			base.Update();

			for (var i = 0; i < ButtonCount; i++) {
				previousButtonState[i] = currentButtonState[i];
				currentButtonState[i] = Emulator.InputBackend.IsButtonDown(i, 0);
			}
		}

		private static int ToIndex(int? i, int? p) {
			return (i ?? 0) + ((p ?? 0) * ButtonCount);
		}
		
		public bool Btn(int? i = null, int? p = null) {
			return currentButtonState[ToIndex(i, p)];
		}

		public bool Btnp(int? i = null, int? p = null) {
			var index = ToIndex(i, p);
			return currentButtonState[index] && !previousButtonState[index];
		}
	}
}