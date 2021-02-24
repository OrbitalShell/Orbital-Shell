using System.Collections.Generic;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Module
{
    public interface IModuleManager
    {
        IModuleCommandManager ModuleCommandManager { get; }
        IModuleHookManager ModuleHookManager { get; }
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