using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public ModuleHookManager(ModuleSet modules)
        {
            _modules = modules;
        }

        object _GetInstance(Type type)
        {
            if (_instances.TryGetValue(type, out var o))
                return o;
            o = Activator.CreateInstance(type, new object[] { });
            _instances.Add(type, o);
            return o;
        }

        /// <summary>
        /// register a kernel-hook
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="mi">hook method info</param>
        public void RegisterHook(
            CommandEvaluationContext context,
            string name,
            MethodInfo mi)
        {
            object owner = _GetInstance(mi.DeclaringType);
            var hs = new HookSpecification(name, owner, mi);
            _hooks.AddOrReplace(name, hs);
        }

        /// <summary>
        /// invoke hooks having the given name
        /// hooks crashes are catched here for kernel stability reasons
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="name">hook name</param>
        /// <param name="callBack">called after a hook has finished exec (param is the hook method object owner)</param>
        public void InvokeHooks(
            CommandEvaluationContext context,
            string name,
            Action<object> callBack = null
        )
        {
            if (_hooks.TryGetValue(name, out var hookList))
            {
                foreach (var hook in hookList)
                {
                    try
                    {
                        hook.Method.Invoke(hook.Owner, new object[] { context });
                        callBack?.Invoke(hook.Owner);
                    }
                    catch (Exception ex)
                    {
                        context.CommandLineProcessor.LogError($"kook '{name}' crashed: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="name">hook name</param>
        public void InvokeHooks(
            CommandEvaluationContext context,
            Hooks name,
            Action<object> callBack = null
        )
        {
            InvokeHooks(context, name + "", callBack);
        }
    }
}