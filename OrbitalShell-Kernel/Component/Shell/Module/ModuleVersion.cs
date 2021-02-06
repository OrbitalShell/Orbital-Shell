namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// module version
    /// </summary>
    public class ModuleVersion
    {
        public readonly string Version;

        public ModuleVersion(string version)
        {
            Version = version;
        }

        public override string ToString()
        {
            return Version;
        }
    }
}