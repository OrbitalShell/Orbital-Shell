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
        settings_enableConsoleCompatibilityMode,

        /// <summary>
        /// initial console width
        /// </summary>
        settings_consoleInitialWindowWidth,

        /// <summary>
        /// initial console height
        /// </summary>
        settings_consoleInitialWindowHeight,

        /// <summary>
        /// if enabled, activate a fix that cleanup properly end of lines (ConsoleTextWrapper.LNBRK)
        /// </summary>
        settings_enableAvoidEndOfLineFilledWithBackgroundColor,

        debug_pipeline,
        display_tableFormattingOptions,
        display_fileSystemPathFormattingOptions,
        display_colors_colorSettings,
        userProfile,

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
