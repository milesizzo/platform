#define WIN64

using System;

namespace Platform
{
#if WINDOWS || LINUX || WIN64
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new PlatformGame())
                game.Run();
        }
    }
#endif
}
