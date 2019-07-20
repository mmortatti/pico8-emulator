using System.Diagnostics;
using MoonSharp.Interpreter;
using MoonSharp.RemoteDebugger;

namespace pico8_interpreter.Pico8
{
    class MoonSharpInterpreter : ILuaInterpreter
    {
        private Script scriptInterpreter;
        RemoteDebuggerService remoteDebugger;
        
        public MoonSharpInterpreter()
        {
            scriptInterpreter = new Script();
            //ActivateRemoteDebugger(scriptInterpreter);
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
                var func = scriptInterpreter.Globals[name];
                scriptInterpreter.Call(func);
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

        private void ActivateRemoteDebugger(Script script)
        {
            if (remoteDebugger == null)
            {
                remoteDebugger = new RemoteDebuggerService();

                // the last boolean is to specify if the script is free to run 
                // after attachment, defaults to false
                remoteDebugger.Attach(script, "Description of the script", false);
            }

            // start the web-browser at the correct url. Replace this or just
            // pass the url to the user in some way.
            Process.Start(remoteDebugger.HttpUrlStringLocalHost);
        }
    }
}
