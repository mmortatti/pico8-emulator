namespace pico8_interpreter.Pico8
{
    using NLua;

    /// <summary>
    /// Implements the interface for the NLua interpreter. <see cref="NLuaInterpreter" />
    /// </summary>
    internal class NLuaInterpreter : ILuaInterpreter
    {
        /// <summary>
        /// Defines an instace of the NLua Interpreter.
        /// </summary>
        internal Lua scriptInterpreter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLuaInterpreter"/> class.
        /// </summary>
        public NLuaInterpreter()
        {
            scriptInterpreter = new Lua();
            scriptInterpreter.LoadCLRPackage();
        }

        /// <summary>
        /// The AddFunction
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="func">The func<see cref="object"/></param>
        public void AddFunction(string name, object func)
        {
            scriptInterpreter[name] = func;
        }

        /// <summary>
        /// The CallFunction
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        public void CallFunction(string name)
        {
            (scriptInterpreter[name] as LuaFunction).Call();
        }

        /// <summary>
        /// The CallIfDefined
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool CallIfDefined(string name)
        {
            if (IsDefined(name))
            {
                CallFunction(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// The IsDefined
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsDefined(string name)
        {
            return scriptInterpreter[name] != null;
        }

        /// <summary>
        /// The RunScript
        /// </summary>
        /// <param name="script">The script<see cref="string"/></param>
        public void RunScript(string script)
        {
            scriptInterpreter.DoString(script);
        }
    }
}
