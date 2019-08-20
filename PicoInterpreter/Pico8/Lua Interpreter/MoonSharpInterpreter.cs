namespace pico8_interpreter.Pico8
{
    using MoonSharp.Interpreter;
    using MoonSharp.RemoteDebugger;
    using System.Diagnostics;

    /// <summary>
    /// Implements the interface for the Moonsharp interpreter. <see cref="MoonSharpInterpreter" />.
    /// </summary>
    internal class MoonSharpInterpreter : ILuaInterpreter
    {
        /// <summary>
        /// Defines the a script in the Moonsharp interpreter.
        /// </summary>
        private Script scriptInterpreter;

        /// <summary>
        /// Defines the debugger for Moonsharp.
        /// </summary>
        internal RemoteDebuggerService remoteDebugger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoonSharpInterpreter"/> class.
        /// </summary>
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

        public bool CallIfDefined(string name)
        {
            if (IsDefined(name))
            {
                var func = scriptInterpreter.Globals[name];
                scriptInterpreter.Call(func);
                return true;
            }

            return false;
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
