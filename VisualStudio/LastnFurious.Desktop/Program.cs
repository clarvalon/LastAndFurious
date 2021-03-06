using System;
using Clarvalon.XAGE.dll;
using Clarvalon.XAGE.Global;

namespace XAGE_EXE
{
    public static class Program
    {
#if RELEASE
        public static bool Release = true; 
        public static bool AllowDebugging = true;
        public static int LogLevel = 0;
        public static bool LogAsync = true;
        public static bool CatchFatalExceptions = true;
        public static NotImplementedAction OnNotImplemented = NotImplementedAction.Ignore;
#else
        public static bool Release = false;
        public static bool AllowDebugging = true;
        public static int LogLevel = 0;
        public static bool LogAsync = false;
        public static bool CatchFatalExceptions = false;
        public static NotImplementedAction OnNotImplemented = NotImplementedAction.Warn;
#endif

        /// <summary>
        /// The main entry point for the application.  This instantiates and runs our game (within XAGE.Engine.Core.dll)
        /// </summary>
        static void Main(string[] args)
        {
            // Must do this before anything else
            DllMap.Initialise();

            // Create Script Delegates that get injected into the engine
            RunSettings runSettings = new RunSettings(
                Release,
                AllowDebugging,
                LogLevel,
                LogAsync,
                OnNotImplemented,
                LastnFurious.CSharpScript.Load,
                LastnFurious.CSharpScript.Serialize,
                LastnFurious.CSharpScript.Deserialize,
                typeof(LastnFurious.CSharpScript));

            // Run the game - any unhandled exceptions will be caught in Release mode only
            // In Debug mode it will stop in place, which is generally more useful for debugging
            try
            {
                using (XNAGame game = new XNAGame(runSettings))
                {
                    game.Run();
                }
            }
            catch (Exception exc) when (CatchFatalExceptions)
            {
                Logger.Fatal("Unhandled exception: " + exc.Message);
                Logger.Fatal("StackTrace: " + exc.StackTrace);
            }
            finally
            {
                Logger.Reset();
            }
        }
    }
}
