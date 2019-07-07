using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace pico8_interpreter.Pico8
{
    class MoonSharpInterpreter : ILuaInterpreter
    {
        private Script scriptInterpreter;
        public MoonSharpInterpreter()
        {
            scriptInterpreter = new Script();
        }

        public void AddFunction(string name, object func)
        {
            scriptInterpreter.Globals[name] = func;
        }

        public void CallFunction(string name)
        {
            scriptInterpreter.Call(name);
        }

        public void CallIfDefined(string name)
        {
            if (IsDefined(name))
            {
                scriptInterpreter.Call(scriptInterpreter.Globals[name]);
            }
        }

        public bool IsDefined(string name)
        {
            return scriptInterpreter.Globals[name] != null;
        }

        public void RunScript(string script)
        {
            scriptInterpreter.DoString(script);
        }
    }
}
