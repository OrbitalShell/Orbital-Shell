using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Lib;

/// <summary>
/// declare a shell module
/// </summary>
[assembly: ShellModule()]
[assembly: ModuleTargetPlateform(TargetPlatform.Any)]
[assembly: ModuleShellMinVersion("1.0.0")]
[assembly: ModuleAuthors("Orbital Shell team")]
namespace OrbitalShell.Kernel.Commands
{
    class ModuleInfo { }
}