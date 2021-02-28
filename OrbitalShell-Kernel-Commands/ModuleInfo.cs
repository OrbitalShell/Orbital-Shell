using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Lib;

// declare a shell module

[assembly: ShellModule("OrbitalShell-Kernel-Commands")]
[assembly: ModuleTargetPlateform(TargetPlatform.Any)]
[assembly: ModuleShellMinVersion("1.0.9")]
[assembly: ModuleAuthors("Orbital Shell team")]
namespace OrbitalShell.Kernel.Commands
{
    public class ModuleInfo { }
}
