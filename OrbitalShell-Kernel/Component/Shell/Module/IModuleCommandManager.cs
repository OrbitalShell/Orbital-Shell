using System;
using System.Collections.Generic;
using System.Reflection;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Module
{
    public interface IModuleCommandManager
    {
        List<CommandSpecification> AllCommands { get; }
        IEnumerable<string> CommandDeclaringShortTypesNames { get; }
        IEnumerable<string> CommandDeclaringTypesAssemblyQualifiedNames { get; }
        IEnumerable<string> CommandDeclaringTypesNames { get; }
        IReadOnlyDictionary<string, List<CommandSpecification>> Commands { get; }

        string CheckAndNormalizeCommandName(string s);
        string CheckAndNormalizeCommandNamespace(string[] segments);
        CommandSpecification GetCommandSpecification(MethodInfo commandMethodInfo);
        int RegisterCommandClass(CommandEvaluationContext context, Type type);
        int RegisterCommandClass(CommandEvaluationContext context, Type type, bool registerAsModule);
        void RegisterCommandClass<T>(CommandEvaluationContext context);
        bool UnregisterCommand(CommandSpecification comSpec);
        ModuleSpecification UnregisterModuleCommands(CommandEvaluationContext context, string modulePackageId);
    }
}