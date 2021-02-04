using System.Collections.Generic;
using System.Reflection;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Lib;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// modules hooks manager
    /// </summary>
    public class ModuleHookManager
    {
        readonly ModuleSet _modules;

        Dictionary<string, List<HookSpecification>> _hooks = new Dictionary<string, List<HookSpecification>>();

        public ModuleHookManager(ModuleSet modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// register a kernel-hook
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="mi">hook method info</param>
        public void RegisterHook(
            CommandEvaluationContext context,
            string name,
            object owner,
            MethodInfo mi)
        {
            var hs = new HookSpecification(name, owner, mi);
            _hooks.AddOrReplace(name, hs);
        }
    }
}