namespace pico8_interpreter
{
    using System;

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
