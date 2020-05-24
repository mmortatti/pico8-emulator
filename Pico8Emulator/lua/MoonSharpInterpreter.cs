using MoonSharp.Interpreter;
using System;

namespace Pico8Emulator.lua {
	public class MoonSharpInterpreter : LuaInterpreter {
		private Script script;
		private string latestScript;

		public MoonSharpInterpreter()
		{
			script = new Script();

			script.Options.TailCallOptimizationThreshold = 0;
			System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
		}

		public void AddFunction(string name, object func) {
			script.Globals[name] = func;
		}

		public void CallFunction(string name) {
			try {
				script.Call(name);
			}
			catch (Exception e) {
				HandleError(e, name);
			}
		}

		public bool CallIfDefined(string name) {
			if (IsDefined(name)) {
				script.Call(script.Globals[name]);

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
				HandleError(e, "runscript()");
			}
		}

		private void HandleError(Exception e, string where) {
			// Uncomment for debugging. Commented out to save my ssd
			// File.WriteAllText("log.txt", latestScript);

			if (e is MoonSharp.Interpreter.SyntaxErrorException se) {
				Log.Error($"{@where}: {se.DecoratedMessage}");
			}
			else if (e is MoonSharp.Interpreter.InterpreterException ie) {
				Log.Error($"{@where}: {ie.DecoratedMessage}");
			}
		}

		public bool IsDefined(string name) {
			return script.Globals[name] != null;
		}
	}
}