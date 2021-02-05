using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// declares a dependency to an other module
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleDependencyAttribute : Attribute
    {
        public readonly string ModuleName;
        public readonly string ModuleMinVersion;

        public ModuleDependencyAttribute(string moduleName, string moduleMinVersion)
        {
            ModuleName = moduleName;
            ModuleMinVersion = moduleMinVersion;
        }
    }
}