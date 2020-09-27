namespace DotNetConsoleAppToolkit.Component.CommandLine.Variable
{
    /// <summary>
    /// standard shell environment namespaces
    /// </summary>
    public enum ShellEnvironmentVar
    {
        Debug_Pipeline,
        Display_TableFormattingOptions,
        Display_FileSystemPathFormattingOptions,
        Display_Colors_ColorSettings,
        UserPath,

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
