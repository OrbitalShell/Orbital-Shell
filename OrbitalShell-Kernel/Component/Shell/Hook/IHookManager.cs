using System;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Hook
{
    public interface IHookManager
    {
        AggregateHookResult<ResultType> InvokeHooks<ParameterType, ResultType>(
            CommandEvaluationContext context,
            Hooks name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            );

        AggregateHookResult<ResultType> InvokeHooks<ParameterType, ResultType>(
            CommandEvaluationContext context,
            string name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            );

        void InvokeHooks<ParameterType>(
            CommandEvaluationContext context,
            Hooks name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            );

        void InvokeHooks<ParameterType>(
            CommandEvaluationContext context,
            string name,
            ParameterType parameter = default,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            );

        void InvokeHooks(
            CommandEvaluationContext context,
            Hooks name,
            HookTriggerMode hookTriggerMode,
            Action<object> callBack
            );

        void InvokeHooks(
            CommandEvaluationContext context,
            string name,
            HookTriggerMode hookTriggerMode,
            Action<object> callBack
            );

        void InvokeHooks(
            CommandEvaluationContext context,
            Hooks name,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime
            );

        void InvokeHooks(
            CommandEvaluationContext context,
            string name,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime
            );

        void RegisterHook(CommandEvaluationContext context, string name, MethodInfo mi);
    }
}