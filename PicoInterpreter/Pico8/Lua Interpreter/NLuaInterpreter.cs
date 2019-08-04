using NLua;
using System;

namespace pico8_interpreter.Pico8
{
    class NLuaInterpreter : ILuaInterpreter
    {
        Lua scriptInterpreter;
        public NLuaInterpreter()
        {
            scriptInterpreter = new Lua();
            scriptInterpreter.LoadCLRPackage();
        }

        public void AddFunction(string name, object func)
        {
            scriptInterpreter[name] = func;
        }

        public void CallFunction(string name)
        {
            (scriptInterpreter[name] as LuaFunction).Call();
        }

        public bool CallIfDefined(string name)
        {
            if (IsDefined(name))
            {
                CallFunction(name);
                return true;
            }

            return false;
        }

        public bool IsDefined(string name)
        {
            return scriptInterpreter[name] != null;
        }

        public void RunScript(string script)
        {
            scriptInterpreter.DoString(script);
        }
    }
}
