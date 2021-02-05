using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// declare dependency to shell min-version
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleShellMinVersionAttribute : Attribute
    {
        public readonly string ShellMinVersion;

        public ModuleShellMinVersionAttribute(string shellMinVersion)
        {
            ShellMinVersion = shellMinVersion;
        }
    }
}