namespace OrbitalShell.Component.CommandLine.Variable
{
    /// <summary>
    /// standard shell environment namespaces
    /// </summary>
    public enum ShellEnvironmentNamespace
    {
        /// <summary>
        /// os environment (underlying shell/os)
        /// </summary>
        os,

        /// <summary>
        /// shell settings
        /// </summary>
        settings,

        /// <summary>
        /// CommandSettings
        /// </summary>        
        commandsSettings,

        /// <summary>
        /// debug settings
        /// </summary>
        debug,

        /// <summary>
        /// display settings
        /// </summary>
        display,

        /// <summary>
        /// it may be possible to find games
        /// </summary>
        games

    }
}
