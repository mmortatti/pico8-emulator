
namespace pico8_interpreter.Pico8
{
    public interface ILuaInterpreter
    {
        void AddFunction(string name, object func);
        void CallFunction(string name);
        bool CallIfDefined(string name);
        void RunScript(string script);
        bool IsDefined(string name);
    }
}
