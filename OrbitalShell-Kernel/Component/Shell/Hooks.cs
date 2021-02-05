namespace OrbitalShell.Component.Shell
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
    }
}