using MoonSharp.Interpreter;
using System;

namespace Pico8Emulator.lua {
	public class MoonSharpInterpreter : LuaInterpreter {
		private Script script = new Script();
		private string latestScript;

		public void AddFunction(string name, object func) {
			script.Globals[name] = func;
		}

		public void CallFunction(string name) {
			try {
				script.Call(name);
			}
			catch (Exception e) {
				HandleError(e);
			}
		}

		public bool CallIfDefined(string name) {
			if (IsDefined(name)) {
				try {
					script.Call(script.Globals[name]);
				}
				catch (Exception e) {
					HandleError(e);
				}

				return true;
			}

			return false;
		}

		public void RunScript(string str) {
			try {
				latestScript = str;
				script.DoString(str);
			}
			catch (Exception e) {
				HandleError(e);
			}
		}

		private void HandleError(Exception e) {
			// Uncomment for debugging. Commented out to save my ssd
			// File.WriteAllText("log.txt", latestScript);

			if (e is MoonSharp.Interpreter.SyntaxErrorException se) {
				Log.Error(se.DecoratedMessage);
			}
			else if (e is MoonSharp.Interpreter.InterpreterException ie) {
				Log.Error(ie.DecoratedMessage);
			}
		}

		public bool IsDefined(string name) {
			return script.Globals[name] != null;
		}
	}
}