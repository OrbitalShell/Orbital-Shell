using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Lib;

/// <summary>
/// declare a shell module
/// </summary>
[assembly: ShellModule("OrbitalShell-Kernel")]
[assembly: ModuleTargetPlateform(TargetPlatform.Any)]
[assembly: ModuleShellMinVersion("1.0.6")]
[assembly: ModuleAuthors("Orbital Shell team")]
namespace OrbitalShell.Kernel
{
    public class ModuleInfo { }
}