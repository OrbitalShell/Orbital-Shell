using System.Collections.Generic;
using System.Reflection;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// modules hooks manager
    /// </summary>
    public class ModuleHookManager
    {
        readonly ModuleSet _modules;

        Dictionary<string, List<MethodInfo>> _hooks = new Dictionary<string, List<MethodInfo>>();

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
            MethodInfo mi,
            object caller)
        {
            
        }
    }
}