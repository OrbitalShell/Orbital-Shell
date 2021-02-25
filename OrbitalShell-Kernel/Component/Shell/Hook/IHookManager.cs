using System;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Hook
{
    public interface IHookManager
    {
        AggregateHookResult<ResultType> InvokeHooks<CallerType, ParameterType, ResultType>(
            CommandEvaluationContext context, 
            Hooks name, 
            CallerType caller,
            ParameterType parameter = null,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            where CallerType : class
            where ResultType : class
            where ParameterType : class;

        AggregateHookResult<ResultType> InvokeHooks<CallerType, ParameterType, ResultType>(
            CommandEvaluationContext context, 
            string name, 
            CallerType caller,
            ParameterType parameter = null,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            where CallerType : class
            where ResultType : class
            where ParameterType : class;

        void InvokeHooks<CallerType, ParameterType>(
            CommandEvaluationContext context,
            Hooks name,
            CallerType caller,
            ParameterType parameter = null,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            where CallerType : class
            where ParameterType : class;

        void InvokeHooks<CallerType, ParameterType>(
            CommandEvaluationContext context,
            string name,
            CallerType caller,
            ParameterType parameter = null,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            where CallerType : class
            where ParameterType : class;

        void InvokeHooks<CallerType>(
            CommandEvaluationContext context,
            Hooks name,
            CallerType caller,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            where CallerType : class;

        void InvokeHooks<CallerType>(
            CommandEvaluationContext context,
            string name,
            CallerType caller,
            HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime,
            Action<object> callBack = null
            )
            where CallerType : class;

        void RegisterHook(CommandEvaluationContext context, string name, MethodInfo mi);
    }
}