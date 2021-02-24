namespace OrbitalShell.Component.Shell.Module.Data
{
    /// <summary>
    /// reference of a module in a repository
    /// </summary>
    public class ModuleReference
    {
        public string Type;

        public string ModuleId;

        public ModuleVersion Version
            => new ModuleVersion(LastKnownVersion);

        public string LastKnownVersion;

        public string Description;

        public ModuleReference() { }

        public ModuleReference(
            string moduleId, 
            string version, 
            string description)
        {
            ModuleId = moduleId;
            Description = description;
            LastKnownVersion = version;
        }

        public override string ToString()
        {
            return $"{ModuleId} - {Description} - Version = {Version}";
        }
    }
}