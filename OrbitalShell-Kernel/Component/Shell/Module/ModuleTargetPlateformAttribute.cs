using System;
using OrbitalShell.Lib;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// declares a dependency to an other module
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleTargetPlateformAttribute : Attribute
    {
        public readonly TargetPlatform TargetPlateform;

        public ModuleTargetPlateformAttribute(TargetPlatform targetPlateform)
        {
            TargetPlateform = targetPlateform;
        }
    }
}