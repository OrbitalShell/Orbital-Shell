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

#if no
        /// <summary>
        /// if enabled, avoid to set the background colors in the command line reader component and the app-console layer, to preserve console background transparency
        /// </summary>
        Settings_EnableConsoleBackgroundTransparentMode,
#endif

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
