using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;

namespace OrbitalShell.Component.Shell.Hook
{
    /// <summary>
    /// modules hooks manager
    /// </summary>
    public class HookManager : IHookManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Supprimer les membres priv�s non lus", Justification = "<En attente>")]
        readonly IModuleSet _modules;
        readonly Dictionary<string, List<HookSpecification>> _hooks = new Dictionary<string, List<HookSpecification>>();
        readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public HookManager(IModuleSet modules)
        {
            _modules = modules;
        }

        object GetInstance(Type type)
        {
            if (_instances.TryGetValue(type, out var o))
                return o;
            o = Activator.CreateInstance(type, Array.Empty<object>());
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
            object owner = GetInstance(mi.DeclaringType);      // TODO: having no instance, we MUST use DI to share INSTANCES
            var hs = new HookSpecification(name, owner, mi);
            _hooks.AddOrReplace(name, hs);
        }

        readonly Dictionary<string, HookTriggerMode> _hooksTriggerState = new Dictionary<string, HookTriggerMode>();

        /// <summary>
        /// invoke hooks having the given name
        /// hooks crashes are catched here for kernel stability reasons
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="name">hook name</param>
        /// <param name="parameter">parameter</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        /// <param name="callBack">called after a hook has finished exec (param is the hook method object owner)</param>
        public AggregateHookResult<ResultType> InvokeHooks<ParameterType, ResultType>(
            CommandEvaluationContext context,
            string name,         
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
        )
        {
            if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.debug_enableHookTrace,false))
                context.Out.Echo(context.ShellEnv.Colors.Log + "[invoke hook: " + name + "](rdc) ");

            AggregateHookResult<ResultType> result = null;

            if (_hooks.TryGetValue(name, out var hookList) && hookList.Any())
            {
                result = new AggregateHookResult<ResultType>();

                foreach (var hook in hookList)
                {
                    try
                    {
                        var triggerStateKey = hook.Owner.ToString() + hookTriggerMode;
                        if (_hooksTriggerState.ContainsKey(triggerStateKey) && hookTriggerMode == HookTriggerMode.FirstTimeOnly)
                            break;
                        _hooksTriggerState.AddOrReplace(triggerStateKey, hookTriggerMode);

                        if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.debug_enableHookTrace))
                            context.Out.Echo(context.ShellEnv.Colors.Log + $"[hook '{hook.Name}' handled by: '{hook.Owner}.{hook.Method}'](rdc) ");

                        var hookResult = hook.Method.Invoke(hook.Owner, new object[] { context });
                        result.Results.Add((hook, (ResultType)hookResult ));
                        callBack?.Invoke(hook.Owner);
                    }
                    catch (Exception ex)
                    {
                        var m = $"hook '{hook.Owner}.{hook.Method}' has crashed: {ex.InnerException?.Message}";
                        context.Out.Errorln(m);
                        context.Logger.LogError(m);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="caller">caller</param>
        /// <param name="parameter">parameter</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        /// <param name="callBack">callback after hook</param>
        public AggregateHookResult<ResultType> InvokeHooks<ParameterType,ResultType>(
            CommandEvaluationContext context,
            Hooks name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
        )
            => InvokeHooks<ParameterType,ResultType>(context, name + "", parameter, hookTriggerMode, callBack);

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="parameter">parameter</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        /// <param name="callBack">callback after hook</param>
        public void InvokeHooks<ParameterType>(
            CommandEvaluationContext context,
            Hooks name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            => InvokeHooks<ParameterType, object>(context, name, parameter, hookTriggerMode, callBack);

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="parameter">parameter</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        /// <param name="callBack">callback after hook</param>
        public void InvokeHooks<ParameterType>(
            CommandEvaluationContext context,
            string name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            => InvokeHooks<ParameterType, object>(context, name + "", parameter, hookTriggerMode, callBack);

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        /// <param name="callBack">callback after hook</param>
        public void InvokeHooks(
            CommandEvaluationContext context,
            Hooks name,
            HookTriggerMode hookTriggerMode,
            Action<object> callBack
            )
            => InvokeHooks<object, object>(context, name, null, hookTriggerMode, callBack);

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        /// <param name="callBack">callback after hook</param>
        public void InvokeHooks(
            CommandEvaluationContext context,
            string name,
            HookTriggerMode hookTriggerMode,
            Action<object> callBack
            )
            => InvokeHooks<object, object>(context, name + "", null, hookTriggerMode, callBack);

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        public void InvokeHooks(
            CommandEvaluationContext context,
            Hooks name,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime
            )
            => InvokeHooks<object, object>(context, name, null, hookTriggerMode);

        /// <summary>
        /// invoke hooks having the given name
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="name">hook name</param>
        /// <param name="hookTriggerMode">how the hook should be tiggered</param>
        public void InvokeHooks(
            CommandEvaluationContext context,
            string name,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime
            )
            => InvokeHooks<object, object>(context, name + "", null, hookTriggerMode);

    }
}