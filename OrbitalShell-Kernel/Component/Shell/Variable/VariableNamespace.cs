namespace OrbitalShell.Component.Shell.Variable
{
    public enum VariableNamespace
    {
        /// <summary>
        /// environment (shell variables)
        /// </summary>
        env,

        /// <summary>
        /// local to the command context (user variables)
        /// </summary>
        local,

        /// <summary>
        /// global (shell variables)
        /// </summary>
        global,

        /// <summary>
        /// empty name space (shell contextual variables)
        /// </summary>
        _
    }
}
