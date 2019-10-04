using MoonSharp.Interpreter;

namespace Pico8Emulator.lua {
	public class MoonScriptInterpreter : LuaInterpreter {
		private Script script = new Script();
		
		public void AddFunction(string name, object func) {
			script.Globals[name] = func;
		}

		public void CallFunction(string name) {
			script.Call(name);
		}

		public bool CallIfDefined(string name) {
			if (IsDefined(name)) {
				script.Call(script.Globals[name]);
				return true;
			}

			return false;
		}

		public void RunScript(string str) {
			script.DoString(str);
		}

		public bool IsDefined(string name) {
			return script.Globals[name] != null;
		}
	}
}