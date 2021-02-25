using System.Collections.Generic;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Hook;

namespace OrbitalShell.Component.Shell.Module
{
    public interface IModuleManager
    {
        IModuleCommandManager ModuleCommandManager { get; }
        IHookManager ModuleHookManager { get; }
        IReadOnlyDictionary<string, ModuleSpecification> Modules { get; }

        Assembly GetLoadedModuleAssembly(string path);
        ModuleSpecification GetModuleByLowerPackageId(string lowerPackareId);
        ModuleSpecification GetModuleByName(string moduleName);
        bool IsModuleAssemblyLoaded(string path);
        ModuleSpecification RegisterModule(CommandEvaluationContext context, Assembly assembly);
        ModuleSpecification RegisterModule(CommandEvaluationContext context, string assemblyPath);
        ModuleSpecification UnregisterModule(CommandEvaluationContext context, string moduleName);
    }
}