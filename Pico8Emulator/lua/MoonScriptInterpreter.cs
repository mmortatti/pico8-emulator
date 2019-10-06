using System;
using MoonSharp.Interpreter;

namespace Pico8Emulator.lua {
	public class MoonScriptInterpreter : LuaInterpreter {
		private Script script = new Script();
		
		public void AddFunction(string name, object func) {
			script.Globals[name] = func;
		}

		public void CallFunction(string name) {
			try {
				script.Call(name);
			} catch (Exception e) {
				Log.Error(e.Message);
			}
		}

		public bool CallIfDefined(string name) {
			if (IsDefined(name)) {
				try {
					script.Call(script.Globals[name]);
				} catch (Exception e) {
					Log.Error(e.Message);
				}
				
				return true;
			}

			return false;
		}

		public void RunScript(string str) {
			try {
				script.DoString(str);
			} catch (Exception e) {
				Log.Error(e.Message);
			}
		}

		public bool IsDefined(string name) {
			return script.Globals[name] != null;
		}
	}
}