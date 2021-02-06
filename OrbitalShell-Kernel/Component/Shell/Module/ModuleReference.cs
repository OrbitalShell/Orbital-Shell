namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// reference of a module in a repository
    /// </summary>
    public class ModuleReference
    {
        public readonly string Name;

        public ModuleVersion Version;

        public string Description;

        public ModuleReference(string name, string version, string description)
        {
            Name = name;
            Description = description;
            Version = new ModuleVersion(version);
        }

        public override string ToString()
        {
            return $"{Name} - {Description} - Version = {Version}";
        }
    }
}