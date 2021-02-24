using System;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Hook;

namespace OrbitalShell.Component.Shell.Module
{
    public interface IModuleHookManager
    {
        void InvokeHooks(CommandEvaluationContext context, Hooks name, HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime, Action<object> callBack = null);
        void InvokeHooks(CommandEvaluationContext context, string name, HookTriggerMode hookTriggerMode = HookTriggerMode.EachTime, Action<object> callBack = null);
        void RegisterHook(CommandEvaluationContext context, string name, MethodInfo mi);
    }
}