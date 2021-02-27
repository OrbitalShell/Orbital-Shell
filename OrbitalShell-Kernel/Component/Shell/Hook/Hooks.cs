using System;

namespace OrbitalShell.Component.Shell.Hook
{
    /// <summary>
    /// shell named hooks points for extensivity throught hook system
    /// </summary>
    public enum Hooks
    {
        /// <summary>
        /// shell has just terminate Initialize()
        /// </summary>
        ShellInitialized,

        /// <summary>
        /// module init (module load)
        /// </summary>
        ModuleInit,

        /// <summary>
        /// prompt output begin
        /// </summary>
        PromptOutputBegin,

        /// <summary>
        /// prompt output end
        /// </summary>
        PromptOutputEnd,

        /// <summary>
        /// just before task exec of command line eval within command line reader
        /// </summary>
        PreProcessCommandLine,

        /// <summary>
        /// just after exec of command line eval within command line reader
        /// </summary>
        PostProcessCommandLine,

        /// <summary>
        /// error while processing command line within command line reader
        /// </summary>
        ProcessCommandLineError,

        /// <summary>
        /// user cancellation (CTRL+C) while processing command line within command line reader
        /// </summary>
        ProcessCommandLineCanceled,

        /// <summary>
        /// just after something is added to command history
        /// </summary>
        PostHistoryAppend,

        /// <summary>
        /// just after history is cleared
        /// </summary>
        ClearHistory,

        /// <summary>
        /// starts reading command line user input
        /// </summary>
        BeginReadCommandLine,

        /// <summary>
        /// a key was pressed while command line user input
        /// </summary>
        ReadCommandLineKeyPressed,

        /// <summary>
        /// clr enter
        /// </summary>
        ReadCommandLineEnterPressed,

        /// <summary>
        /// clr esc
        /// </summary>
        ReadCommandLineEscPressed,

        /// <summary>
        /// clr home
        /// </summary>
        ReadCommandLineHomePressed,

        /// <summary>
        /// clr end
        /// </summary>
        ReadCommandLineEndPressed,

        /// <summary>
        /// clr tab
        /// </summary>
        ReadCommandLineTabPressed,

        /// <summary>
        /// clr left arrow
        /// </summary>
        ReadCommandLineLeftArrowPressed,

        /// <summary>
        /// clr right arrow
        /// </summary>
        ReadCommandLineRightArrowPressed,

        /// <summary>
        /// clr up arrow
        /// </summary>
        ReadCommandLineUpArrowPressed,

        /// <summary>
        /// clr down arrow
        /// </summary>
        ReadCommandLineDownArrowPressed,

        /// <summary>
        /// clr backspace
        /// </summary>
        ReadCommandLineBackspacePressed,

        /// <summary>
        /// clr delete
        /// </summary>
        ReadCommandLineDeletePressed,
    }
}