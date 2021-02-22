using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// this attribute when applied to an assembly declares it as a shell module. Then it can take part of the shell module actions (load/unload/install/uninstall)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ShellModuleAttribute : Attribute
    {
        public readonly string PackageId;

        /// <summary>
        /// declare the assembly beeing a shell module having the specified package ID (must match PackageID in .csproj)
        /// </summary>
        /// <param name="packageId"></param>
        public ShellModuleAttribute(string packageId) { PackageId = packageId; }
    }
}
