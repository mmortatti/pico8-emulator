using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    public interface ILuaInterpreter
    {
        void AddFunction(string name, object func);
        void CallFunction(string name);
        void CallIfDefined(string name);
        void RunScript(string script);
        bool IsDefined(string name);
    }
}
