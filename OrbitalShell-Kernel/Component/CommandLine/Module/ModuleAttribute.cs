using System;

namespace OrbitalShell.Component.CommandLine.Module
{
    /// <summary>
    /// this attribute when applied to an assembly declares it as a shell module. Then it can take part of the shell module actions (load/unload/install/uninstall)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleAttribute : Attribute
    {
        public string ShellMinVersion;

        public ModuleAttribute(string shellMinVersion) {
            ShellMinVersion = shellMinVersion;
        }
    }
}