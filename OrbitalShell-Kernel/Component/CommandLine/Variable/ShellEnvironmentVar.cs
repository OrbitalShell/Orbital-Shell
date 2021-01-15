namespace DotNetConsoleAppToolkit.Component.CommandLine.Variable
{
    /// <summary>
    /// standard shell environment namespaces
    /// </summary>
    public enum ShellEnvironmentVar
    {
        /// <summary>
        /// if enabled, activate codes that try to fix bad known console behaviors (eg. VSCode)
        /// </summary>
        Settings_EnableConsoleCompatibilityMode,

        /// <summary>
        /// initial console width
        /// </summary>
        Settings_ConsoleInitialWindowWidth,

        /// <summary>
        /// initial console height
        /// </summary>
        Settings_ConsoleInitialWindowHeight,

        /// <summary>
        /// if enabled, activate a fix that cleanup properly end of lines (ConsoleTextWrapper.LNBRK)
        /// </summary>
        Settings_EnableAvoidEndOfLineFilledWithBackgroundColor,

        Debug_Pipeline,
        Display_TableFormattingOptions,
        Display_FileSystemPathFormattingOptions,
        Display_Colors_ColorSettings,
        UserPath,

        // traditional bash variables

        HOME,
        // reference to the name 'orbsh' is impossible here
        SHELL,
        SHELL__VERSION,
        SHELL__NAME,
        SHELL__LONG__NAME,
        SHELL__EDITOR,
        SHELL__LICENSE,
        PS1,
        PS2,
        PS3,
        PS4
    }
}
