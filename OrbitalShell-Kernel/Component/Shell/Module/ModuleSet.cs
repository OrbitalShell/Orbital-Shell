using System.Collections.Generic;

namespace OrbitalShell.Component.Shell.Module
{
    public class ModuleSet : Dictionary<string, ModuleSpecification> , IModuleSet
    {

    }

    public interface IModuleSet : IDictionary<string,ModuleSpecification> { }
}