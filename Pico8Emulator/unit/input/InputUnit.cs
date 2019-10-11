using Pico8Emulator.lua;
using System;

namespace Pico8Emulator.unit.input {
	/*
	 * TODO: I removed mask implementation for simplicity, get it back!
	 */
	public class InputUnit : Unit {
		public const int ButtonCount = 6;
		public const int MaxPlayers = 8;
		public const int StateSize = ButtonCount * MaxPlayers;

		private bool[] _previousButtonState = new bool[StateSize];
		private bool[] _currentButtonState = new bool[StateSize];

		public InputUnit(Emulator emulator) : base(emulator) {

		}

		public override void DefineApi(LuaInterpreter script) {
			base.DefineApi(script);

			script.AddFunction("btn", (Func<int?, int?, bool>)Btn);
			script.AddFunction("btnp", (Func<int?, int?, bool>)Btnp);
		}

		public override void Update() {
			base.Update();

			for (var i = 0; i < ButtonCount; i++) {
				_previousButtonState[i] = _currentButtonState[i];
				_currentButtonState[i] = Emulator.InputBackend.IsButtonDown(i, 0);
			}
		}

		private static int ToIndex(int? i, int? p) {
			return (i ?? 0) + ((p ?? 0) * ButtonCount);
		}

		public bool Btn(int? i = null, int? p = null) {
			return _currentButtonState[ToIndex(i, p)];
		}

		public bool Btnp(int? i = null, int? p = null) {
			var index = ToIndex(i, p);
			return _currentButtonState[index] && !_previousButtonState[index];
		}
	}
}