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

        ///
        /// current prompt text
        ///        
        settings_prompt,

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

        home,
        shell,
        SHELL__VERSION,
        SHELL__NAME,
        SHELL__LONG__NAME,
        SHELL__EDITOR,
        SHELL__LICENSE,

        // traditional bash special variables

        sp__lastCommandReturnCode = '?',
        lastComReturnCode,
        
        /// <summary>
        /// 💥 emoji active shell pid: should be '$' , but can't due to parser failure. so currently is '#'
        /// </summary>
        sp__activeShellPID = '#',
        activeShellPID,

        sp__lastTaskID = '!',
        lastTaskID,

        /// <summary>
        /// 💥 shell options: should be '-' , but can't due to parser failure. so currently is '°'
        /// </summary>
        sp__shellOpts = '°',
        shellOpts,

        // batch args : @TODO: move in Local env

        sp__arg0 = '0',
        sp__arg1 = '1',
        sp__arg2 = '2',
        sp__arg3 = '3',
        sp__arg4 = '4',
        sp__arg5 = '5',
        sp__arg6 = '6',
        sp__arg7 = '7',
        sp__arg8 = '8',
        sp__arg9 = '9'
    }
}
